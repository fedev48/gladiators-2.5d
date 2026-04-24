using UnityEngine;
using UnityEngine.Rendering;

public class Camera2FollowSize : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera camera2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
        camera2.orthographicSize = mainCamera.orthographicSize;
    }
}
