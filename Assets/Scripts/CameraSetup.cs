using System;
using System.IO;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

public class CameraSetup : MonoBehaviour
{
    public enum LensShiftIgnore
    {
        None = 0x00,
        X    = 0x01,
        Y    = 0x10,
        Both = 0x11,
    }

    [SerializeField] private UnityEngine.GameObject m_ImageObject;
    [SerializeField] private LensShiftIgnore m_IgnoreLensShift = LensShiftIgnore.None;

    private Camera m_Camera;
    private float m_ImageRatio;
    private CameraParameters.Intrinsics m_IntrinsicParameters;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
        if (m_ImageObject == null)
        {
            m_ImageObject = m_Camera
                .transform.Find("Canvas")
                .transform.Find("Image")
                .gameObject;
        }
        Sprite sprite = m_ImageObject.GetComponent<Image>().sprite;
        string filePath = UnityEditor.AssetDatabase.GetAssetPath(sprite) + ".xmp";
        m_ImageRatio = (float)sprite.texture.width / sprite.texture.height;

        // Load and parse the XML
        System.Xml.Linq.XElement cameraElement = XDocument
            .Parse(File.ReadAllText(filePath))
            .Element("camera");
        System.Xml.Linq.XElement calibrationTag = cameraElement.Element("calibration");
        System.Xml.Linq.XElement extrinsicsTag = cameraElement.Element("extrinsics");

        // Extract calibration data
        int imageWidth = int.Parse(calibrationTag.Attribute("w").Value);
        int imageHeight = int.Parse(calibrationTag.Attribute("h").Value);
        Vector2 focalLengthPx = new Vector2(
            float.Parse(calibrationTag.Attribute("fx").Value),
            float.Parse(calibrationTag.Attribute("fy").Value)
        );
        Vector2 principalPoint = new Vector2(
            float.Parse(calibrationTag.Attribute("cx").Value),
            float.Parse(calibrationTag.Attribute("cy").Value)
        );

        // Extract extrinsics data (rotation & translation)
        float[] rotationValues = Array.ConvertAll(
            extrinsicsTag.Element("rotation").Value.Split(' '), float.Parse
        );
        float[,] rotation = new float[,]
        {
            { rotationValues[0], rotationValues[1], rotationValues[2] },
            { rotationValues[3], rotationValues[4], rotationValues[5] },
            { rotationValues[6], rotationValues[7], rotationValues[8] },
        };
        float[] translationValues = Array.ConvertAll(
            extrinsicsTag.Element("translation").Value.Split(' '), float.Parse
        );
        Vector3 translation = new Vector3(translationValues[0], translationValues[1], translationValues[2]);

        // Set camera parameters
        SetIntrinsicParameters(imageWidth, imageHeight, focalLengthPx, principalPoint);
        SetExtrinsicParameters(rotation, translation);

        float xmpRatio = (float)imageWidth / imageHeight;
        if (Mathf.Abs(m_ImageRatio - xmpRatio) > 0.01f)
        {
            m_Camera.transform.Rotate(0f, 0f, 90f, Space.Self);
            m_Camera.sensorSize = new Vector2(
                m_Camera.sensorSize.y,
                m_Camera.sensorSize.x
            );
        }
        FixProportions();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            FixProportions();
        }
    }

    private void SetIntrinsicParameters(
            int i_ImageWidth, int i_ImageHeight,
            Vector2 i_FocalLengthPx, Vector2 i_PrincipalPoint, float i_SensorX = 35f)
    {
        m_IntrinsicParameters = CameraParameters.ComputeIntrinsics(
            i_ImageWidth, i_ImageHeight, i_FocalLengthPx, i_PrincipalPoint
        );

        if (((int)m_IgnoreLensShift & (int)LensShiftIgnore.X) != 0)
        {
            m_IntrinsicParameters.lensShift.x = 0f;
        }
        if (((int)m_IgnoreLensShift & (int)LensShiftIgnore.Y) != 0)
        {
            m_IntrinsicParameters.lensShift.y = 0f;
        }

        // Apply parameters
        m_Camera.focalLength = m_IntrinsicParameters.focalLength;
        m_Camera.sensorSize = m_IntrinsicParameters.sensorSize;
        m_Camera.lensShift = m_IntrinsicParameters.lensShift;
    }

    private void SetExtrinsicParameters(float[,] i_RotationMatrix, Vector3 i_Translation)
    {
        CameraParameters.Extrinsics parameters = CameraParameters.ComputeExtrinsics(
            i_RotationMatrix, i_Translation
        );
        m_Camera.transform.position = parameters.position;
        m_Camera.transform.rotation = Quaternion.Euler(parameters.rotation);
    }

    private void FixProportions()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        Vector2 baseSensorSize = m_IntrinsicParameters.sensorSize;

        // Scale sensor matching image width
        m_Camera.sensorSize = new Vector2(screenRatio, 1f) * baseSensorSize.y;

        // Scale image
        m_ImageObject.GetComponent<RectTransform>().localScale = Vector3.one
            * (m_ImageRatio / screenRatio);

        // Scale focal lenght accordingly
        m_Camera.focalLength = m_IntrinsicParameters.focalLength * m_ImageRatio;
    }
}
