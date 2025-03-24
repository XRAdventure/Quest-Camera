using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PassthroughCameraSamples;
using ZXing;
using Meta.XR;
public class QrCodeDetection : MonoBehaviour
{
    [SerializeField] private WebCamTextureManager webCamManager;
    [SerializeField] private int scanFrameFrequency = 10;
    [SerializeField] private EnvironmentRaycastManager raycastManager;

    private BarcodeReader barcodeReader = new BarcodeReader();
    private bool isCameraReady;

    [System.Serializable]
    public struct QrCodeTarget
    {
        public string QrCodeContent;
        public Transform Object;
    }

    [SerializeField] private List<QrCodeTarget> qrCodeTargets = new List<QrCodeTarget>();
    private Dictionary<string, Transform> qrCodeTargetsDic = new Dictionary<string, Transform>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        foreach (var qrCodeTarget in qrCodeTargets)
        {
            qrCodeTargetsDic.Add(qrCodeTarget.QrCodeContent, qrCodeTarget.Object);
        }

        while (webCamManager.WebCamTexture == null)
        {
            yield return null;
        }

        isCameraReady = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCameraReady && Time.frameCount % scanFrameFrequency == 0)
        {
            WebCamTexture webCamTexture = webCamManager.WebCamTexture;

            if (webCamTexture == null || webCamTexture.width <= 16 || webCamTexture.height <= 16)
            {
                return;
            }

            Color32[] camPixels = webCamTexture.GetPixels32();

            Result result = barcodeReader.Decode(camPixels, webCamTexture.width, webCamTexture.height);

            if(result != null) 
            {
                if(qrCodeTargetsDic.TryGetValue(result.Text, out Transform obj)) 
                {
                    Vector2Int qrCodeCenter = GetQrCodeCenter(result.ResultPoints, webCamTexture.height);
                    Pose pose = ConvertScreenPointToWorldPoint(qrCodeCenter);
                    obj.SetPositionAndRotation(pose.position, pose.rotation);
                }
            }
        }
    }

    private Vector2Int GetQrCodeCenter(ResultPoint[] resultPoints, int textureHeight) 
    {
        if(resultPoints == null || resultPoints.Length == 0) 
        {
            return Vector2Int.zero;
        }

        float sumX = 0;
        float sumY = 0;

        foreach (var point in resultPoints)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        float x = sumX / resultPoints.Length;
        float y = sumY / resultPoints.Length;

        int centerX = Mathf.RoundToInt(x);
        int centerY = Mathf.RoundToInt(textureHeight - y);

        return new Vector2Int(centerX, centerY);
    }

    private Pose ConvertScreenPointToWorldPoint(Vector2Int screenPoint) 
    {
        Ray ray = PassthroughCameraUtils.ScreenPointToRayInWorld(webCamManager.Eye, screenPoint);
        if(raycastManager.Raycast(ray, out EnvironmentRaycastHit hitInfo)) 
        {
            Pose pose = new Pose(hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            return pose;
        }
        return Pose.identity;
    }
}
