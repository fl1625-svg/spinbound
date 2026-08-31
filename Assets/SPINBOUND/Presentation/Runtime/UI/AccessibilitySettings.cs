using System;
using UnityEngine;

namespace Spinbound.Presentation.UI
{
    public enum CameraShakeLevel : byte
    {
        Off = 0,
        Low = 1,
        Full = 2,
    }

    public enum ColorVisionMode : byte
    {
        Standard = 0,
        Deuteranopia = 1,
        Protanopia = 2,
        Tritanopia = 3,
    }

    [Serializable]
    public sealed class AccessibilitySettings
    {
        private const string PlayerPrefsKey = "spinbound.accessibility.v1";

        public CameraShakeLevel CameraShake = CameraShakeLevel.Low;
        public bool ReduceMotion;
        public float CameraSensitivity = 1f;
        public bool ClassicView;
        public ColorVisionMode ColorVision = ColorVisionMode.Standard;
        public float VfxIntensity = 1f;
        public float MusicVolume = .85f;
        public float SfxVolume = 1f;
        public float AmbienceVolume = .85f;
        public float UiVolume = 1f;
        public float TouchSize = 1f;

        public static AccessibilitySettings Default()
        {
            return new AccessibilitySettings();
        }

        public static AccessibilitySettings Load()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return Default();

            try
            {
                string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                    return Default();

                AccessibilitySettings value = JsonUtility.FromJson<AccessibilitySettings>(json);
                return (value ?? Default()).Sanitized();
            }
            catch (Exception)
            {
                return Default();
            }
        }

        public void Save()
        {
            SanitizedInPlace();
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public AccessibilitySettings Sanitized()
        {
            var copy = new AccessibilitySettings
            {
                CameraShake = CameraShake,
                ReduceMotion = ReduceMotion,
                CameraSensitivity = CameraSensitivity,
                ClassicView = ClassicView,
                ColorVision = ColorVision,
                VfxIntensity = VfxIntensity,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                AmbienceVolume = AmbienceVolume,
                UiVolume = UiVolume,
                TouchSize = TouchSize,
            };
            copy.SanitizedInPlace();
            return copy;
        }

        private void SanitizedInPlace()
        {
            if (!Enum.IsDefined(typeof(CameraShakeLevel), CameraShake)) CameraShake = CameraShakeLevel.Low;
            if (!Enum.IsDefined(typeof(ColorVisionMode), ColorVision)) ColorVision = ColorVisionMode.Standard;
            CameraSensitivity = Mathf.Clamp(CameraSensitivity, .35f, 2.5f);
            VfxIntensity = Mathf.Clamp01(VfxIntensity);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            AmbienceVolume = Mathf.Clamp01(AmbienceVolume);
            UiVolume = Mathf.Clamp01(UiVolume);
            TouchSize = Mathf.Clamp(TouchSize, .75f, 1.5f);
        }
    }
}
