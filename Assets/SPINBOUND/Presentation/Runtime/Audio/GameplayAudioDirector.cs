using System;
using UnityEngine;
using Spinbound.Presentation.UI;

namespace Spinbound.Presentation.Audio
{
    /// <summary>
    /// Presentation-only audio runtime. It consumes gameplay events and settings but never mutates Core state.
    /// </summary>
    public sealed class GameplayAudioDirector : MonoBehaviour
    {
        private RotorAudioDirector _rotor;
        private DynamicMusicDirector _music;
        private WorldAmbienceDirector _ambience;
        private AudioSource _oneShot;
        private AccessibilitySettings _settings;

        private AudioClip _surfaceHit;
        private AudioClip _spring;
        private AudioClip _heart;
        private AudioClip _clear;
        private AudioClip _fail;
        private AudioClip _uiConfirm;
        private AudioClip _uiCancel;

        public RotorAudioDirector Rotor => _rotor;
        public DynamicMusicDirector Music => _music;
        public WorldAmbienceDirector Ambience => _ambience;

        public static GameplayAudioDirector Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var root = new GameObject("Gameplay Audio Runtime");
            root.transform.SetParent(parent, false);
            var director = root.AddComponent<GameplayAudioDirector>();
            director._rotor = RotorAudioDirector.Build(root.transform);
            director._music = DynamicMusicDirector.Build(root.transform);
            director._ambience = WorldAmbienceDirector.Build(root.transform);
            director._oneShot = CreateOneShotSource(root.transform);
            director.ApplySettings(AccessibilitySettings.Load());
            return director;
        }

        public void ConfigureOneShots(
            AudioClip surfaceHit,
            AudioClip rotationSpring,
            AudioClip heartZone,
            AudioClip stageClear,
            AudioClip stageFail,
            AudioClip uiConfirm,
            AudioClip uiCancel)
        {
            _surfaceHit = surfaceHit;
            _spring = rotationSpring;
            _heart = heartZone;
            _clear = stageClear;
            _fail = stageFail;
            _uiConfirm = uiConfirm;
            _uiCancel = uiCancel;
        }

        public void ApplySettings(AccessibilitySettings settings)
        {
            _settings = (settings ?? AccessibilitySettings.Default()).Sanitized();
            _rotor?.ApplySettings(_settings);
            _music?.ApplySettings(_settings);
            _ambience?.ApplySettings(_settings);
            if (_oneShot != null)
                _oneShot.volume = Mathf.Clamp01(_settings.SfxVolume);
        }

        public void Handle(GameplayAudioEvent audioEvent)
        {
            switch (audioEvent.Type)
            {
                case GameplayAudioEventType.RotorMotion:
                    _rotor?.SetSpeedTier(audioEvent.SpeedTier);
                    _music?.SetIntensity(audioEvent.SpeedTier switch
                    {
                        Core.Simulation.SpeedTier.Speed1 => .22f,
                        Core.Simulation.SpeedTier.Speed2 => .58f,
                        _ => .86f,
                    });
                    break;
                case GameplayAudioEventType.SurfaceHit:
                    PlayOneShot(_surfaceHit, Mathf.Lerp(.65f, 1f, audioEvent.Severity));
                    break;
                case GameplayAudioEventType.RotationSpring:
                    PlayOneShot(_spring, 1f);
                    break;
                case GameplayAudioEventType.HeartZone:
                    PlayOneShot(_heart, .92f);
                    break;
                case GameplayAudioEventType.StageClear:
                    _music?.SetIntensity(1f);
                    PlayOneShot(_clear, 1f);
                    break;
                case GameplayAudioEventType.StageFail:
                    PlayOneShot(_fail, 1f);
                    break;
                case GameplayAudioEventType.UiConfirm:
                    PlayUi(_uiConfirm);
                    break;
                case GameplayAudioEventType.UiCancel:
                    PlayUi(_uiCancel);
                    break;
            }
        }

        public void SetPaused(bool paused)
        {
            _rotor?.SetPaused(paused);
            _music?.SetPaused(paused);
            _ambience?.SetPaused(paused);
        }

        private void PlayOneShot(AudioClip clip, float scale)
        {
            if (_oneShot == null || clip == null) return;
            float volume = Mathf.Clamp01((_settings?.SfxVolume ?? 1f) * scale);
            _oneShot.PlayOneShot(clip, volume);
        }

        private void PlayUi(AudioClip clip)
        {
            if (_oneShot == null || clip == null) return;
            float volume = Mathf.Clamp01(_settings?.UiVolume ?? 1f);
            _oneShot.PlayOneShot(clip, volume);
        }

        private static AudioSource CreateOneShotSource(Transform parent)
        {
            var go = new GameObject("Gameplay One Shots");
            go.transform.SetParent(parent, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
