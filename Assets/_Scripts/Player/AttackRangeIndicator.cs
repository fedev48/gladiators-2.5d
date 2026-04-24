using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AttackRangeIndicator : MonoBehaviour
{
    [SerializeField] private DecalProjector decalLine;
    [SerializeField] private DecalProjector decalTip;
    [SerializeField] private Unit decalOrigin;
    Vector3 targetPosition = Vector3.zero;
    void Update()
    {
        decalTip.transform.position = targetPosition;
        Vector3 direction = targetPosition - decalOrigin.transform.position;
        float distance = direction.magnitude;
        decalLine.size = new Vector3(distance, decalLine.size.y, decalLine.size.z);
        decalLine.pivot = new Vector3(distance/2, decalLine.pivot.y, decalLine.pivot.z);
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        decalLine.transform.rotation = Quaternion.Euler(90, -angle, 0);
    }

    public void UpdateTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }
    public void SetOriginUnit(Unit unit)
    {
        decalOrigin = unit;
    }


}
