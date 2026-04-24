
using System;
using UnityEngine;

public abstract class IPlayerSpell : MonoBehaviour
{
    public bool isLoadingRangeAttack;
    public virtual event EventHandler OnAttackFinished;

    public virtual void AttackReleased(object sender, EventArgs e)
    {
        
    }
    public virtual void LoadAttack(Vector3 target)
    {
        
    }
    
    public virtual void PerformAttack(Vector3 target)
    {
        
    }
    

}

