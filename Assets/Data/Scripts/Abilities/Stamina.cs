using System.Collections;
using UnityEditor;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    public bool InitOnStart;
    public bool Regenerate;
    public int MaxAmount;
    public int StartAmount;
    public int RegeneratePerSec;
    public float RegenerateDelay = 1;
    public float RegeneratePenaltyDelay = 5;

    private bool CanRegenerate = true;
    public int CurrentAmount { get; private set; }
    public float VaryingAmount { get; private set; }

    void Start()
    {
        if (!InitOnStart)
            return;

        Init();
    }

    public void Init()
    {
        VaryingAmount = StartAmount;
        CurrentAmount = StartAmount;
    }

    IEnumerator CoRegenerateDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CanRegenerate = true;
    }

    public void TakeStamina(int amount)
    {
        if (CanRegenerate)
        {
            CanRegenerate = false;

            // Either long delay, because stamina is depleted, or standard one
            StartCoroutine(CurrentAmount - amount < 0
                ? CoRegenerateDelay(RegeneratePenaltyDelay)
                : CoRegenerateDelay(RegenerateDelay));
        }

        CurrentAmount = Mathf.Max(0, CurrentAmount - amount);
        VaryingAmount = CurrentAmount;
    }

    void Update()
    {
        if (!Regenerate)
            return;
        
        if (CanRegenerate)
        {
            VaryingAmount += RegeneratePerSec * Time.deltaTime;
            VaryingAmount = Mathf.Min(VaryingAmount, MaxAmount);

            CurrentAmount = (int)VaryingAmount;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isEditor)
            return;

        StartAmount = Mathf.Clamp(StartAmount, 0, MaxAmount);
    }

    void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position + Vector3.up * 2, "SP : " + CurrentAmount + "/" + MaxAmount);
    }
#endif
}