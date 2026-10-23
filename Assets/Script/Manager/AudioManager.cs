using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    [Serializable]
    public class Sound
    {
        [Tooltip("The name used to call this sound (e.g. 'Disparo_Jefe')")]
        public string name;
        public AudioClip clip;
    }

    public class AudioManager : MonoBehaviour, IAudioService
    {
        public static AudioManager Instance { get; private set; }

        [Title("Audio Libraries (Legacy Compatibility)")]
        [SerializeField] private List<Sound> musicTracks = new List<Sound>();
        [SerializeField] private List<Sound> sfxClips = new List<Sound>();

        [Title("Volume Defaults")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.3f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private readonly Dictionary<string, AudioClip> sfxLookup = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AudioClip> musicLookup = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            CacheLibraries();
        }

        private void InitializeAudioSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private void CacheLibraries()
        {
            sfxLookup.Clear();
            foreach (var sound in sfxClips)
            {
                if (!string.IsNullOrEmpty(sound.name) && sound.clip != null)
                {
                    sfxLookup[sound.name] = sound.clip;
                }
            }

            musicLookup.Clear();
            foreach (var sound in musicTracks)
            {
                if (!string.IsNullOrEmpty(sound.name) && sound.clip != null)
                {
                    musicLookup[sound.name] = sound.clip;
                }
            }
        }

        public void Play(AudioCueSO cue, Vector3 position = default)
        {
            if (cue == null) return;

            AudioClip clip = cue.GetRandomClip();
            if (clip == null) return;

            float volume = cue.GetRandomVolume() * sfxVolume;
            float pitch = cue.GetRandomPitch();

            if (cue.SpatialBlend > 0.05f && position != default)
            {
                // 3D spatial playback
                GameObject tempGO = new GameObject($"AudioCue_3D_{clip.name}");
                tempGO.transform.position = position;
                AudioSource source = tempGO.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = volume;
                source.pitch = pitch;
                source.spatialBlend = cue.SpatialBlend;
                source.outputAudioMixerGroup = cue.OutputGroup;
                source.Play();
                Destroy(tempGO, clip.length / Mathf.Max(0.1f, Mathf.Abs(pitch)));
            }
            else
            {
                // 2D playback
                sfxSource.pitch = pitch;
                sfxSource.outputAudioMixerGroup = cue.OutputGroup;
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void PlaySFX(string soundName, Vector3 position = default)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            if (sfxLookup.TryGetValue(soundName, out AudioClip clip))
            {
                sfxSource.pitch = 1f;
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX '{soundName}' not found.");
            }
        }

        public void PlayMusic(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            if (!musicLookup.TryGetValue(soundName, out AudioClip clip))
            {
                Debug.LogWarning($"[AudioManager] Music '{soundName}' not found.");
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            // Smooth crossfade with DOTween
            if (musicSource.isPlaying)
            {
                musicSource.DOFade(0f, 0.4f).SetUpdate(true).OnComplete(() =>
                {
                    musicSource.clip = clip;
                    musicSource.Play();
                    musicSource.DOFade(musicVolume, 0.4f).SetUpdate(true);
                });
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            musicSource.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                musicSource.Stop();
                musicSource.volume = musicVolume;
            });
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetSfxVolume(volume);
        }

        public void SetVolumes(float music, float sfx)
        {
            SetMusicVolume(music);
            SetSfxVolume(sfx);
        }
    }
}
