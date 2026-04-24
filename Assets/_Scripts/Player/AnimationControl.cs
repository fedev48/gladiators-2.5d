using UnityEngine;

public class AnimationControl : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private Animator animator;

    [Header("rotation")]
    [SerializeField] private float visualPitch = 34f;

    [Header("movement")]
    [SerializeField] private float speedSmooth = 12f;
    [SerializeField] private float runEnterThreshold = 0.22f;
    [SerializeField] private float runExitThreshold = 0.12f;
    [SerializeField] private float facingThreshold = 0.08f;

    private Camera mainCamera;
    private Vector3 previousPosition;
    private Vector3 smoothedVelocity;
    private bool isRunning;

    void Start()
    {
        mainCamera = Camera.main;
        previousPosition = transform.position;
    }

    void LateUpdate()
    {
        if (visualsTransform == null || animator == null) return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        Vector3 rawVelocity = (transform.position - previousPosition) / dt;
        previousPosition = transform.position;

        float lerpFactor = 1f - Mathf.Exp(-speedSmooth * dt);
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rawVelocity, lerpFactor);

        visualsTransform.rotation = Quaternion.Euler(
            visualPitch,
            mainCamera.transform.rotation.eulerAngles.y,
            0f
        );

        float flatSpeed = new Vector2(smoothedVelocity.x, smoothedVelocity.z).magnitude;

        if (!isRunning && flatSpeed > runEnterThreshold)
        {
            isRunning = true;
        }
        else if (isRunning && flatSpeed < runExitThreshold)
        {
            isRunning = false;
        }

        animator.SetBool("isRunning", isRunning);

        if (!isRunning) return;

        Vector3 localVelocity = mainCamera.transform.InverseTransformDirection(smoothedVelocity);

        if (localVelocity.x > facingThreshold)
        {
            SetFacingRight(true);
        }
        else if (localVelocity.x < -facingThreshold)
        {
            SetFacingRight(false);
        }
    }

    private void SetFacingRight(bool facingRight)
    {
        Vector3 scale = visualsTransform.localScale;
        float absX = Mathf.Abs(scale.x);
        scale.x = facingRight ? absX : -absX;
        visualsTransform.localScale = scale;
    }
}