using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleIdle : AIModule
{
    protected override float CalcPriority()
    {
        return 1;
    }

    protected override Vector3 OnProcessMovement()
    {
        var best = Driver.PickShuffleModule() ?? Driver.PickBestModule();
        if (best != null && best != this)
        {
            Driver.SwitchModule(best);
        }

        return Vector3.zero;
    }
}
