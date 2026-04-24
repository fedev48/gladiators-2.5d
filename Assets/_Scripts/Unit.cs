using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit : NetworkBehaviour
{
    public NavMeshAgent agent;
    public GameObject SelectedVisual;
    
    public virtual void CalculateLocalPath(Vector3 targetPosition)
    {
        
    }

    public virtual void PerformRightClickAction(Vector3 targetPosition)
    {
        
    }
    public virtual void ActivateOrDeactivateSelectedVisual(bool isActive)
    {
        SelectedVisual.SetActive(isActive);
    }

    public virtual void RangeAttack()
    {
        
    }

   
}
