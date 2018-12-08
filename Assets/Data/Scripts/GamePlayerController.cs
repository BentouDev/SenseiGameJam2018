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

        [Header("Data")]
        public GameObject CultistPrefab;
        public float ReviveRadius;
        public float TakeRadius;
        public float ReviveDuration;
        public LayerMask QueryMask;

        private Vector2 CurrentInput = Vector2.zero;

        public Transform CurrentTarget;

        public BasePawn RevivedPawn;

        public enum State
        {
            Idle,
            Revive,
            Take
        }

        public State CurrentState;

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
                    if (!RevivedPawn)
                        CurrentState = State.Idle;
                }

                if (Input.GetButtonDown(TakeButton))
                {
                    DoTake();
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
            Pawn.MaxSpeed *= 0.5f;
            
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

        void DoTake()
        {
            if (CurrentState != State.Idle)
                return;
        }

        protected override void OnDrawDebug()
        {
            base.OnDrawDebug();
            Print($"Current state {CurrentState}");
            Print($"Current revive {RevivedPawn}");
            
        }
    }
}