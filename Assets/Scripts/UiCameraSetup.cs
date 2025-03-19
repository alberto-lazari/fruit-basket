using System.Collections.Generic;

using UnityEngine;

public class UiCameraSetup : MonoBehaviour {

    [SerializeField] private Material m_CameraMaterial;

    private void Start () {
        Camera camera = GetComponent<Camera>();
        if (camera.targetTexture != null)
        {
            camera.targetTexture.Release();
        }
        camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        m_CameraMaterial.mainTexture = camera.targetTexture;
    }
}
