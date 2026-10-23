using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    [CreateAssetMenu(fileName = "NewAudioCue", menuName = "Audio/Audio Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [Header("Audio Clips")]
        [Tooltip("Collection of audio clips. One will be chosen randomly on playback to eliminate repetition.")]
        [SerializeField] private AudioClip[] clips;

        [Header("Playback Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float baseVolume = 1f;
        [SerializeField] private Vector2 volumeVariation = new Vector2(0.95f, 1f);
        [SerializeField] private Vector2 pitchVariation = new Vector2(0.95f, 1.05f);

        [Header("Routing & Spatial")]
        [SerializeField] private AudioMixerGroup outputGroup;
        [Range(0f, 1f)]
        [SerializeField] private float spatialBlend = 0f;
        [SerializeField] private bool loop = false;

        public AudioMixerGroup OutputGroup => outputGroup;
        public float SpatialBlend => spatialBlend;
        public bool Loop => loop;

        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        public float GetRandomVolume()
        {
            float mult = Random.Range(volumeVariation.x, volumeVariation.y);
            return Mathf.Clamp01(baseVolume * mult);
        }

        public float GetRandomPitch()
        {
            return Random.Range(pitchVariation.x, pitchVariation.y);
        }

#if UNITY_EDITOR
        [TriInspector.Button("▶ Play Preview")]
        private void PlayPreview()
        {
            AudioClip clip = GetRandomClip();
            if (clip == null)
            {
                Debug.LogWarning($"[AudioCueSO] {name} has no audio clips assigned to preview.", this);
                return;
            }

            System.Reflection.Assembly editorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtil = editorAssembly.GetType("UnityEditor.AudioUtil");
            if (audioUtil != null)
            {
                var method = audioUtil.GetMethod(
                    "PlayPreviewClip",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null,
                    new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null);
                method?.Invoke(null, new object[] { clip, 0, false });
            }
        }

        [TriInspector.Button("■ Stop Preview")]
        private void StopPreview()
        {
            System.Reflection.Assembly editorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtil = editorAssembly.GetType("UnityEditor.AudioUtil");
            if (audioUtil != null)
            {
                var method = audioUtil.GetMethod(
                    "StopAllPreviewClips",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                method?.Invoke(null, null);
            }
        }
#endif
    }
}
