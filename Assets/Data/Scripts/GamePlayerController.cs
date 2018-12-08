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
        
        [Header("Throw")]
        public float ThrowForce = 5;
        public float MaxPotThrowDistance = 7;
        public float PotThrowDuration = 4;
        public int PotHealAmount;

        private Vector2 CurrentInput = Vector2.zero;

        public Transform CurrentTarget;

        public BasePawn RevivedPawn;
        public BasePawn TakenPawn;

        private Transform PointOfTaken;

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
                else if (TakenPawn)
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

        IEnumerator ProcessPotThrow(BasePawn takenPawn)
        {
            float beginTime = Time.time;
            var beginPos = takenPawn.transform.position;
            while (Time.time - beginTime < PotThrowDuration)
            {
                var newPos = Vector3.Lerp(beginPos, MainGame.Instance.Pot.transform.position,
                    (Time.time - beginTime) / PotThrowDuration);

                takenPawn.transform.position = newPos;

                yield return null;
            }
            
            Destroy(takenPawn.gameObject);
            MainGame.Instance.Pot.Damageable.Heal(PotHealAmount);
        }

        void DoTake()
        {
            if (CurrentState == State.Take)
            {
                // Perform throw
                TakenPawn.transform.SetParent(null, true);
                var driver = TakenPawn.GetComponent<AIDriver>();

                var dirToPot = MainGame.Instance.Pot.transform.position - Pawn.transform.position;
                var distToPot = Vector3.Distance(MainGame.Instance.Pot.transform.position, Pawn.transform.position);
                if (Vector3.Dot(dirToPot, Pawn.transform.forward) > 0.25f && distToPot < MaxPotThrowDistance)
                {
                    StartCoroutine(ProcessPotThrow(TakenPawn));
                }
                else
                {
                    // back online, but he will fall
                    // driver.EnableInput();

                    foreach (var child in TakenPawn.GetComponentsInChildren<Collider>())
                    {
                        child.enabled = true;
                    }

//                    var statePawn = TakenPawn as StatePawn;
//                    if (statePawn)
//                    {
//                        statePawn.SwitchState<PawnThrown>();
//                    }

                    var force = Vector3.Lerp(Pawn.transform.forward, Vector3.up, 0.5f) * ThrowForce;
                    // TakenPawn.ForceSum = 
                    TakenPawn.Body.AddForce(force, ForceMode.Impulse);
                }

                TakenPawn = null;
                CurrentState = State.Idle;

                return;
            }
            
            if (CurrentState != State.Idle)
                return;
            
            foreach (var collider in Physics.OverlapSphere(Pawn.transform.position, ReviveRadius, QueryMask, QueryTriggerInteraction.Collide)
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
                var driver = TakenPawn.GetComponent<AIDriver>();
                // pull the plug
                driver.DisableInput();
                TakenPawn.ResetBody();
                TakenPawn.transform.SetParent(PointOfTaken);
                TakenPawn.transform.localPosition = Vector3.zero;
                
                CurrentState = State.Take;
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