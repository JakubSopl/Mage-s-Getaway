using System;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    private Animator animator;
    private Action onAnimationComplete;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnAnimationComplete()
    {
        onAnimationComplete?.Invoke();
    }

    public void PlayAnimation(string animationName)
    {
        animator.SetTrigger(animationName);
    }

    public void PlayForcedAnimation(string animationName, Action onAnimationComplete)
    {
        PlayAnimation(animationName);
        this.onAnimationComplete = onAnimationComplete;
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Ignoruj výšku
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
