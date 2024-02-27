using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using Unity.Mathematics;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class GroundTruth : MonoBehaviour
{
    private ROSConnection ros;
    private PoseMsg pose;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>("/unity/husky/pose");
        pose = new PoseMsg();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 position = GameObject.Find("husky_origin").transform.position;
        Quaternion rotation = GameObject.Find("husky_origin").transform.rotation;
        pose.position = new PointMsg(position.x, position.y, position.z);
        pose.orientation = new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w);
        ros.Publish("/unity/husky/pose", pose);
    }
}
