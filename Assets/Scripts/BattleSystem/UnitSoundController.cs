using UnityEngine;

public class UnitSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {
        switch (name)
        {
            case "Attack":
                if (attackSound != null) audioSource.PlayOneShot(attackSound);
                break;
            case "Heal":
                if (healSound != null) audioSource.PlayOneShot(healSound);
                break;
            case "Death":
                if (deathSound != null) audioSource.PlayOneShot(deathSound);
                break;
        }
    }
}
