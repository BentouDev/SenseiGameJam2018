using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotController : MonoBehaviour
{
    public Damageable Damageable;

    public bool IsAlive => Damageable && Damageable.IsAlive;
}
