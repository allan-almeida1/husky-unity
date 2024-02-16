using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;
using UnityEngine.UI;

public class CameraROSv2 : MonoBehaviour
{
    private RenderTexture rt;
    private ROSConnection ros;
    private Camera cam;
    private Texture2D image;
    private Texture2D result;
    public int smallTextureWidth = 320;
    public int smallTextureHeight = 176;
    public RawImage rawImage;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        rt = new RenderTexture(960, 528, 24);
        image = new Texture2D(rt.width, rt.height);
        result = new Texture2D(smallTextureWidth, smallTextureHeight);
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>("/camera/image");
    }

    // Update is called once per frame
    void Update()
    {
        cam.targetTexture = rt;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentRT;

        ImageMsg imageMsg = image.ToImageMsg(new HeaderMsg());
        imageMsg.encoding = "rgba8";
        ros.Publish("/camera/image", imageMsg);
        Texture2D smallImage = ResizeTexture(image, smallTextureWidth, smallTextureHeight);
        rawImage.texture = smallImage;
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
}
