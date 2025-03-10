using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepEat : MonoBehaviour
{
    public Animator animator;

    // Vyberte animaci, kterou m� zv��e pou��vat
    public string chosenAnimation = "GoatSheep_eating"; 

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // P�ehr�vej pouze jednu animaci p�i spu�t�n� hry
        PlayStaticAnimation();
    }

    void PlayStaticAnimation()
    {
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName(chosenAnimation))
        {
            animator.CrossFade(chosenAnimation, 0.1f); // Hladk� p�echod na vybranou animaci
        }
    }
}
