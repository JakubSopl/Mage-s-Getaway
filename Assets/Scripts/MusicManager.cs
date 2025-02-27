using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource; // AudioSource pro hudbu
    public AudioClip normalMusic; // Norm�ln� soundtrack
    public AudioClip battleMusic; // Soundtrack pro battle
    public Movement movement; // Reference na Movement script

    private bool lastBattleState; // Ulo�� posledn� stav bitvy

    void Start()
    {
        PlayMusic(normalMusic);
        lastBattleState = movement.isInBattle; // Ulo�� v�choz� stav
    }

    void Update()
    {
        if (movement == null || audioSource == null)
        {
            Debug.LogError("MusicManager: Chyb� reference na Movement nebo AudioSource!");
            return;
        }

        // Zjist�, jestli se zm�nil stav bitvy
        if (movement.isInBattle != lastBattleState)
        {
            lastBattleState = movement.isInBattle;
            PlayMusic(movement.isInBattle ? battleMusic : normalMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return; // Pokud u� hraje spr�vn� hudba, ned�l�me nic

        Debug.Log("P�ep�n�m hudbu na: " + clip.name);
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
