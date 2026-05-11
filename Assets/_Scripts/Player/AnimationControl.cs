using UnityEngine;
using UnityEngine.AI;

public class AnimationControl : MonoBehaviour
{
    private enum FacingDirection { Front, Back, Side }

    [Header("References")]
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Rotation")]
    [SerializeField] private float visualPitch = 34f;

    [Header("Movement")]
    [SerializeField] private float speedSmooth = 12f;
    [SerializeField] private float runEnterThreshold = 0.22f;
    [SerializeField] private float runExitThreshold = 0.12f;
    [SerializeField] private float facingThreshold = 0.08f;

    [Header("Direction Angles")]
    [Range(0, 180)]
    [SerializeField] private float frontAngle = 40f;

    private static readonly int ParamIsWalking = Animator.StringToHash("IsWalking");
    private static readonly int ParamDirection  = Animator.StringToHash("Direction");
    private static readonly int ParamAttack     = Animator.StringToHash("Attack");
    private static readonly int ParamSummon     = Animator.StringToHash("Summon");

    private Camera mainCamera;
    private Vector3 previousPosition;
    private Vector3 smoothedVelocity;
    private bool isWalking;
    private bool isActing;
    private FacingDirection currentDirection = FacingDirection.Front;

    void Start()
    {
        mainCamera = Camera.main;
        previousPosition = transform.position;
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
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

        visualsTransform.rotation = Quaternion.Euler(visualPitch, mainCamera.transform.rotation.eulerAngles.y, 0f);

        float flatSpeed = new Vector2(smoothedVelocity.x, smoothedVelocity.z).magnitude;


        if (!isWalking && flatSpeed > runEnterThreshold) isWalking = true;
      
        else if (isWalking && flatSpeed < runExitThreshold) isWalking = false;

        animator.SetBool(ParamIsWalking, isWalking);

       
        if (!isActing &&  agent.desiredVelocity.magnitude > facingThreshold) UpdateDirection(agent != null ? agent.desiredVelocity : smoothedVelocity);
        Debug.Log("desiredVelocity "+agent.desiredVelocity.magnitude);
        Debug.Log("Direction "+animator.GetInteger(ParamDirection));

    }

    public void TriggerAttack()
    {
        isActing = true;
        animator.SetTrigger(ParamAttack);
    }

    public void TriggerSummon()
    {
        isActing = true;
        animator.SetTrigger(ParamSummon);
    }

    public void OnAttackImpact() { }

    public void OnSummonSpawn() { }

    public void OnActionFinished()
    {
        isActing = false;
    }

    private void UpdateDirection(Vector3 velocity)
    {
        Vector3 localVelocity = mainCamera.transform.InverseTransformDirection(velocity);
        float angle = Mathf.Abs(Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg);

        FacingDirection newDirection = angle > 180f - frontAngle ? FacingDirection.Front : FacingDirection.Side;

        if (newDirection == FacingDirection.Side)
            SetFacingRight(localVelocity.x > 0f);
        else
            SetFacingRight(true);

        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            animator.SetInteger(ParamDirection, (int)currentDirection);
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
