using UnityEngine;
using System.Collections;

namespace Ursaanimation.CubicFarmAnimals
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;
        public string walkForwardAnimation = "walk_forward";
        public string runForwardAnimation = "run_forward";
        public string turn90LAnimation = "turn_90_L";
        public string turn90RAnimation = "turn_90_R";
        public string trotAnimation = "trot_forward";
        public string sittostandAnimation = "sit_to_stand";
        public string standtositAnimation = "stand_to_sit";

        public Transform groundCheck;
        public float groundCheckDistance = 0.2f;
        public LayerMask groundLayer;
        public LayerMask obstacleLayer; // Pøidáno pro kontrolu pøekážek
        public float obstacleCheckDistance = 1f; // Nastavení vzdálenosti pro kontrolu pøekážek
        private bool isGrounded;
        private bool isObstacleAhead;

        // Movement speed variables
        public float walkSpeed = 1f;
        public float runSpeed = 2f;
        public float moveDuration = 2f;
        public float idleDuration = 8f;
        public float sitDuration = 8f;
        public float shortIdleDuration = 1f;

        // Sit and stand control
        private bool isSitting;

        void Start()
        {
            animator = GetComponent<Animator>();
            StartCoroutine(NPCBehaviorRoutine());
        }

        void Update()
        {
            CheckGroundStatus();
            CheckForObstacle();

            if (isGrounded && !isObstacleAhead)
            {
                MoveCharacter();
            }
            else if (isObstacleAhead)
            {
                StartCoroutine(TurnAndWalk()); // Pokud NPC narazí na pøekážku, otoèí se
            }
        }

        void CheckGroundStatus()
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        }

        void CheckForObstacle()
        {
            Vector3 obstacleRayStart = groundCheck.position + Vector3.up * 0.5f;
            isObstacleAhead = Physics.Raycast(obstacleRayStart, transform.forward, obstacleCheckDistance, obstacleLayer);
        }

        void MoveCharacter()
        {
            if (!isObstacleAhead) // Pøidej kontrolu na pøekážky i zde
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(walkForwardAnimation))
                {
                    transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);
                }
                else if (animator.GetCurrentAnimatorStateInfo(0).IsName(runForwardAnimation))
                {
                    transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
                }
                else if (animator.GetCurrentAnimatorStateInfo(0).IsName(trotAnimation))
                {
                    transform.Translate(Vector3.forward * (walkSpeed * 0.75f) * Time.deltaTime);
                }
            }
            else
            {
                StartCoroutine(TurnAndWalk()); // Pokud je pøekážka pøíliš blízko, otoèí se
            }
        }


        IEnumerator NPCBehaviorRoutine()
        {
            while (true)
            {
                if (isSitting)
                {
                    yield return StandUpForDuration();
                }

                yield return SitOrIdleForDuration();
                yield return RunForDuration();
                yield return IdleBeforeTurn();
                yield return SitOrIdleForDuration();
            }
        }

        IEnumerator RunForDuration()
        {
            animator.CrossFade(runForwardAnimation, 0.1f);
            yield return new WaitForSeconds(moveDuration);

            int nextState = Random.Range(0, 2);
            if (nextState == 0)
            {
                yield return IdleForDuration();
            }
            else
            {
                animator.CrossFade(trotAnimation, 0.1f);
                yield return new WaitForSeconds(moveDuration);
            }
        }

        IEnumerator IdleBeforeTurn()
        {
            animator.CrossFade("idle", 0.1f);
            yield return new WaitForSeconds(shortIdleDuration);

            yield return TurnAndWalk();
        }

        IEnumerator TurnAndWalk()
        {
            int turnDirection = Random.Range(0, 2);

            if (turnDirection == 0)
            {
                animator.CrossFade(turn90LAnimation, 0.1f);
                transform.Rotate(Vector3.up, -90);
            }
            else
            {
                animator.CrossFade(turn90RAnimation, 0.1f);
                transform.Rotate(Vector3.up, 90);
            }

            yield return new WaitForSeconds(1f);

            animator.CrossFade(walkForwardAnimation, 0.1f);
            yield return new WaitForSeconds(moveDuration);
        }

        IEnumerator SitOrIdleForDuration()
        {
            int idleOrSit = Random.Range(0, 2);

            if (idleOrSit == 0)
            {
                animator.CrossFade("idle", 0.1f);
                yield return new WaitForSeconds(idleDuration);
            }
            else if (!isSitting)
            {
                yield return SitDownForDuration();
            }
        }

        IEnumerator SitDownForDuration()
        {
            animator.CrossFade(standtositAnimation, 0.1f);
            isSitting = true;
            yield return new WaitForSeconds(sitDuration);
        }

        IEnumerator StandUpForDuration()
        {
            animator.CrossFade(sittostandAnimation, 0.1f);
            isSitting = false;
            yield return new WaitForSeconds(1f);
        }

        IEnumerator IdleForDuration()
        {
            animator.CrossFade("idle", 0.1f);
            yield return new WaitForSeconds(idleDuration);
        }
    }
}
