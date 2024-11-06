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
        private bool isGrounded;

        // Movement speed variables
        public float walkSpeed = 1f;
        public float runSpeed = 2f;
        public float moveDuration = 2f; // Duration of running or trotting before stopping
        public float idleDuration = 8f; // Duration for idle animation
        public float sitDuration = 8f; // Duration for sitting animation
        public float shortIdleDuration = 1f; // Short idle time before turning

        // Sit and stand control
        private bool isSitting;

        void Start()
        {
            animator = GetComponent<Animator>();
            StartCoroutine(NPCBehaviorRoutine()); // Start the main NPC behavior coroutine
        }

        void Update()
        {
            CheckGroundStatus();

            if (isGrounded)
            {
                MoveCharacter();
            }
        }

        void CheckGroundStatus()
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        }

        void MoveCharacter()
        {
            // Move forward based on the current animation state
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
                transform.Translate(Vector3.forward * (walkSpeed * 0.75f) * Time.deltaTime); // Trot speed
            }
        }

        IEnumerator NPCBehaviorRoutine()
        {
            while (true)
            {
                // Step 1: Sit or idle for 8 seconds
                if (isSitting)
                {
                    yield return StandUpForDuration(); // Ensure NPC stands up before any other action
                }
                else
                {
                    yield return SitOrIdleForDuration();
                }

                // Step 2: Run for 2 seconds, then go to either idle or trot
                yield return RunForDuration();

                // Step 3: Idle briefly after trot, then turn and walk
                yield return IdleBeforeTurn();

                // Step 4: Go back to sit or idle for 8 seconds
                yield return SitOrIdleForDuration();
            }
        }

        IEnumerator RunForDuration()
        {
            animator.Play(runForwardAnimation);
            yield return new WaitForSeconds(moveDuration); // Run for 2 seconds

            // After running, go to either idle or trot
            int nextState = Random.Range(0, 2);
            if (nextState == 0)
            {
                yield return IdleForDuration(); // Go to idle
            }
            else
            {
                animator.Play(trotAnimation); // Go to trot
                yield return new WaitForSeconds(moveDuration); // Trot for 2 seconds
            }
        }

        IEnumerator IdleBeforeTurn()
        {
            // Play idle for a short duration to create a pause before turning
            animator.Play("idle");
            yield return new WaitForSeconds(shortIdleDuration); // Short idle duration before turn

            // Turn in a random direction, then start walking
            yield return TurnAndWalk();
        }

        IEnumerator TurnAndWalk()
        {
            int turnDirection = Random.Range(0, 2); // 0 for left, 1 for right

            if (turnDirection == 0)
            {
                animator.Play(turn90LAnimation);
                transform.Rotate(Vector3.up, -90); // Rotate the NPC 90 degrees to the left
            }
            else
            {
                animator.Play(turn90RAnimation);
                transform.Rotate(Vector3.up, 90); // Rotate the NPC 90 degrees to the right
            }

            yield return new WaitForSeconds(1f); // Allow time for the turn animation

            // Start walking immediately after turning
            animator.Play(walkForwardAnimation);
            yield return new WaitForSeconds(moveDuration); // Walk for 2 seconds
        }

        IEnumerator SitOrIdleForDuration()
        {
            int idleOrSit = Random.Range(0, 2); // 0 for idle, 1 for sit

            if (idleOrSit == 0)
            {
                animator.Play("idle");
                yield return new WaitForSeconds(idleDuration); // Idle for 8 seconds
            }
            else
            {
                yield return SitDownForDuration(); // Sit for 8 seconds
            }
        }

        IEnumerator SitDownForDuration()
        {
            animator.Play(standtositAnimation);
            isSitting = true;
            yield return new WaitForSeconds(sitDuration); // Stay sitting for exactly 8 seconds
        }

        IEnumerator StandUpForDuration()
        {
            animator.Play(sittostandAnimation); // Play stand-up animation
            isSitting = false;
            yield return new WaitForSeconds(1f); // Allow time for the stand-up animation
        }

        IEnumerator IdleForDuration()
        {
            animator.Play("idle");
            yield return new WaitForSeconds(idleDuration); // Stay idle for at least 8 seconds
        }
    }
}
