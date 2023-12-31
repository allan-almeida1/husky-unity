using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Twist = RosMessageTypes.Geometry.Twist;

public class HuskyROS : MonoBehaviour
{
    public string topicName = "cmd_vel";
    public float wheelRadius = 0.1651f;
    public float wheelSeparation = 0.512f;
    const float MAX_LIN_VEL = 0.2f;
    const float MAX_ANG_VEL = 1.5f;

    public GameObject huskyRobot;

    private ArticulationBody[] rightWheels;
    private ArticulationBody[] leftWheels;

    private float rightVelocity = 0.0f;
    private float leftVelocity = 0.0f;
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<Twist>(topicName, VelCallback);
        leftWheels = new ArticulationBody[2];
        rightWheels = new ArticulationBody[2];

        rightWheels[0] = huskyRobot.transform.Find("world/base_link/front_right_wheel").GetComponent<ArticulationBody>();
        rightWheels[1] = huskyRobot.transform.Find("world/base_link/rear_right_wheel").GetComponent<ArticulationBody>();
        leftWheels[0] = huskyRobot.transform.Find("world/base_link/front_left_wheel").GetComponent<ArticulationBody>();
        leftWheels[1] = huskyRobot.transform.Find("world/base_link/rear_left_wheel").GetComponent<ArticulationBody>();

        ChangeDrivesTypeToVelocity();
    }

    void FixedUpdate()
    {
        SetJointVelocities(rightVelocity, leftVelocity);
    }

    private void VelCallback(Twist velMsg)
    {
        float linVel = (float)velMsg.linear.x;
        float angVel = (float)velMsg.angular.z;

        rightVelocity = (linVel / wheelRadius) + (angVel * wheelSeparation / (2 * wheelRadius));
        leftVelocity = (linVel / wheelRadius) - (angVel * wheelSeparation / (2 * wheelRadius));
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
}