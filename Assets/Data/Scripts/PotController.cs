using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class PotController : BaseBehaviour
{
    public Damageable Dmg;
    public float DelayFromDmg = 1;
    
    [Header("Pot data")]
    public float PotThrowDuration = 0.5f;
    public int PotHealAmount = 15;

    public bool IsAlive => Dmg && Dmg.IsAlive;

    private float Lasthurt;

    private List<BasePawn> Processing = new List<BasePawn>();

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

    private void OnTriggerEnter(Collider other)
    {
        var dead = other.GetComponentInChildren<DeadBody>();
        if (dead)
        {
            ThrowToPot(dead.GetComponent<BasePawn>());
        }
    }

    public void ThrowToPot(BasePawn pawn)
    {
        if (Processing.Contains(pawn))
            return;
        
        Processing.Add(pawn);
        StartCoroutine(ProcessPotThrow(pawn));
    }
    
    IEnumerator ProcessPotThrow(BasePawn takenPawn)
    {
        float beginTime = Time.time;
        var beginPos = takenPawn.transform.position;
        while (Time.time - beginTime < 0.5f)
        {
            var newPos = Vector3.Lerp(beginPos, MainGame.Instance.Pot.transform.position, (Time.time - beginTime) / PotThrowDuration);

            takenPawn.transform.position = newPos;

            yield return null;
        }

        Processing.Remove(takenPawn);
        Destroy(takenPawn.gameObject);
        MainGame.Instance.Pot.Dmg.Heal(15);
    }
}
