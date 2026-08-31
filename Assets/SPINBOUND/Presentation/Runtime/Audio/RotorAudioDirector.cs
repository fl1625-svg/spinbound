using System;
using UnityEngine;
using Spinbound.Core.Simulation;
using Spinbound.Presentation.UI;

namespace Spinbound.Presentation.Audio
{
    /// <summary>
    /// Layered rotor audio. Production clips are injected explicitly; no synthetic placeholder audio is generated.
    /// </summary>
    public sealed class RotorAudioDirector : MonoBehaviour
    {
        private AudioSource _coreHum;
        private AudioSource _rotationAir;
        private AudioSource _movement;
        private AudioSource _speed2;
        private AudioSource _speed3;
        private SpeedTier _tier = SpeedTier.Speed1;
        private float _sfxVolume = 1f;
        private bool _paused;

        public int ActiveLoopVoiceCount
        {
            get
            {
                int count = 0;
                count += IsAudible(_coreHum) ? 1 : 0;
                count += IsAudible(_rotationAir) ? 1 : 0;
                count += IsAudible(_movement) ? 1 : 0;
                count += IsAudible(_speed2) ? 1 : 0;
                count += IsAudible(_speed3) ? 1 : 0;
                return count;
            }
        }

        public static RotorAudioDirector Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var root = new GameObject("Rotor Audio Director");
            root.transform.SetParent(parent, false);
            var director = root.AddComponent<RotorAudioDirector>();
            director._coreHum = CreateLoop(root.transform, "Core Hum");
            director._rotationAir = CreateLoop(root.transform, "Rotation Air");
            director._movement = CreateLoop(root.transform, "Movement");
            director._speed2 = CreateLoop(root.transform, "Speed 2 Layer");
            director._speed3 = CreateLoop(root.transform, "Speed 3 Layer");
            director.ApplyMix();
            return director;
        }

        public void ConfigureClips(AudioClip coreHum, AudioClip rotationAir, AudioClip movement, AudioClip speed2, AudioClip speed3)
        {
            Assign(_coreHum, coreHum);
            Assign(_rotationAir, rotationAir);
            Assign(_movement, movement);
            Assign(_speed2, speed2);
            Assign(_speed3, speed3);
            EnsureLoopsStarted();
            ApplyMix();
        }

        public void ApplySettings(AccessibilitySettings settings)
        {
            if (settings == null) return;
            _sfxVolume = Mathf.Clamp01(settings.SfxVolume);
            ApplyMix();
        }

        public void SetSpeedTier(SpeedTier tier)
        {
            _tier = tier;
            EnsureLoopsStarted();
            ApplyMix();
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            AudioSource[] sources = { _coreHum, _rotationAir, _movement, _speed2, _speed3 };
            foreach (AudioSource source in sources)
            {
                if (source == null || source.clip == null) continue;
                if (_paused)
                {
                    source.Pause();
                }
                else
                {
                    source.UnPause();
                    if (!source.isPlaying) source.Play();
                }
            }
        }

        private void EnsureLoopsStarted()
        {
            if (_paused) return;
            AudioSource[] sources = { _coreHum, _rotationAir, _movement, _speed2, _speed3 };
            foreach (AudioSource source in sources)
            {
                if (source != null && source.clip != null && !source.isPlaying)
                    source.Play();
            }
        }

        private void ApplyMix()
        {
            int tierValue = (int)_tier;
            float speed2 = tierValue >= (int)SpeedTier.Speed2 ? 1f : 0f;
            float speed3 = tierValue >= (int)SpeedTier.Speed3 ? 1f : 0f;
            SetVolume(_coreHum, .50f * _sfxVolume);
            SetVolume(_rotationAir, .46f * _sfxVolume);
            SetVolume(_movement, .32f * _sfxVolume);
            SetVolume(_speed2, .42f * speed2 * _sfxVolume);
            SetVolume(_speed3, .52f * speed3 * _sfxVolume);
        }

        private static AudioSource CreateLoop(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            return source;
        }

        private static void Assign(AudioSource source, AudioClip clip)
        {
            if (source == null || source.clip == clip) return;
            source.Stop();
            source.clip = clip;
        }

        private static void SetVolume(AudioSource source, float value)
        {
            if (source != null) source.volume = Mathf.Clamp01(value);
        }

        private static bool IsAudible(AudioSource source)
        {
            return source != null && source.clip != null && source.isPlaying && source.volume > .001f;
        }
    }
}
