using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class GroundTruth : MonoBehaviour
{
    private ROSConnection ros;
    private PoseMsg pose;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>("/unity/husky/pose");
        ros.RegisterPublisher<TwistMsg>("/unity/husky/twist");

        pose = new PoseMsg();
        // twist = new TwistMsg();

        // Initialize last position and rotation
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // Get current position and rotation
        Vector3 currentPosition = GameObject.Find("husky_origin").transform.position;
        Vector3<FLU> currentROSPosition = currentPosition.To<FLU>();
        Quaternion currentRotation = GameObject.Find("husky_origin").transform.rotation;
        Quaternion<FLU> currentROSRotation = currentRotation.To<FLU>();

        // Calculate position and rotation deltas
        Vector3 positionDelta = currentPosition - lastPosition;
        Quaternion rotationDelta = currentRotation * Quaternion.Inverse(lastRotation);
        //Debug.Log("PositionDelta 0: " +  Mathf.Abs(positionDelta[0]) + "PositionDelta 1: " + positionDelta[1] + "PositionDelta 2: " + positionDelta[2]);

    
        // Calculate linear velocity (change in position over time)
        float deltaTime = Time.fixedDeltaTime; // Time between fixed updates

        // Convert position delta to local frame
        Vector3 localPositionDelta = transform.InverseTransformDirection(positionDelta);

        localPositionDelta[0]=Mathf.Abs(localPositionDelta[0]);
        localPositionDelta[1]=Mathf.Abs(localPositionDelta[1]);
        localPositionDelta[2]=Mathf.Abs(localPositionDelta[2]);
        
        // Calculate linear velocity in local frame
        Vector3 linearVelocity = localPositionDelta / deltaTime;        

        // Calculate angular velocity (change in rotation) in local frame
        // Use Quaternion to calculate angle
        float angle;
        Vector3 axis;
        rotationDelta.ToAngleAxis(out angle, out axis);

        // Normalize axis and calculate angular velocity
        Vector3 localAngularVelocity = axis * angle * Mathf.Deg2Rad / deltaTime;

        // Update the last position and rotation
        lastPosition = currentPosition;
        lastRotation = currentRotation;

        // Publish the pose and twist messages
        // pose.position = new PointMsg(currentPosition.x, currentPosition.y, currentPosition.z);
        // pose.orientation = new QuaternionMsg(currentRotation.x, currentRotation.y, currentRotation.z, currentRotation.w);
        pose.position = new PointMsg(currentROSPosition.x, currentROSPosition.y, currentROSPosition.z);
        pose.orientation = new QuaternionMsg(currentROSRotation.x, currentROSRotation.y, currentROSRotation.z, currentROSRotation.w);
        ros.Publish("/unity/husky/pose", pose);

        Vector3Msg linear = new Vector3Msg(linearVelocity.z, linearVelocity.y, linearVelocity.x);
        Vector3Msg angular = new Vector3Msg(localAngularVelocity.x, localAngularVelocity.z, -localAngularVelocity.y);
        ros.Publish("/unity/husky/twist", new TwistMsg(linear, angular));
    }
}
