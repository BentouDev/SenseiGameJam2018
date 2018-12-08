using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Slider TargetUI;
    public Damageable Dmg;

    private void Update()
    {
        if (!TargetUI || !Dmg)
            return;

        TargetUI.value = Dmg.CurrentHealth / (float) Dmg.MaxHealth;
    }
}
