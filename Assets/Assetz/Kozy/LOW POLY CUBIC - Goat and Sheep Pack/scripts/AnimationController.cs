using UnityEngine;
using System.Collections;

namespace Ursaanimation.CubicFarmAnimals
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;

        // Animations
        public string walkForwardAnimation = "walk_forward";
        public string idleAnimation = "idle";
        public string turn90LAnimation = "turn_90_L";
        public string turn90RAnimation = "turn_90_R";

        // Ground and obstacle checking
        public Transform groundCheck;
        public float groundCheckDistance = 0.2f;
        public LayerMask groundLayer;
        public LayerMask obstacleLayer;
        public float obstacleCheckDistance = 1f;

        // Movement settings
        public float walkSpeed = 1f;
        public float moveDuration = 2f;
        public float turnDuration = 0.8f; // Time to complete a 90° turn
        public float idleDuration = 2f;

        private bool isGrounded;
        private bool isObstacleAhead;
        private bool isTurning;

        void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (groundCheck == null)
            {
                Debug.LogError("GroundCheck transform is not assigned!");
            }

            StartCoroutine(NPCBehaviorRoutine());
        }

        void Update()
        {
            CheckGroundStatus();
            CheckForObstacle();

            if (!isGrounded || isObstacleAhead)
            {
                if (!isTurning) // Avoid triggering multiple turns at once
                {
                    StartCoroutine(HandleObstacleAndTurn());
                }
            }
            else
            {
                MoveCharacter();
            }
        }

        void CheckGroundStatus()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
            }
        }

        void CheckForObstacle()
        {
            if (groundCheck != null)
            {
                Vector3 obstacleRayStart = groundCheck.position + Vector3.up * 0.5f;
                isObstacleAhead = Physics.Raycast(obstacleRayStart, transform.forward, obstacleCheckDistance, obstacleLayer);
            }
        }

        void MoveCharacter()
        {
            if (!isTurning && animator.GetCurrentAnimatorStateInfo(0).IsName(walkForwardAnimation))
            {
                transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);
            }
        }

        IEnumerator HandleObstacleAndTurn()
        {
            isTurning = true;

            // Stop and go to idle animation
            PlayAnimation(idleAnimation);
            yield return new WaitForSeconds(0.5f); // Pause briefly before turning

            // Determine turn direction
            int turnDirection = Random.Range(0, 2);
            string turnAnimation = turnDirection == 0 ? turn90LAnimation : turn90RAnimation;

            // Play turn animation
            PlayAnimation(turnAnimation);

            // Smoothly rotate NPC
            float turnAngle = turnDirection == 0 ? -90f : 90f;
            float elapsedTime = 0f;
            Quaternion initialRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, turnAngle, 0f));

            while (elapsedTime < turnDuration)
            {
                transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / turnDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final rotation is exact
            transform.rotation = targetRotation;

            yield return new WaitForSeconds(0.1f); // Small delay to finish the turn animation

            // Resume walking
            PlayAnimation(walkForwardAnimation);
            isTurning = false;
        }

        IEnumerator NPCBehaviorRoutine()
        {
            while (true)
            {
                // Start walking
                PlayAnimation(walkForwardAnimation);
                yield return new WaitForSeconds(moveDuration);

                // Randomly decide next action
                float randomAction = Random.Range(0f, 1f);

                if (randomAction < 0.4f) // 40% chance to eat
                {
                    PlayAnimation("GoatSheep_eating");
                    yield return new WaitForSeconds(5f); // Eating duration
                }

                // Idle after walking, eating, or trotting
                PlayAnimation(idleAnimation);
                yield return new WaitForSeconds(idleDuration);
            }
        }


        void PlayAnimation(string animationName)
        {
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                animator.CrossFade(animationName, 0.1f);
            }
        }
    }
}
