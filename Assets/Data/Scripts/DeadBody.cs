using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class DeadBody : BaseBehaviour
{
    public Collider DeadTrigger;
    public AIDriver Driver;
    public LayerMask Mask;
    public float RagdollDelay = 3;

    void Start()
    {
        if (!Driver)
            Driver = GetComponent<AIDriver>();
    }
    
    public void JustDied()
    {
        DeadTrigger.enabled = true;
    }

    public void OnPickup()
    {
        // Ensure calmness
        Driver.Pawn.Body.freezeRotation = true;
        Driver.Pawn.Body.useGravity = false;
        Driver.Pawn.ResetBody();
        Driver.Pawn.transform.rotation = Quaternion.identity;
    }

    public void Throw(Vector3 power)
    {
        if (!Driver)
            Driver = GetComponent<AIDriver>();
        
        foreach (var child in Driver.Pawn.GetComponentsInChildren<Collider>())
        {
            child.enabled = true;
        }

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
        Driver.Pawn.Body.freezeRotation = true;
        Driver.Pawn.Body.useGravity = false;
        
        foreach (var child in Driver.Pawn.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }
        
        JustDied();
        
        Driver.Pawn.ResetBody();
        Driver.Pawn.transform.rotation = Quaternion.identity;
    }
}
