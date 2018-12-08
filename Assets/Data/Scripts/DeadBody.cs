using Framework;
using UnityEngine;

public class DeadBody : BaseBehaviour
{
    public Collider DeadTrigger;
    
    public void JustDied()
    {
        DeadTrigger.enabled = true;
    }
}
