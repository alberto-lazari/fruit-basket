#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

public class UiCameraSetup : MonoBehaviour
{
    private static readonly string CameraParametersDirectory = Path
        .Combine(Application.streamingAssetsPath, "CameraParameters");

    [SerializeField] private Camera? m_UiCamera;
    [SerializeField] private Camera? m_3dCamera;

    [SerializeField] private Material? m_UiCameraMaterial;
    [SerializeField] private GameObject? m_ImageObject;
    [SerializeField] private List<Sprite> m_BackgroundImages = new List<Sprite>();

    [SerializeField] private bool m_IgnoreLensShiftX = false;
    [SerializeField] private bool m_IgnoreLensShiftY = false;

    private Image? m_ImageComponent;
    private RectTransform? m_ImageTransform;
    private float? m_ImageRatio;
    private CameraParameters.Intrinsics? m_IntrinsicParameters;
    private int m_CurrentImageIndex = 0;

    public void MoveLeft()
    {
        // Update current image
        m_CurrentImageIndex = (m_CurrentImageIndex + m_BackgroundImages.Count - 1)
            % m_BackgroundImages.Count;

        // Setup cameras for the new image
        Setup(m_UiCamera);
        Setup(m_3dCamera);
    }

    public void MoveRight()
    {
        // Update current image
        m_CurrentImageIndex = (m_CurrentImageIndex + 1)
            % m_BackgroundImages.Count;

        // Setup cameras for the new image
        Setup(m_UiCamera);
        Setup(m_3dCamera);
    }

    private void Start()
    {
        if (m_UiCamera == null)
        {
            Debug.LogError("UI Camera not set");
            return;
        }
        if (m_3dCamera == null)
        {
            Debug.LogError("3D Camera not set");
            return;
        }

        if (m_ImageObject == null) m_ImageObject = m_UiCamera
            .transform.Find("Canvas")
            .transform.Find("Image")
            .gameObject;

        // Cache components
        m_ImageComponent = m_ImageObject.GetComponent<Image>();
        m_ImageTransform = m_ImageObject.GetComponent<RectTransform>();

        if (m_BackgroundImages.Count < 1)
        {
            Debug.LogError("No background image set");
            return;
        }

        // Setup both cameras
        Setup(m_UiCamera);
        Setup(m_3dCamera);

        InitCameraShader();
    }

    private void InitCameraShader()
    {
        if (m_UiCamera == null || m_UiCameraMaterial == null) return;

        m_UiCamera.targetTexture?.Release();
        m_UiCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        m_UiCameraMaterial.mainTexture = m_UiCamera.targetTexture;
    }

    private void Setup(in Camera i_Camera)
    {
        Sprite imageSprite = m_BackgroundImages[m_CurrentImageIndex];
        if (m_ImageComponent != null) m_ImageComponent.sprite = imageSprite;

        if (!Directory.Exists(CameraParametersDirectory))
        {
            Debug.LogError($"Directory {CameraParametersDirectory} does not exist.");
            return;
        }
        string imageName = imageSprite.name;
        string xmpFilePath = Directory.EnumerateFiles(CameraParametersDirectory, "*.xmp")
            .Where(file => Regex.IsMatch(
                Path.GetFileNameWithoutExtension(file),
                $"^{Regex.Escape(imageName)}")
            )
            .First();

        m_ImageRatio = (float)imageSprite.texture.width / imageSprite.texture.height;

        XElement cameraElement = XDocument
            .Parse(File.ReadAllText(xmpFilePath))
            .Element("camera");

        // Set camera parameters from XML values
        SetExtrinsicParameters(i_Camera, cameraElement.Element("extrinsics"));
        SetIntrinsicParameters(i_Camera, cameraElement.Element("calibration"));

        FixProportions(i_Camera);
    }

