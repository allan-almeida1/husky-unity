using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class CameraROS : MonoBehaviour
{

    Camera myCam;
    ROSConnection ros;
    // Start is called before the first frame update
    void Start()
    {
        myCam = GetComponent<Camera>();
        myCam.enabled = true;
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>("/camera/image");
        Debug.Log("CameraROS started");
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(GetImage());
    }

    IEnumerator GetImage()
    {
        // Wait for end of frame
        yield return new WaitForEndOfFrame();

        // Get the camera image
        Texture2D tex = new Texture2D(myCam.pixelWidth, myCam.pixelHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, myCam.pixelWidth, myCam.pixelHeight), 0, 0);
        tex.Apply();

        // Convert to ROS message
        ImageMsg imageMsg = tex.ToImageMsg(new HeaderMsg());
        imageMsg.encoding = "rgba8";
        ros.Publish("/camera/image", imageMsg);
        Destroy(tex);
    }
}
