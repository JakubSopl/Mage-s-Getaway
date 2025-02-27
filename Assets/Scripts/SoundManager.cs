using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton

    public AudioSource soundEffectsSource;
    public AudioSource musicSource;

    public List<AudioClip> soundEffects; // Seznam všech zvukových efektù
    public AudioClip soundtrack; // Pøidej SoundTrack sem

    private Dictionary<string, AudioClip> soundEffectsDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Naplníme dictionary zvukovými efekty
        soundEffectsDict = new Dictionary<string, AudioClip>();
        foreach (var clip in soundEffects)
        {
            soundEffectsDict[clip.name] = clip;
        }

        // Spustíme soundtrack, pokud je pøiøazen
        if (soundtrack != null)
        {
            PlayLoopingMusic(soundtrack);
        }
    }

    public void PlaySound(string soundName)
    {
        if (soundEffectsDict.ContainsKey(soundName))
        {
            soundEffectsSource.PlayOneShot(soundEffectsDict[soundName]); // Pøehrání zvuku bez ovlivnìní ostatních
        }
        else
        {
            Debug.LogWarning("Zvuk '" + soundName + "' nebyl nalezen!");
        }
    }

    public void PlayMusic(AudioClip music)
    {
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.loop = false; // Normální pøehrání bez loopu
            musicSource.Play();
        }
    }

    public void PlayLoopingMusic(AudioClip music)
    {
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.loop = true; // Aktivuje opakování
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