    private void SetIntrinsicParameters(in Camera i_Camera, in XElement i_CalibrationTag)
    {
        // Extract calibration data
        float? sensorX = null;
        XAttribute? ccwidth = i_CalibrationTag.Attribute("ccwidth");
        if (ccwidth != null) sensorX = float.Parse(ccwidth.Value);
        int imageWidth = int.Parse(i_CalibrationTag.Attribute("w").Value);
        int imageHeight = int.Parse(i_CalibrationTag.Attribute("h").Value);
        Vector2 focalLengthPx = new Vector2(
            float.Parse(i_CalibrationTag.Attribute("fx").Value),
            float.Parse(i_CalibrationTag.Attribute("fy").Value)
        );
        Vector2 principalPoint = new Vector2(
            float.Parse(i_CalibrationTag.Attribute("cx").Value),
            float.Parse(i_CalibrationTag.Attribute("cy").Value)
        );

        // Compute parameters
        m_IntrinsicParameters = CameraParameters.ComputeIntrinsics(
            imageWidth, imageHeight, focalLengthPx, principalPoint, sensorX
        );

        if (m_IgnoreLensShiftX) m_IntrinsicParameters.lensShift.x = 0f;
        if (m_IgnoreLensShiftY) m_IntrinsicParameters.lensShift.y = 0f;

        // Apply parameters
        i_Camera.focalLength = m_IntrinsicParameters.focalLength;
        i_Camera.sensorSize = m_IntrinsicParameters.sensorSize;
        i_Camera.lensShift = m_IntrinsicParameters.lensShift;

        if (m_ImageRatio == null) return;

        float imageRatio = (float)m_ImageRatio;
        float xmpRatio = (float)imageWidth / imageHeight;
        if (Mathf.Abs(imageRatio - xmpRatio) > 0.01f)
        {
            // Ratios differ => image is vertical
            // Zephyr processed it by rotating -90 deg
            i_Camera.transform.Rotate(0f, 0f, 90f, Space.Self);
            i_Camera.sensorSize = new Vector2(
                i_Camera.sensorSize.y,
                i_Camera.sensorSize.x
            );
        }
    }

    private void SetExtrinsicParameters(in Camera i_Camera, in XElement i_ExtrinsicsTag)
    {
        // Extract extrinsics data (rotation & translation)
        float[] rotationValues = Array.ConvertAll(
            i_ExtrinsicsTag.Element("rotation").Value.Split(' '), float.Parse
        );
        float[,] rotation = new float[,]
        {
            { rotationValues[0], rotationValues[1], rotationValues[2] },
            { rotationValues[3], rotationValues[4], rotationValues[5] },
            { rotationValues[6], rotationValues[7], rotationValues[8] },
        };
        float[] translationValues = Array.ConvertAll(
            i_ExtrinsicsTag.Element("translation").Value.Split(' '), float.Parse
        );
        Vector3 translation = new Vector3(translationValues[0], translationValues[1], translationValues[2]);

        // Compute and set
        CameraParameters.Extrinsics parameters = CameraParameters.ComputeExtrinsics(
            rotation, translation
        );
        i_Camera.transform.position = parameters.position;
        i_Camera.transform.rotation = Quaternion.Euler(parameters.rotation);
    }

    private void FixProportions(in Camera i_Camera)
    {
        if (m_ImageTransform == null || m_ImageRatio == null || m_IntrinsicParameters == null)
            return;

        float imageRatio = (float)m_ImageRatio;
        float screenRatio = (float)Screen.width / Screen.height;
        Vector2 baseSensorSize = m_IntrinsicParameters.sensorSize;

        // Scale sensor matching image width
        i_Camera.sensorSize = new Vector2(screenRatio, 1f) * baseSensorSize.y;

        // Scale image
        m_ImageTransform.localScale = Vector3.one * (imageRatio / screenRatio);

        // Scale focal lenght accordingly
        i_Camera.focalLength = m_IntrinsicParameters.focalLength * imageRatio;
    }
}
