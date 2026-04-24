using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelPerfectFollowCamera : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;

    [Header("Materials")]
    [SerializeField] private Material pixelMaterial;
    [SerializeField] private Material subPixelMaterial;

    [Header("Options")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private bool useSubPixel = true;

    private Camera cam;

    private float fixedHeight;
    private Vector3 horizontalOffset;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target == null)
            return;

        fixedHeight = transform.position.y;

        Vector3 cameraPos = transform.position;
        Vector3 targetPos = target.position;

        // Offset solo en el plano horizontal del mundo
        horizontalOffset = new Vector3(
            cameraPos.x - targetPos.x,
            0f,
            cameraPos.z - targetPos.z
        );
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + horizontalOffset.x,
            fixedHeight,
            target.position.z + horizontalOffset.z
        );

        if (!snapToPixelGrid)
        {
            transform.position = desiredPosition;
            ResetSubPixelOffset();
            return;
        }

        float downScale = GetEffectiveDownScale();
        float renderWidth = Screen.width / downScale;
        float renderHeight = Screen.height / downScale;

        float worldUnitsPerPixel = (cam.orthographicSize * 2f) / renderHeight;

        Vector3 camRight = transform.right;
        Vector3 camUp = transform.up;

        float desiredX = Vector3.Dot(desiredPosition, camRight);
        float desiredY = Vector3.Dot(desiredPosition, camUp);

        float snappedX = Mathf.Round(desiredX / worldUnitsPerPixel) * worldUnitsPerPixel;
        float snappedY = Mathf.Round(desiredY / worldUnitsPerPixel) * worldUnitsPerPixel;

        float remainderX = desiredX - snappedX;
        float remainderY = desiredY - snappedY;

        float currentX = Vector3.Dot(desiredPosition, camRight);
        float currentY = Vector3.Dot(desiredPosition, camUp);

        Vector3 snappedPosition =
            desiredPosition
            + camRight * (snappedX - currentX)
            + camUp * (snappedY - currentY);

        transform.position = snappedPosition;

        ApplySubPixelOffset(remainderX, remainderY, worldUnitsPerPixel, renderWidth, renderHeight);
    }

    private void ApplySubPixelOffset(
        float remainderX,
        float remainderY,
        float worldUnitsPerPixel,
        float renderWidth,
        float renderHeight)
    {
        if (!useSubPixel || subPixelMaterial == null)
        {
            ResetSubPixelOffset();
            return;
        }

        Vector2 offset = new Vector2(
            remainderX / worldUnitsPerPixel / renderWidth,
            remainderY / worldUnitsPerPixel / renderHeight
        );

        subPixelMaterial.SetVector("_SubPixelOffset", offset);
    }

    private void ResetSubPixelOffset()
    {
        if (subPixelMaterial != null)
            subPixelMaterial.SetVector("_SubPixelOffset", Vector4.zero);
    }

    private float GetEffectiveDownScale()
    {
        if (pixelMaterial == null || !pixelMaterial.HasFloat("_Downscale"))
            return 1f;

        float value = pixelMaterial.GetFloat("_Downscale");

        if (Mathf.Approximately(value, 0f))
            return 1f;

        return 1f / value;
    }

    public void RecalculateOffset()
    {
        if (target == null)
            return;

        fixedHeight = transform.position.y;

        horizontalOffset = new Vector3(
            transform.position.x - target.position.x,
            0f,
            transform.position.z - target.position.z
        );
    }
}

// using System.Collections.Generic;
// using UnityEditor.Rendering.LookDev;
// using UnityEngine;

// [RequireComponent(typeof(Camera))]

// public class PixelPerfectFollowCamera : MonoBehaviour
// {
//     [SerializeField] private Transform target;
//     [SerializeField] private Material pixelMaterial;
//     [SerializeField] private Material subPixelMaterial;
//     [SerializeField] private bool snapToPixelGrid = true;
//     [SerializeField] private bool subPixelMovement = true;
   
//     private Vector3 initialOffset;
//     private Vector3 baseDesiredPosition;

//     [Header("Cameras with pixel perfect")]
//     [SerializeField] private Camera cam;
//     // [SerializeField] private List<Camera> secondaryCameras;

    

//     private void Awake()
//     {
   
//     }

//     private void Start()
//     {
//         if (target != null)
//         {
//             initialOffset = transform.position - target.position;
//             baseDesiredPosition = target.position + initialOffset;
//         }
//     }

//     private void LateUpdate()
//     {
//         if (target == null || cam == null)
//             return;

//         Vector3 desiredPosition = target.position + initialOffset;

//         if (!snapToPixelGrid)
//         {
//             transform.position = desiredPosition;
//             return;
//         }

//         float downScale = GetDownScale();
//         float renderHeight = Screen.height / downScale;

//         float worldUnitsPerPixel = (cam.orthographicSize * 2f) / renderHeight;

//         Vector3 camRight = transform.right;
//         Vector3 camUp = transform.up;
//         Vector3 camForward = transform.forward;

//         Vector3 delta = desiredPosition - baseDesiredPosition;

//         float dx = Vector3.Dot(delta, camRight);
//         float dy = Vector3.Dot(delta, camUp);
//         float dz = Vector3.Dot(delta, camForward);

//         float snappedDx = Mathf.Round(dx / worldUnitsPerPixel) * worldUnitsPerPixel;
//         float snappedDy = Mathf.Round(dy / worldUnitsPerPixel) * worldUnitsPerPixel;

//         Vector3 snappedWorldPosition =
//             baseDesiredPosition +
//             camRight * snappedDx +
//             camUp * snappedDy +
//             camForward * dz;

//         transform.position = snappedWorldPosition;

//         // foreach(Camera secondaryCamera in secondaryCameras)
//         // {
//         //     secondaryCamera.transform.position = snappedWorldPosition;
//         //     secondaryCamera.orthographicSize = cam.orthographicSize;
//         // }

       
//         Vector2 pixelAmount = new Vector2(Screen.width / downScale, Screen.height / downScale);
//         float pixelSize = cam.orthographicSize * 2f / pixelAmount.y;

//         float subPixelX = dx - snappedDx;
//         float subPixelY = dy - snappedDy;

//         ApplyOffset(new Vector2(subPixelX, subPixelY), pixelAmount, pixelSize);
       
//     }

//     private float GetDownScale()
//     {
//         if (pixelMaterial == null || !pixelMaterial.HasFloat("_Downscale"))
//             return 1f;

//         float downscale = pixelMaterial.GetFloat("_Downscale");

//         if (Mathf.Approximately(downscale, 0f))
//             return 1f;

//         return 1f / downscale;
//     }


//     public void RecalculateOffset()
//     {
//         if (target != null)
//         {
//             initialOffset = transform.position - target.position;
//             baseDesiredPosition = target.position + initialOffset;
//         }
//     }

//     void ApplyOffset(Vector2 subPixelDelta, Vector2 pixelAmount, float pixelSize)
//     {
//         if (!subPixelMovement || subPixelMaterial == null)
//             return;

//         Vector2 offsetAmt = new Vector2(
//             subPixelDelta.x / pixelSize / pixelAmount.x,
//             subPixelDelta.y / pixelSize / pixelAmount.y
//         );

//         subPixelMaterial.SetVector("_SubPixelOffset", offsetAmt);
//     }
// }