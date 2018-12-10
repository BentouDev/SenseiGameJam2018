using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class DeadBody : BaseBehaviour
{
    public Collider DeadCollider;
    public Collider DeadTrigger;
    public AIDriver Driver;
    public LayerMask Mask;
    public float RagdollDelay = 3;

    public bool WasThrown { get; private set; }

    void Start()
    {
        if (!Driver)
            Driver = GetComponent<AIDriver>();

        DeadTrigger.enabled = false;
    }
    
    public void JustDied()
    {
        DeadTrigger.enabled = true;
    }

    public void OnPickup()
    {
        // Ensure calmness
        foreach (var child in Driver.Pawn.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }
        
        Driver.Pawn.Body.freezeRotation = true;
        Driver.Pawn.Body.useGravity = false;
        Driver.Pawn.ResetBody();
    }
    
    public void EnsureDeadCollider()
    {
        foreach (var child in Driver.Pawn.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }

        DeadCollider.enabled = true;
    }

    public void Throw(Vector3 power)
    {
        if (!Driver)
            Driver = GetComponent<AIDriver>();

        WasThrown = true;
        
        EnsureDeadCollider();
        EnableRagdoll();

        Driver.Pawn.Body.AddForce(power, ForceMode.Impulse);
    }

    IEnumerator HandleRagdoll()
    {
        yield return new WaitForSeconds(RagdollDelay);
        
        while (!Physics.Raycast(Driver.Pawn.transform.position, Vector3.down, 1, Mask))
        {
            yield return null;
        }

        DisableRagdoll();
    }

    public void EnableRagdoll()
    {
        Driver.Pawn.Body.freezeRotation = false;
        Driver.Pawn.Body.useGravity = true;

        StartCoroutine(HandleRagdoll());
    }

    public void DisableRagdoll()
    {
        WasThrown = false;
        
        Driver.Pawn.Body.freezeRotation = true;
        Driver.Pawn.Body.useGravity = false;
        
        foreach (var child in Driver.Pawn.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }
        
        JustDied();
        
        Driver.Pawn.ResetBody();
    }
}
