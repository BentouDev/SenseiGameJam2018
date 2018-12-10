using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CurveContainer : MonoBehaviour
{
    public AnimationCurve ScaleCurve;
    public AnimationCurve PosCurve;

    public GameObject Object;

    public UnityEvent OnPot;

    public Animator Anim;
    public string Trigger;
    
    public float AnimPlayThreshold = 0.4f;
    public float HealPlayThreshold = 0.5f;
}
