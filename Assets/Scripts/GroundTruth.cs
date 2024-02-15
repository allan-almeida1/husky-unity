using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GroundTruth : MonoBehaviour
{
    GameObject base_link;
    // Start is called before the first frame update
    void Start()
    {
        base_link = GameObject.Find("world/base_link");
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 position = base_link.transform.position;
        Quaternion rotation = base_link.transform.rotation;
        Vector3 velocity = base_link.GetComponent<ArticulationBody>().velocity;
        Vector3 angular_velocity = base_link.GetComponent<ArticulationBody>().angularVelocity;
        float linear_velocity = math.sqrt(math.pow(velocity.x, 2) + math.pow(velocity.z, 2));
        float angular_velocity_z = angular_velocity.y;
        Debug.Log("Linear Velocity: " + linear_velocity + " m/s");
        Debug.Log("Angular Velocity: " + angular_velocity_z + " rad/s");
    }
}
