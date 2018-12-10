using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class PotController : BaseBehaviour
{
    public Damageable Dmg;
    public float DelayFromDmg = 1;

    private float Lasthurt;
    
    public bool IsAlive => Dmg && Dmg.IsAlive;
    
    private List<Transform> Processing = new List<Transform>();

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
        var dead = other.GetComponentInParent<DeadBody>();
        if (dead && dead.WasThrown)
        {
            var pawn = dead.GetComponent<BasePawn>();
            if (!pawn.IsAlive())
                ThrowToPot(pawn);
        }
    }

    public void ThrowToPot(BasePawn pawn)
    {
        if (Processing.Contains(pawn.transform))
            return;
        
        Processing.Add(pawn.transform);
        DoAnimate(pawn.transform);
    }
    
    public void DoAnimate(Transform pawn = null)
    {
        if (!pawn)
            return;
        
        StartCoroutine(CoAnimate(pawn));
    }

    private bool Animate;
    private CurveContainer Provider;

    IEnumerator CoAnimate(Transform pawn)
    {
        float startTime = Time.time;

        if (!Provider)
        {
            var go = GameObject.FindWithTag("Curve");
            Provider = go.GetComponent<CurveContainer>();
        }

        var scaleCurve = Provider.ScaleCurve;
        var posCurve = Provider.PosCurve;
        var effectPrefab = Provider.Object;
        var onPot = Provider.OnPot;

        Vector3 zeroPos = pawn.position;
        Vector3 oldScale = pawn.transform.localScale;

        float Duration = Vector3.Distance(zeroPos, transform.position);
        
        Animate = true;
        bool spawned = false;
        bool animated = false;
        while (Animate && Time.time - startTime < Duration)
        {
            var coeff = (Time.time - startTime) / Duration;
            var pos = Vector3.Lerp(zeroPos, transform.position, coeff);
            zeroPos = pos;
            
            // pos.y += Mathf.Sin(0.75f + (Mathf.PI * coeff)) * 3;

            if (!animated && coeff > Provider.AnimPlayThreshold)
            {
                animated = true;
                Provider.Anim.SetTrigger(Provider.Trigger);
            }

            if (!spawned && coeff > Provider.HealPlayThreshold)
            {
                spawned = true;

                Instantiate(effectPrefab, transform.position, Quaternion.identity);
                MainGame.Instance.Pot.Dmg.Heal(15);
                onPot.Invoke();
            }

            var scaleValue = scaleCurve.Evaluate(coeff);
            var posValue = posCurve.Evaluate(coeff);

            pos.y = zeroPos.y + posValue; 

            pawn.transform.position = pos;
            pawn.transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, scaleValue);

            yield return null;
        }
        
        Processing.Remove(pawn);
        Destroy(pawn.gameObject);
    }

    public void StopAnim()
    {
        Animate = false;
    }
}
