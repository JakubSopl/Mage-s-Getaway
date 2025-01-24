using System;
using System.Collections;
using UnityEngine;

public class UnitParticleController : MonoBehaviour
{
    private ParticleSystem currentParticleSystem;

    public IEnumerator PlayParticle(string name, Action onParticleFinish)
    {
        currentParticleSystem = transform.Find(name)?.GetComponent<ParticleSystem>();
        if (currentParticleSystem != null)
        {
            currentParticleSystem.Play();
            yield return new WaitForSeconds(currentParticleSystem.main.duration + currentParticleSystem.main.startLifetime.constantMax);
        }

        onParticleFinish?.Invoke();
    }
}
