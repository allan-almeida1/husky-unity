using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class GroundTruth : MonoBehaviour
{
    private ROSConnection ros;
    private PoseMsg pose;
    private TwistMsg twist;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>("/unity/husky/pose");
        ros.RegisterPublisher<TwistMsg>("/unity/husky/twist");

        pose = new PoseMsg();
        twist = new TwistMsg();

        // Initialize last position and rotation
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // Get current position and rotation
        Vector3 currentPosition = GameObject.Find("husky_origin").transform.position;
        Quaternion currentRotation = GameObject.Find("husky_origin").transform.rotation;

        // Calculate position and rotation deltas
        Vector3 positionDelta = currentPosition - lastPosition;
        Quaternion rotationDelta = currentRotation * Quaternion.Inverse(lastRotation);

        // Calculate linear velocity (change in position over time)
        float deltaTime = Time.fixedDeltaTime; // Time between fixed updates

        // Convert position delta to local frame
        Vector3 localPositionDelta = transform.InverseTransformDirection(positionDelta);

        // Calculate linear velocity in local frame
        Vector3 linearVelocity = localPositionDelta / deltaTime;        

        // Calculate angular velocity (change in rotation) in local frame
        // Use Quaternion to calculate angle
        float angle;
        Vector3 axis;
        rotationDelta.ToAngleAxis(out angle, out axis);

        // Normalize axis and calculate angular velocity
        Vector3 localAngularVelocity = (axis * angle * Mathf.Deg2Rad) / deltaTime;

        // Update the last position and rotation
        lastPosition = currentPosition;
        lastRotation = currentRotation;

        // Publish the pose and twist messages
        pose.position = new PointMsg(currentPosition.x, currentPosition.y, currentPosition.z);
        pose.orientation = new QuaternionMsg(currentRotation.x, currentRotation.y, currentRotation.z, currentRotation.w);
        ros.Publish("/unity/husky/pose", pose);

        twist.linear = new Vector3Msg(linearVelocity.z, linearVelocity.y, linearVelocity.x);
        twist.angular = new Vector3Msg(localAngularVelocity.x, localAngularVelocity.z, -localAngularVelocity.y);
        ros.Publish("/unity/husky/twist", twist);
    }
}
