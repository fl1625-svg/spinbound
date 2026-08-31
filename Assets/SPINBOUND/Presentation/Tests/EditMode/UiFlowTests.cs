using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;
using Spinbound.Meta;
using Spinbound.Presentation.UI;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Presentation.Tests.EditMode
{
    public sealed class UiFlowTests
    {
        [Test]
        public void ResultsPanelDisplaysImmutableRunResultValues()
        {
            var root = new GameObject("UI Test Root", typeof(RectTransform));
            try
            {
                var result = new RunResult(
                    stageId: World1StageSequence.Get(0).Id,
                    rawTimeSeconds: 12.345f,
                    penaltySeconds: 3f,
                    damageCount: 0,
                    orbitCoreIds: new[] { "core-a", "core-b" },
                    mode: RotorMode.Standard,
                    practice: false,
                    completed: true);

                var record = new StageProgressRecord(
                    result.StageId,
                    hasCleared: true,
                    bestValidTimeSeconds: 14.500f,
                    bestDamageCount: 0,
                    masteryFlags: StageMasteryFlags.Clear | StageMasteryFlags.Perfect | StageMasteryFlags.MasterTime,
                    orbitCoreIds: new[] { "core-a", "core-b" });

                ResultsPanel panel = ResultsPanel.Build(root.transform, null, null, null);
                panel.Show(World1StageSequence.Get(0), result, record, World1StageSequence.Get(1));

                Assert.That(panel.CurrentResult, Is.SameAs(result));
                Assert.That(panel.CurrentRecord, Is.SameAs(record));

                string[] text = root.GetComponentsInChildren<Text>(true).Select(value => value.text).ToArray();
                Assert.That(text, Does.Contain("00:15.345"));
                Assert.That(text, Does.Contain("PERFECT"));
                Assert.That(text, Does.Contain("2 / 3"));
                Assert.That(text, Does.Contain("MASTER TIME"));
                Assert.That(text, Does.Contain("00:14.500"));
                Assert.That(text, Does.Contain("NEXT COURSE"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void AccessibilitySettingsClampUnsafeValuesWithoutMutatingSource()
        {
            var source = new AccessibilitySettings
            {
                CameraSensitivity = 99f,
                VfxIntensity = -5f,
                MusicVolume = 4f,
                SfxVolume = -2f,
                AmbienceVolume = 2f,
                UiVolume = -1f,
                TouchSize = .1f,
            };

            AccessibilitySettings sanitized = source.Sanitized();

            Assert.That(source.CameraSensitivity, Is.EqualTo(99f));
            Assert.That(sanitized.CameraSensitivity, Is.EqualTo(2.5f));
            Assert.That(sanitized.VfxIntensity, Is.EqualTo(0f));
            Assert.That(sanitized.MusicVolume, Is.EqualTo(1f));
            Assert.That(sanitized.SfxVolume, Is.EqualTo(0f));
            Assert.That(sanitized.AmbienceVolume, Is.EqualTo(1f));
            Assert.That(sanitized.UiVolume, Is.EqualTo(0f));
            Assert.That(sanitized.TouchSize, Is.EqualTo(.75f));
        }
    }
}
