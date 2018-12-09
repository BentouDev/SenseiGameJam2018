using System.Collections;
using System.Linq;
using Framework;
using UnityEngine;

namespace Data.Scripts
{
    public class GamePlayerController : BasePlayerController
    {
        [Header("Input")]
        public string MoveX = "Move X";
        public string MoveY = "Move Y";
        public string ReviveButton = "Fire 1";
        public string TakeButton = "Fire 2";

        [Header("Body detection")]
        public LayerMask QueryMask;
        
        [Header("Revive")]
        public GameObject CultistPrefab;
        public float ReviveRadius;
        public float TakeRadius;
        public float ReviveDuration;
        public GameObject ReviveEffect;
        
        [Header("Throw")]
        public float ThrowForce = 5;
        public float MaxPotThrowDistance = 7;
        public float LayerBlendTime = 0.5f;
        public float PositionBlendTime = 0.25f;
        public float PositionBlendDelay = 1;
        
        private float StartOfLayerBlend;
        private float StartOfPositionBlend;

        private Vector2 CurrentInput = Vector2.zero;

        public Transform CurrentTarget;

        public BasePawn RevivedPawn;
        public BasePawn TakenPawn;

        private Transform PointOfTaken;
        private Vector3 TakenOffset;

        private bool CanThrow = true;

        public enum State
        {
            Idle,
            Revive,
            Take
        }

        public State CurrentState;

        protected override void OnInit()
        {
            base.OnInit();

            PointOfTaken = Pawn.GetComponent<PointOfTakenProvider>().Provide();
            TakenOffset = Pawn.GetComponent<PointOfTakenProvider>().Offset;
            Pawn.GetComponent<AnimCallback>().OnDoThrow.AddListener(EndThrow);
            Pawn.GetComponent<AnimCallback>().OnEndPullUp.AddListener(EndPullUp);
        }

        protected override void OnProcessControll()
        {
            if (!Pawn)
                return;

            Vector3 direction = Vector3.zero;
            if (Enabled)
            {
                CurrentInput.x = Input.GetAxis(MoveX);
                CurrentInput.y = Input.GetAxis(MoveY);

                if (Input.GetButton(ReviveButton))
                {
                    DoRevive();
                }
                else
                {
                    if (CurrentState == State.Revive && !RevivedPawn)
                        CurrentState = State.Idle;
                }

                if (Input.GetButtonDown(TakeButton))
                {
                    DoTake();
                }
                else if (CanThrow && TakenPawn)
                {
                    TakenPawn.ResetBody();
                    TakenPawn.transform.localPosition = Vector3.zero;
                }

                if (CurrentTarget)
                {
                    var rawDistance = transform.position - CurrentTarget.position;
                    Pawn.DesiredForward = -Vector3.Normalize(rawDistance);
                }

                var flatVelocity = new Vector3(CurrentInput.x, 0, CurrentInput.y);
                direction = Quaternion.LookRotation(Vector3.Normalize(Pawn.DesiredForward)) * flatVelocity;
                
                direction.Normalize();
            }
            
            Pawn.ProcessMovement(direction);
            Pawn.Tick();
        }

        protected override void OnStop()
        {
            Pawn.Stop();
        }

        protected override void OnFixedTick()
        {
            Pawn.FixedTick();
        }

        protected override void OnLateTick()
        {
            //if (IsAttacking)
            //    return;

            if (Enabled)
            {
                if (PawnCamera.transform.forward.magnitude > 0)
                {
                    Pawn.DesiredForward = Vector3.Slerp(Pawn.DesiredForward, new Vector3(
                            PawnCamera.transform.forward.x,
                            0,
                            PawnCamera.transform.forward.z
                        ), Time.deltaTime * 10);
                }
                
                PawnCamera.OnUpdate();
            }

            Pawn.LateTick();
        }

        void DoRevive()
        {
            if (RevivedPawn || CurrentState == State.Take)
                return;

            CurrentState = State.Revive;
            
            foreach (var collider in Physics.OverlapSphere(Pawn.transform.position, ReviveRadius, QueryMask, QueryTriggerInteraction.Collide)
                .Where(p =>
                {
                    var dir = p.transform.position - Pawn.transform.position;
                    return Vector3.Dot(dir.normalized, Pawn.transform.forward) > 0.5f;
                })
                .OrderBy(p => Vector3.Distance(p.transform.position, Pawn.transform.position)))
            {
                var pawn = collider.GetComponentInParent<BasePawn>();
                if (pawn && !pawn.IsAlive())
                {
                    RevivedPawn = pawn;
                    break;
                }
            }

            if (RevivedPawn)
            {
                StartCoroutine(ProcessRevive());
            }
        }

