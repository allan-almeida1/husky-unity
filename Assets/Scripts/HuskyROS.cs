using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Twist = RosMessageTypes.Geometry.TwistMsg;
using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;
// using Mathf.Numerics.LinearAlgebra;

public class HuskyROS : MonoBehaviour
{
    public string topicName = "cmd_vel";
    public string topicName1 = "/unity/husky/twist";

    public string huskyTwistTopic = "/unity/husky/twist"; // Se não for esse, colocar o correto
    public float wheelRadius = 0.165f;
    public float wheelSeparation = 0.555f;
    const float MAX_LIN_VEL = 0.2f;
    const float MAX_ANG_VEL = 1.5f;

    public GameObject huskyRobot;

    private ArticulationBody[] rightWheels;
    private ArticulationBody[] leftWheels;

    private JointStateMsg jointStateMsg;

    private float rightVelocity = 0.0f;
    private float leftVelocity = 0.0f;
    private PIDController leftPID;
    private PIDController rightPID;

    private TwistMsg twist_robot;

    // Definição da pseudo-inversa de A Modelo Skid-Steer Simetrico
    // alfa=wheelRadius = 0.165f;
    // xicr=wheelSeparation = 0.555f;
    public float a11 = -0.0000f, a12 = 6.0606f, a13 = -3.3636f;
    public float a21 = -0.0000f, a22 = 6.0606f, a23 =  3.3636f;
 
    void Start()
    {

        ROSConnection.GetOrCreateInstance().Subscribe<Twist>(topicName, VelCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<Twist>(huskyTwistTopic, TwistCallback);
        ROSConnection.GetOrCreateInstance().RegisterPublisher<JointStateMsg>("/unity/husky/joint_states");
        jointStateMsg = new JointStateMsg();
        jointStateMsg.name = new string[] {"left_side_wheels_joint", "right_side_wheels_joint"};

        leftWheels = new ArticulationBody[2];
        rightWheels = new ArticulationBody[2];

        rightWheels[0] = huskyRobot.transform.Find("world/base_link/front_right_wheel").GetComponent<ArticulationBody>();
        rightWheels[1] = huskyRobot.transform.Find("world/base_link/rear_right_wheel").GetComponent<ArticulationBody>();
        leftWheels[0] = huskyRobot.transform.Find("world/base_link/front_left_wheel").GetComponent<ArticulationBody>();
        leftWheels[1] = huskyRobot.transform.Find("world/base_link/rear_left_wheel").GetComponent<ArticulationBody>();

        rightPID = new PIDController(0.5f, 0.2f, 0.001f);
        leftPID = new PIDController(0.5f, 0.2f, 0.001f);

        ChangeDrivesTypeToVelocity();
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        //Debug.Log("v_robot: " + (float)twist_robot.linear.x + " w_robot: " + (float)twist_robot.linear.z);

        // Get the current joint velocities
        float rightJointVel = rightWheels[0].jointVelocity[0];
        float leftJointVel = leftWheels[0].jointVelocity[0];

        // Print to console
        //Debug.Log("Right velocity SP: " + rightVelocity + " Left velocity SP: " + leftVelocity);
        //Debug.Log("Right joint velocity: " + rightJointVel + " Left joint velocity: " + leftJointVel);

        // Publish current joint states
        jointStateMsg.velocity = new double[] {leftJointVel, rightJointVel};
        ROSConnection.GetOrCreateInstance().Publish("/unity/husky/joint_states", jointStateMsg);

        // Calculate the PID output
        float rightPIDOutput = rightPID.Calculate(rightVelocity, rightJointVel, deltaTime);
        float leftPIDOutput = leftPID.Calculate(leftVelocity, leftJointVel, deltaTime);

        // Set the joint velocities
        //SetJointVelocities(rightPIDOutput + rightVelocity, leftPIDOutput + leftVelocity);

        // SetJointVelocities(rightPIDOutput, leftPIDOutput);
        SetJointVelocities(rightVelocity, leftVelocity);
    }

    private void VelCallback(Twist velMsg)
    {
        float linVel = (float)velMsg.linear.x;
        float angVel = (float)velMsg.angular.z;

        // Differential robot 
        rightVelocity = (linVel / wheelRadius) + (angVel * wheelSeparation / (2 * wheelRadius));
        leftVelocity = (linVel / wheelRadius) - (angVel * wheelSeparation / (2 * wheelRadius));
        
        // output to console
        Debug.Log("Right velocity Dif: " + rightVelocity + " Left velocity Dif: " + leftVelocity);
        

        // Sid-Steer Robot
        leftVelocity =  a12 * linVel + a13 * angVel;
        rightVelocity =  a22 * linVel + a23 * angVel;

        //output to console
        Debug.Log("Right velocity Skid: " + rightVelocity + " Left velocity Skid: " + leftVelocity);
    }
    
    private void ChangeDrivesTypeToVelocity()
    {
        var frontRightDrive = rightWheels[0].xDrive;
        var rearRightDrive = rightWheels[1].xDrive;
        var frontLeftDrive = leftWheels[0].xDrive;
        var rearLeftDrive = leftWheels[1].xDrive;

        frontRightDrive.driveType = ArticulationDriveType.Velocity;
        rearRightDrive.driveType = ArticulationDriveType.Velocity;
        frontLeftDrive.driveType = ArticulationDriveType.Velocity;
        rearLeftDrive.driveType = ArticulationDriveType.Velocity;

        rightWheels[0].xDrive = frontRightDrive;
        rightWheels[1].xDrive = rearRightDrive;
        leftWheels[0].xDrive = frontLeftDrive;
        leftWheels[1].xDrive = rearLeftDrive;
    }

    private void SetJointVelocities(float rightVel, float leftVel)
    {
        var frontRightDrive = rightWheels[0].xDrive;
        var rearRightDrive = rightWheels[1].xDrive;
        var frontLeftDrive = leftWheels[0].xDrive;
        var rearLeftDrive = leftWheels[1].xDrive;

        frontRightDrive.targetVelocity = rightVel * Mathf.Rad2Deg;
        rearRightDrive.targetVelocity = rightVel * Mathf.Rad2Deg;
        frontLeftDrive.targetVelocity = leftVel * Mathf.Rad2Deg;
        rearLeftDrive.targetVelocity = leftVel * Mathf.Rad2Deg;

        rightWheels[0].xDrive = frontRightDrive;
        rightWheels[1].xDrive = rearRightDrive;
        leftWheels[0].xDrive = frontLeftDrive;
        leftWheels[1].xDrive = rearLeftDrive;
    }

    private void TwistCallback(Twist twistMsg)
    {
        // float linVel = (float)twistMsg.linear.x;
        // float angVel = (float)twistMsg.angular.z;
        // Debug.Log("LinVel: " + linVel + " AngVel: " + angVel);
    }
}
