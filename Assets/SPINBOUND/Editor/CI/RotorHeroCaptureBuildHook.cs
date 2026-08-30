#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Spinbound.EditorTools
{
    public sealed class RotorHeroCaptureBuildHook : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report == null || report.summary.platform != BuildTarget.WebGL)
                return;

            Shader shader = Shader.Find("SPINBOUND/Rotor Hero");
            if (shader == null)
                throw new BuildFailedException("SPINBOUND WebGL requires the SPINBOUND/Rotor Hero shader.");
            if (!shader.isSupported)
                throw new BuildFailedException("SPINBOUND/Rotor Hero is unsupported on the active WebGL build renderer.");

            BuildRotorHeroReviewScene.CaptureForCi();
        }
    }
}
#endif
