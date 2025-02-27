using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource; // AudioSource pro hudbu
    public AudioClip normalMusic; // Normální soundtrack
    public AudioClip battleMusic; // Soundtrack pro battle
    public Movement movement; // Reference na Movement script

    private bool lastBattleState; // Uloží poslední stav bitvy

    void Start()
    {
        PlayMusic(normalMusic);
        lastBattleState = movement.isInBattle; // Uloží výchozí stav
    }

    void Update()
    {
        if (movement == null || audioSource == null)
        {
            Debug.LogError("MusicManager: Chybí reference na Movement nebo AudioSource!");
            return;
        }

        // Zjistí, jestli se zmìnil stav bitvy
        if (movement.isInBattle != lastBattleState)
        {
            lastBattleState = movement.isInBattle;
            PlayMusic(movement.isInBattle ? battleMusic : normalMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return; // Pokud už hraje správná hudba, nedìláme nic

        Debug.Log("Pøepínám hudbu na: " + clip.name);
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
