using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelPerfectFollowCamera : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [Range(0f, 1f)]
    [SerializeField] private float followSpeed = 0.15f;
    [SerializeField] private float verticalOffset = 0f;

    [Header("Materials")]
    [SerializeField] private Material pixelMaterial;
    [SerializeField] private Material subPixelMaterial;

    [Header("Options")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private bool useSubPixel = true;

    private Camera cam;
    private float fixedHeight;
    private Vector3 horizontalOffset;

    private EntityQuery playerQuery;
    private bool hasEcsWorld;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target == null)
            return;

        fixedHeight = transform.position.y;

        horizontalOffset = new Vector3(
            transform.position.x - target.position.x,
            0f,
            transform.position.z - target.position.z
        );

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
            return;

        playerQuery = world.EntityManager.CreateEntityQuery(
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.ReadOnly<PlayerTag>()
        );
        hasEcsWorld = true;
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        FollowPlayer();

        Vector3 desiredPosition = new Vector3(
            target.position.x + horizontalOffset.x,
            fixedHeight,
            target.position.z + horizontalOffset.z
        ) + transform.up * verticalOffset;

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

    private void FollowPlayer()
    {
        if (!hasEcsWorld || playerQuery.IsEmpty)
            return;

        playerQuery.CompleteDependency();
        float3 playerPos = playerQuery.GetSingleton<LocalToWorld>().Position;
        float t = 1f - Mathf.Pow(1f - followSpeed, Time.deltaTime * 60f);
        target.position = Vector3.Lerp(target.position, (Vector3)playerPos, t);
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
