using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepEat : MonoBehaviour
{
    public Animator animator;

    // Vyberte animaci, kterou má zvíøe používat
    public string chosenAnimation = "GoatSheep_eating"; 

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Pøehrávej pouze jednu animaci pøi spuštìní hry
        PlayStaticAnimation();
    }

    void PlayStaticAnimation()
    {
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(chosenAnimation))
        {
            animator.CrossFade(chosenAnimation, 0.1f); // Hladký pøechod na vybranou animaci
        }
    }
}
