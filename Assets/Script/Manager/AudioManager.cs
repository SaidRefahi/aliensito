using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Sound
{
    [Tooltip("El nombre que usaremos para llamar a este sonido (ej. 'Disparo_Jefe')")]
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Listas de Sonido")]
    [SerializeField] private List<Sound> musicTracks;
    [SerializeField] private List<Sound> sfxClips;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private float musicVolume = 0.3f; 
    private float sfxVolume = 0.3f;   

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume; // <--- NUEVO (Asignamos el volumen inicial)
        
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

  
    public void PlaySFX(string name)
    {
        Sound s = FindSound(sfxClips, name);
        if (s == null) return;


        sfxSource.PlayOneShot(s.clip, sfxVolume); // <--- MODIFICADO
    }
    
    public void PlayMusic(string name)
    {
        Sound s = FindSound(musicTracks, name);
        if (s == null) return;

        if (musicSource.clip == s.clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void SetMusicVolume(float volume) 
    {
        musicVolume = volume;
        musicSource.volume = musicVolume; 
    }


    public void SetSFXVolume(float volume) 
    {
        sfxVolume = volume;

    }


    private Sound FindSound(List<Sound> list, string name)
    {
        Sound s = list.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("¡AudioManager: Sonido '" + name + "' no encontrado!");
            return null;
        }
        return s;
    }
}