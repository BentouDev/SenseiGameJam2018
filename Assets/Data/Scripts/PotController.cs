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
        var dead = other.GetComponentInChildren<DeadBody>();
        if (dead)
        {
            ThrowToPot(dead.GetComponent<BasePawn>());
        }
    }

    public void ThrowToPot(BasePawn pawn)
    {
        if (Processing.Contains(pawn.transform))
            return;
        
        Processing.Add(pawn.transform);
        DoAnimate(pawn.transform);
        // StartCoroutine(ProcessPotThrow(pawn));
    }
    
    IEnumerator ProcessPotThrow(BasePawn takenPawn)
    {
        float beginTime = Time.time;
        var beginPos = takenPawn.transform.position;
        while (Time.time - beginTime < 0.5f)
        {
            var newPos = Vector3.Lerp(beginPos, MainGame.Instance.Pot.transform.position, (Time.time - beginTime) / 0.5f);

            takenPawn.transform.position = newPos;

            yield return null;
        }

        Processing.Remove(takenPawn.transform);
        Destroy(takenPawn.gameObject);
        MainGame.Instance.Pot.Dmg.Heal(15);
    }

    public void DoAnimate(Transform pawn = null)
    {
        if (!pawn)
            return;
        
        StartCoroutine(CoAnimate(pawn));
    }

    private bool Animate;

    IEnumerator CoAnimate(Transform pawn)
    {
        float startTime = Time.time;
        // var force = transform.TransformDirection(ComputerForce(woozek, angle) * Power);
        
//        foreach (var coll in GetComponentsInChildren<Collider>())
//        {
//            Physics.IgnoreCollision(coll, MainGame.Instance.Player.Pawn.GetComponentInChildren<Collider>());            
//        }
//
//        AnimationCurve curveInstance = MainGame.Instance.GlobalVars.GetValue<AnimationCurve>("Curve");
//        var effectPrefab = MainGame.Instance.GlobalVars.GetValue<GameObject>("CauldronEffect");

        // if (!effectPrefab)
        //{
            var hack = GameObject.FindWithTag("Curve");
            var curveInstance = hack.GetComponent<CurveContainer>().Curve;
            var effectPrefab = hack.GetComponent<CurveContainer>().Object;
            var onPot = hack.GetComponent<CurveContainer>().OnPot;
        //}

        Vector3 zeroPos = pawn.position;
        Vector3 oldScale = pawn.transform.localScale;

        float Duration = Vector3.Distance(zeroPos, transform.position);
        
        Animate = true;
        bool spawned = false;
        while (Animate && Time.time - startTime < Duration)
        {
//            Body.velocity = force;
//            
//            force = Vector3.Lerp(force, Vector3.zero, Time.fixedDeltaTime);
//            force.y += Physics.gravity.y * Time.fixedDeltaTime;
//            if (Mathf.Abs(force.magnitude) < 0.01f)
//            {
//                force = Vector3.zero;
//            }

            var coeff = (Time.time - startTime) / Duration;
            var pos = Vector3.Lerp(zeroPos, transform.position, coeff);
            zeroPos = pos;
            pos.y += Mathf.Sin(0.5f + (Mathf.PI * coeff)) * 2;

            if (!spawned && coeff > 0.75f)
            {
                spawned = true;
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
                MainGame.Instance.Pot.Dmg.Heal(15);
                onPot.Invoke();
            }

            var curveValue = curveInstance.Evaluate(coeff);
            
            pawn.transform.position = pos;
            pawn.transform.localScale = Vector3.Lerp(oldScale, Vector3.zero, curveValue);
            // Body.velocity = pos - transform.position; 

            yield return null;
        }

        // Activator.Enabled = true;
        
        Processing.Remove(pawn);
        Destroy(pawn.gameObject);
    }

    public void StopAnim()
    {
        Animate = false;
    }

}
