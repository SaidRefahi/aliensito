using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IAudioService
    {
        void Play(AudioCueSO cue, Vector3 position = default);
        void PlaySFX(string soundName, Vector3 position = default);
        void PlayMusic(string soundName);
        void StopMusic();
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        float MusicVolume { get; }
        float SfxVolume { get; }
    }
}
