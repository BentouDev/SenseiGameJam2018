using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleAttack : Ability
{
    private AIDriver Driver;

    [Header("Balance")]
    public int Dmg = 1;
    
    [Header("Anim")]
    public UnityEvent OnAttack;
    
    protected override void OnBegin()
    {
        if (!Driver)
            Driver = GetComponentInParent<AIDriver>();

        if (Driver && Driver.CurrentEnemy)
        {
            Driver.CurrentEnemy.Hurt(Dmg);
            OnAttack.Invoke();
        }
    }
}
