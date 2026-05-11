using System.Collections;
using UnityEngine;

public class SwordSwingController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Tip of the weapon — child of the weapon mesh. Defines where the physical tip is.")]
    public Transform tipTransform;

    [Tooltip("The pivot point (e.g. base of the staff). The weapon rotates from here.")]
    public Transform pivotTransform;

    [Tooltip("The point the tip tries to aim at.")]
    public Transform target;

    [Header("Pivot Constraints")]
    [Range(0f, 180f)]
    public float maxPivotAngle = 60f;

    [Header("Spring")]
    [Range(0.5f, 12f)] public float frequency = 4f;
    [Range(0f, 2f)]    public float damping    = 0.6f;
    [Range(-2f, 5f)]   public float response   = 0f;

    [Header("Attack")]
    [Range(0f, 350f)]
    public float attackSpeed;
    [Range(0f, 40f)]
    public float attackReturnSpeed;
    [Range(0f, 5f)]
    [SerializeField] float attackRadius = 1f;

    [Range(0f, 20f)]
    [SerializeField] float frequencyInAttack;
    [Range(0f, 20f)]
    [SerializeField] float frequencyAttackReturn;

    [Header("Pivot Recoil")]
    [Range(0f, 2f)]
    [SerializeField] float pivotLiftAmount = 0.3f;
    [Range(0f, 20f)]
    [SerializeField] float pivotLiftSpeed = 8f;
    [Range(0f, 20f)]
    [SerializeField] float pivotReturnSpeed = 4f;

    private SecondOrderSystem3 _aimSys;

    // Normalized direction from weapon root to tip in weapon's local space — constant after Awake.
    private Vector3 _tipLocalDir;
    // World-space offset from pivot to weapon captured at startup.
    private Vector3 _initialOffset;

    private bool _isAttacking;

    void Awake()
    {
        if (tipTransform == null || pivotTransform == null)
        {
            Debug.LogError("[SwordSwingController] Assign tipTransform and pivotTransform in the Inspector.");
            enabled = false;
            return;
        }

        _tipLocalDir   = tipTransform.localPosition.normalized;
        _initialOffset = transform.position - pivotTransform.position;
        if (_tipLocalDir.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning("[SwordSwingController] tipTransform is at the weapon root origin. Falling back to Vector3.forward.");
            _tipLocalDir = Vector3.forward;
        }

        // Capture the initial spatial relationship between weapon and pivot.
        _initialOffset = transform.position - pivotTransform.position;

        _aimSys = new SecondOrderSystem3(frequency, damping, response, transform.rotation * _tipLocalDir);
    }

    void Update()
    {
        _aimSys.SetParams(frequency, damping, response);

        // Weapon follows pivot while preserving initial offset — rotates from its own position.
        transform.position = pivotTransform.position + _initialOffset;

        Vector3 restAimDir = pivotTransform.rotation * _tipLocalDir;

        Vector3 currentTargetPos = target != null ? target.position : transform.position + restAimDir;
        Vector3 toTarget         = (currentTargetPos - transform.position).normalized;
        Vector3 desiredAimDir    = restAimDir;

        if (toTarget.sqrMagnitude > 0.001f)
        {
            if (_isAttacking)
            {
                desiredAimDir = toTarget;
            }
            else
            {
                float angle = Vector3.Angle(restAimDir, toTarget);
                desiredAimDir = angle <= maxPivotAngle
                    ? toTarget
                    : Vector3.RotateTowards(restAimDir, toTarget, maxPivotAngle * Mathf.Deg2Rad, 0f);
            }
        }

        Vector3 aimDir = _aimSys.Update(desiredAimDir, Time.deltaTime).normalized;

        ApplyRotationToFollowTarget(aimDir);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
           
        if (Input.GetMouseButtonDown(0)&& Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Attack(hitInfo.point);
        }
           
    }

    public void Attack(Vector3 strikePoint)
    {
        if (_isAttacking) return;
        StartCoroutine(AttackStaffCoroutine(strikePoint));
    }

    private IEnumerator AttackStaffCoroutine(Vector3 strikePoint)
    {
        _isAttacking = true;
        Vector3 returnLocalPos = target.localPosition;
        Vector3 strikeDir = target.parent.InverseTransformPoint(strikePoint);
        Vector3 strikeLocalPos = strikeDir.normalized * attackRadius;

        while (Vector3.SqrMagnitude(target.localPosition - strikeLocalPos) > 0.1f)
        {
            frequency = frequencyInAttack;
            Vector3 toTarget = strikeLocalPos - target.localPosition;
            float distance = toTarget.magnitude;

            if (distance <= 0.1f)
                break;

            float step = attackSpeed * Time.deltaTime;

            if (step >= distance)
                target.localPosition = strikeLocalPos;
            else
                target.localPosition += toTarget / distance * step;

            yield return null;
        }

        target.localPosition = strikeLocalPos;
        StartCoroutine(PivotRecoilCoroutine());
        yield return new WaitForSeconds(.5f);

        while (Vector3.SqrMagnitude(target.localPosition - returnLocalPos) > 0.1f)
        {
            frequency = frequencyAttackReturn;
            Vector3 toTarget = returnLocalPos - target.localPosition;
            float distance = toTarget.magnitude;

            if (distance <= 0.1f)
                break;

            float step = attackReturnSpeed * Time.deltaTime;

            if (step >= distance)
                target.localPosition = returnLocalPos;
            else
                target.localPosition += toTarget / distance * step;

            yield return null;
        }

        target.localPosition = returnLocalPos;
        _isAttacking = false;
    }

    
    private IEnumerator PivotRecoilCoroutine()
    {
        Vector3 originLocalPos = pivotTransform.localPosition;
        Vector3 liftedLocalPos = originLocalPos + Vector3.up * pivotLiftAmount;

        while (Vector3.SqrMagnitude(pivotTransform.localPosition - liftedLocalPos) > 0.0001f)
        {
            pivotTransform.localPosition = Vector3.MoveTowards(pivotTransform.localPosition, liftedLocalPos, pivotLiftSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.SqrMagnitude(pivotTransform.localPosition - originLocalPos) > 0.0001f)
        {
            pivotTransform.localPosition = Vector3.MoveTowards(pivotTransform.localPosition, originLocalPos, pivotReturnSpeed * Time.deltaTime);
            yield return null;
        }

        pivotTransform.localPosition = originLocalPos;
    }

    private void ApplyRotationToFollowTarget(Vector3 aimDir)
    {
        if (aimDir.sqrMagnitude < 0.001f) return;

        Vector3 upRef = pivotTransform.up;
        if (Mathf.Abs(Vector3.Dot(aimDir, upRef)) > 0.99f)
            upRef = pivotTransform.forward;

        Quaternion newRot =
            Quaternion.LookRotation(aimDir, upRef) *
            Quaternion.FromToRotation(_tipLocalDir, Vector3.forward);

        
        Quaternion delta = newRot * Quaternion.Inverse(transform.rotation);

    
        Vector3 dir = transform.position - pivotTransform.position;
        dir = delta * dir;

        transform.position = pivotTransform.position + dir;

    
        transform.rotation = newRot;
    }

    
}


