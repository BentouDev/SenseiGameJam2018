using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimCallback : MonoBehaviour
{
    public UnityEvent OnDoThrow;
    public UnityEvent OnEndPullUp;
    public UnityEvent OnPlayStep;
    
    void DoThrow(string wtf)
    {
        OnDoThrow.Invoke();
    }

    void EndPullUp(string non)
    {
        OnEndPullUp.Invoke();
    }

    void PlayStep(string ndua)
    {
        OnPlayStep.Invoke();
    }
}