        IEnumerator ProcessRevive()
        {
            var oldSpeed = Pawn.Movement.MaxSpeed;
            Pawn.MaxSpeed *= 0.1f;

            Instantiate(ReviveEffect, RevivedPawn.transform.position, Quaternion.Euler(0,0,90), RevivedPawn.transform); // ;__;

            Pawn.Anim.SetTrigger("OnRevive");
            
            yield return new WaitForSeconds(ReviveDuration);
            
            Pawn.MaxSpeed = oldSpeed;

            var oldPos = RevivedPawn.transform.position;
            Destroy(RevivedPawn.gameObject);
            
            var go = Instantiate(CultistPrefab, oldPos, Quaternion.identity);
            var driver = go.GetComponent<AIDriver>();
            MainGame.Instance.Controllers.Register(driver);
            driver.EnableInput();
            driver.Init();

            CurrentState = State.Idle;
        }


        public void EndPullUp()
        {
            Pawn.MaxSpeed = Pawn.Movement.MaxSpeed * 0.65f;
            CanThrow = true;
        }

        public void EndThrow()
        {
            if (!TakenPawn)
                return;
            
            TakenPawn.transform.SetParent(null, true);
            var driver = TakenPawn.GetComponent<AIDriver>();

            var dirToPot = MainGame.Instance.Pot.transform.position - Pawn.transform.position;
            var distToPot = Vector3.Distance(MainGame.Instance.Pot.transform.position, Pawn.transform.position);
            if (Vector3.Dot(dirToPot, Pawn.transform.forward) > 0.25f && distToPot < MaxPotThrowDistance)
            {
                MainGame.Instance.Pot.ThrowToPot(TakenPawn);
            }
            else
            {
                // back online, but he will fall
                TakenPawn.GetComponent<DeadBody>()
                    .Throw(Vector3.Lerp(Pawn.transform.forward, Vector3.up, 0.5f) * ThrowForce);
            }
            
            TakenPawn = null;
            CurrentState = State.Idle;

            Pawn.MaxSpeed = Pawn.Movement.MaxSpeed;
        }

        void DoTake()
        {
            if (!CanThrow)
                return;
            
            if (CurrentState == State.Take)
            {
                if (!TakenPawn)
                {
                    CurrentState = State.Idle;
                    Debug.DebugBreak();
                    return;
                }
                
                // Perform throw

                Pawn.Anim.SetLayerWeight(1, 0);
                Pawn.Anim.SetTrigger("DoThrow");

                Pawn.MaxSpeed = 0;

                return;
            }
            
            if (CurrentState != State.Idle)
                return;
            
            foreach (var collider in Physics.OverlapSphere(Pawn.transform.position, ReviveRadius, QueryMask, QueryTriggerInteraction.Collide)
                .Where(p =>
                {
                    var dir = p.transform.position - Pawn.transform.position;
                    return Vector3.Dot(dir.normalized, Pawn.transform.forward) > 0.5f;
                })
                .OrderBy(p => Vector3.Distance(p.transform.position, Pawn.transform.position)))
            {
                var pawn = collider.GetComponentInParent<BasePawn>();
                if (pawn && !pawn.IsAlive())
                {
                    TakenPawn = pawn;
                    break;
                }
            }

            if (TakenPawn)
            {
                CanThrow = false;

                var driver = TakenPawn.GetComponent<AIDriver>();
                TakenPawn.GetComponent<DeadBody>().OnPickup();
                
                // pull the plug
                driver.DisableInput();
                TakenPawn.ResetBody();

                Pawn.Anim.SetTrigger("OnTake");
                StartCoroutine(AnimateLayer(0, 1));
                StartCoroutine(AnimatePosition());
                
                CurrentState = State.Take;

                Pawn.MaxSpeed = 0;
            }
        }

        IEnumerator AnimatePosition()
        {
            yield return new WaitForSeconds(PositionBlendDelay);

            StartOfPositionBlend = Time.time;

            var startPos = TakenPawn.transform.position;
            while (Time.time - StartOfPositionBlend < PositionBlendTime)
            {
                var target = PointOfTaken.position;
                TakenPawn.transform.position = Vector3.Lerp
                (
                    startPos,
                    target,
                    (Time.time - StartOfPositionBlend) / PositionBlendTime
                );

                yield return null;
            }

            TakenPawn.transform.SetParent(PointOfTaken);
            TakenPawn.transform.localPosition = Vector3.zero;
        }

        IEnumerator AnimateLayer(float from, float to)
        {
            StartOfLayerBlend = Time.time;
            
            while (Time.time - StartOfLayerBlend < LayerBlendTime)
            {
                float magnitude = (Time.time - StartOfLayerBlend) / LayerBlendTime;
                
                Pawn.Anim.SetLayerWeight(1, Mathf.Lerp(from, to, magnitude));

                yield return null;
            }
        }

        protected override void OnDrawDebug()
        {
            base.OnDrawDebug();
            Print($"Current state {CurrentState}");
            Print($"Current revive {RevivedPawn}");
        }
    }
}