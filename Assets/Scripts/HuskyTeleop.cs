using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HuskyTeleop : MonoBehaviour
{
    public float wheelRadius = 0.1651f;
    public float wheelSeparation = 0.512f;
    const float MAX_LIN_VEL = 0.2f;
    const float MAX_ANG_VEL = 1.5f;

    public GameObject huskyRobot;

    private ArticulationBody[] rightWheels;
    private ArticulationBody[] leftWheels;

    private float linVel;
    private float angVel;
    private float rightVelocity;
    private float leftVelocity;
    // Start is called before the first frame update
    void Start()
    {
        leftWheels = new ArticulationBody[2];
        rightWheels = new ArticulationBody[2];

        rightWheels[0] = huskyRobot.transform.Find("world/base_link/front_right_wheel").GetComponent<ArticulationBody>();
        rightWheels[1] = huskyRobot.transform.Find("world/base_link/rear_right_wheel").GetComponent<ArticulationBody>();
        leftWheels[0] = huskyRobot.transform.Find("world/base_link/front_left_wheel").GetComponent<ArticulationBody>();
        leftWheels[1] = huskyRobot.transform.Find("world/base_link/rear_left_wheel").GetComponent<ArticulationBody>();

        ChangeDrivesTypeToVelocity();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        linVel = 0.0f;
        angVel = 0.0f;

        if (Gamepad.all[0].buttonSouth.isPressed)
        {


            linVel = Gamepad.all[0].leftStick.y.ReadValue() * MAX_LIN_VEL;
            angVel = -Gamepad.all[0].leftStick.x.ReadValue() * MAX_ANG_VEL;

        }

        rightVelocity = (linVel / wheelRadius) + (angVel * wheelSeparation / (2 * wheelRadius));
        leftVelocity = (linVel / wheelRadius) - (angVel * wheelSeparation / (2 * wheelRadius));

        SetJointVelocities(rightVelocity, leftVelocity);
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
