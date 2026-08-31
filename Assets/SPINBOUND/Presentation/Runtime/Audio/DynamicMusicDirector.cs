using System;
using UnityEngine;
using Spinbound.Presentation.UI;

namespace Spinbound.Presentation.Audio
{
    /// <summary>
    /// Synchronized Base/Rhythm/Motion/Finale music stems. Clips must be authored to the same tempo/grid.
    /// </summary>
    public sealed class DynamicMusicDirector : MonoBehaviour
    {
        private AudioSource _baseStem;
        private AudioSource _rhythmStem;
        private AudioSource _motionStem;
        private AudioSource _finaleStem;
        private float _musicVolume = .85f;
        private float _intensity;
        private bool _started;
        private bool _paused;

        public static DynamicMusicDirector Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var root = new GameObject("Dynamic Music Director");
            root.transform.SetParent(parent, false);
            var director = root.AddComponent<DynamicMusicDirector>();
            director._baseStem = CreateStem(root.transform, "Base Stem");
            director._rhythmStem = CreateStem(root.transform, "Rhythm Stem");
            director._motionStem = CreateStem(root.transform, "Motion Stem");
            director._finaleStem = CreateStem(root.transform, "Finale Stem");
            director.ApplyMix();
            return director;
        }

        public void ConfigureStems(AudioClip baseStem, AudioClip rhythmStem, AudioClip motionStem, AudioClip finaleStem)
        {
            StopAll();
            _baseStem.clip = baseStem;
            _rhythmStem.clip = rhythmStem;
            _motionStem.clip = motionStem;
            _finaleStem.clip = finaleStem;
            _started = false;
            StartSynchronized();
        }

        public void ApplySettings(AccessibilitySettings settings)
        {
            if (settings == null) return;
            _musicVolume = Mathf.Clamp01(settings.MusicVolume);
            ApplyMix();
        }

        public void SetIntensity(float normalized)
        {
            _intensity = Mathf.Clamp01(normalized);
            ApplyMix();
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            AudioSource[] stems = { _baseStem, _rhythmStem, _motionStem, _finaleStem };
            foreach (AudioSource stem in stems)
            {
                if (stem == null || stem.clip == null) continue;
                if (paused) stem.Pause();
                else stem.UnPause();
            }
        }

        public void StartSynchronized()
        {
            if (_started || _paused) return;
            if (_baseStem == null || _baseStem.clip == null) return;

            double startTime = AudioSettings.dspTime + .08d;
            ScheduleIfPresent(_baseStem, startTime);
            ScheduleIfPresent(_rhythmStem, startTime);
            ScheduleIfPresent(_motionStem, startTime);
            ScheduleIfPresent(_finaleStem, startTime);
            _started = true;
            ApplyMix();
        }

        private void ApplyMix()
        {
            SetVolume(_baseStem, .82f * _musicVolume);
            SetVolume(_rhythmStem, Mathf.SmoothStep(0f, .72f, Mathf.InverseLerp(.12f, .58f, _intensity)) * _musicVolume);
            SetVolume(_motionStem, Mathf.SmoothStep(0f, .78f, Mathf.InverseLerp(.42f, .82f, _intensity)) * _musicVolume);
            SetVolume(_finaleStem, Mathf.SmoothStep(0f, .88f, Mathf.InverseLerp(.76f, 1f, _intensity)) * _musicVolume);
        }

        private void StopAll()
        {
            AudioSource[] stems = { _baseStem, _rhythmStem, _motionStem, _finaleStem };
            foreach (AudioSource stem in stems)
                stem?.Stop();
        }

        private static AudioSource CreateStem(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private static void ScheduleIfPresent(AudioSource source, double startTime)
        {
            if (source != null && source.clip != null)
                source.PlayScheduled(startTime);
        }

        private static void SetVolume(AudioSource source, float value)
        {
            if (source != null) source.volume = Mathf.Clamp01(value);
        }
    }
}
