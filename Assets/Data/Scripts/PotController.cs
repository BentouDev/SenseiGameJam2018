using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class PotController : BaseBehaviour
{
    public Damageable Dmg;
    public float DelayFromDmg = 1;

    public bool IsAlive => Dmg && Dmg.IsAlive;

    private float Lasthurt;

    private void Start()
    {
        if (!Dmg)
            Dmg = GetComponentInChildren<Damageable>();
    }

    void Update()
    {
        if (!MainGame.Instance.IsPlaying())
            return;
        
        if (Time.time - Lasthurt > DelayFromDmg)
        {
            Lasthurt = Time.time;
            Dmg.Hurt(1);
        }
    }
}
