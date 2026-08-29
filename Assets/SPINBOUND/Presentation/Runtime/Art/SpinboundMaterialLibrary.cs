using UnityEngine;

namespace Spinbound.Presentation.Art
{
    public static class SpinboundMaterialLibrary
    {
        public static Material CreateStylized(string name, Color baseColor, Color shadowColor, Color rimColor, float smoothness = 0.32f, float metallic = 0f)
        {
            var shader = Shader.Find("SPINBOUND/Stylized PBR") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_ShadowColor", shadowColor);
            material.SetColor("_RimColor", rimColor);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_RimPower", 3.2f);
            material.SetFloat("_RimStrength", 0.22f);
            material.SetFloat("_MatcapStrength", 0.18f);
            return material;
        }

        public static Material CreateFoliage(string name, Color baseColor, Color tipColor, float windStrength = 0.08f)
        {
            var shader = Shader.Find("SPINBOUND/Stylized Foliage") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_TipColor", tipColor);
            material.SetFloat("_WindStrength", windStrength);
            material.SetFloat("_WindScale", 1.25f);
            material.SetFloat("_Translucency", 0.35f);
            return material;
        }
    }
}