public class SecondOrderSystem3
{
    private float   _k1, _k2, _k3;
    private Vector3 _xp;
    private Vector3 _y;
    private Vector3 _yd;

    public Vector3 Value => _y;
    public float   Speed => _yd.magnitude;

    public SecondOrderSystem3(float f, float z, float r, Vector3 x0)
    {
        SetParams(f, z, r);
        _xp = x0;
        _y  = x0;
        _yd = Vector3.zero;
    }

    public void SetParams(float f, float z, float r)
    {
        f = Mathf.Max(f, 0.01f);
        float w = 2f * Mathf.PI * f;
        _k1 = z / (Mathf.PI * f);
        _k2 = 1f / (w * w);
        _k3 = r * z / w;
    }

    public Vector3 Update(Vector3 x, float dt)
    {
        if (dt < 1e-6f) return _y;
        Vector3 xd = (x - _xp) / dt;
        _xp = x;
        float k2s = Mathf.Max(_k2, Mathf.Max(dt * dt / 2f + dt * _k1 / 2f, dt * _k1));
        _y  += dt * _yd;
        _yd += dt * (x + _k3 * xd - _y - _k1 * _yd) / k2s;
        return _y;
    }

    public void Reset(Vector3 x0)
    {
        _xp = x0;
        _y  = x0;
        _yd = Vector3.zero;
    }
}
