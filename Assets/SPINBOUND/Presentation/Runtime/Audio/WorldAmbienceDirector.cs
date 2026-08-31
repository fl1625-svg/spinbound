using System;
using UnityEngine;
using Spinbound.Presentation.UI;

namespace Spinbound.Presentation.Audio
{
    public sealed class WorldAmbienceDirector : MonoBehaviour
    {
        private AudioSource _ambience;
        private float _ambienceVolume = .85f;
        private bool _paused;

        public static WorldAmbienceDirector Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var root = new GameObject("World Ambience Director");
            root.transform.SetParent(parent, false);
            var director = root.AddComponent<WorldAmbienceDirector>();
            director._ambience = root.AddComponent<AudioSource>();
            director._ambience.loop = true;
            director._ambience.playOnAwake = false;
            director._ambience.spatialBlend = 0f;
            director.ApplyMix();
            return director;
        }

        public void ConfigureClip(AudioClip clip)
        {
            if (_ambience.clip == clip) return;
            _ambience.Stop();
            _ambience.clip = clip;
            if (!_paused && clip != null) _ambience.Play();
            ApplyMix();
        }

        public void ApplySettings(AccessibilitySettings settings)
        {
            if (settings == null) return;
            _ambienceVolume = Mathf.Clamp01(settings.AmbienceVolume);
            ApplyMix();
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            if (_ambience == null || _ambience.clip == null) return;
            if (paused)
            {
                _ambience.Pause();
            }
            else
            {
                _ambience.UnPause();
                if (!_ambience.isPlaying) _ambience.Play();
            }
        }

        private void ApplyMix()
        {
            if (_ambience != null)
                _ambience.volume = .72f * _ambienceVolume;
        }
    }
}
