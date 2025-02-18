using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton

    public AudioSource soundEffectsSource;
    public AudioSource musicSource;

    public List<AudioClip> soundEffects; // Seznam v�ech zvukov�ch efekt�
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

        // Napln�me dictionary zvukov�mi efekty
        soundEffectsDict = new Dictionary<string, AudioClip>();
        foreach (var clip in soundEffects)
        {
            soundEffectsDict[clip.name] = clip;
        }
    }

    public void PlaySound(string soundName)
    {
        if (soundEffectsDict.ContainsKey(soundName))
        {
            soundEffectsSource.PlayOneShot(soundEffectsDict[soundName]); // P�ehr�n� zvuku bez ovlivn�n� ostatn�ch
        }
        else
        {
            Debug.LogWarning("Zvuk '" + soundName + "' nebyl nalezen!");
        }
    }

    public void PlayMusic(AudioClip music)
    {
        musicSource.clip = music;
        musicSource.Play();
    }
}
