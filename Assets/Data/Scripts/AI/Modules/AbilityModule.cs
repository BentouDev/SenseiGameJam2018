using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine.Events;

public class AbilityModule : AIModule
{
    private Color gizmoColor = Color.white;

    [Header("Debug")]
    public bool DebugDraw;

    [System.Serializable]
    public struct CollisionInfo
    {
        [SerializeField]
        public Vector3 Offset;

        [SerializeField]
        public float Distance;

        [SerializeField]
        public LayerMask Mask;
    }

    [Header("Triggering")]
    public List<CollisionInfo> Collision;

    [Header("System")]
    public bool FaceEnemyOnBegin;
    public bool FaceEnemyOnUpdate;

    public Ability AbilityToUse;
    public AIModule NextModule;
    public bool ShuffleIfPossible;

    private bool Used;

    protected override void OnBegin()
    {
        Used = false;

        if (FaceEnemyOnBegin)
        {
            var dir = Driver.CurrentEnemy.transform.position - Driver.Pawn.transform.position;
            Driver.Pawn.LockFaceDirection(dir.normalized);
        }
    }

    public static void DrawCollider(CollisionInfo info, Transform transform, BasePawn pawn = null)
    {
        if (pawn)
        {
            var offset = pawn.transform.position + pawn.transform.rotation * info.Offset;
            Gizmos.DrawWireSphere(offset, info.Distance);
        }
        else
        {
            var offset = transform.position + transform.rotation * info.Offset;
            Gizmos.DrawWireSphere(offset, info.Distance);
        }
    }

    protected override Vector3 OnProcessMovement()
    {
        if (FaceEnemyOnUpdate)
        {
            var dir = Driver.CurrentEnemy.transform.position - Driver.Pawn.transform.position;
            Driver.Pawn.LockFaceDirection(dir.normalized);
        }
        
        if (!Used)
        {
            Used = true;
            Driver.Abilities.ChangeAbility(AbilityToUse);
            Driver.SwitchModule(NextModule != null ? NextModule : (ShuffleIfPossible ? Driver.PickShuffleModule() : Driver.PickBestModule()));
        }

        Driver.SwitchModule(NextModule != null ? NextModule : (ShuffleIfPossible ? Driver.PickShuffleModule() : Driver.PickBestModule()));

        return base.OnProcessMovement();
    }

    protected override void OnEnd()
    {
        if (FaceEnemyOnBegin || FaceEnemyOnUpdate)
        {
            Driver.Pawn.UnlockFaceDirection();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!DebugDraw)
            return;

        Gizmos.color = gizmoColor;
        foreach (CollisionInfo info in Collision)
        {
            DrawCollider(info, transform, Pawn);
        }
    }

    protected override float CalcPriority()
    {
        if (!AbilityToUse || !AbilityToUse.IsCooledDown)
            return 0;

        if (Driver.CurrentEnemy == null)
            return 0;

        float highestChance = 0;

        foreach (CollisionInfo info in Collision)
        {
            var offset = Pawn.transform.position + Pawn.transform.rotation * info.Offset;
            if (Physics.CheckSphere(offset, info.Distance, info.Mask))
            {
                var distance = Driver.CurrentEnemy.transform.position - offset;
                var rate = (1 - (distance.magnitude / info.Distance));
                var chance = 50 + 100 * rate;

                if (chance > highestChance)
                    highestChance = chance;

                gizmoColor = Color.Lerp(Color.red, Color.white, rate);
            }
            else
            {
                gizmoColor = Color.white;
            }
        }

        return Mathf.Clamp(highestChance, 0, 100);
    }
}