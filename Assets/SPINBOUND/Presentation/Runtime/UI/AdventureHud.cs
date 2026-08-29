using UnityEngine;
using UnityEngine.UI;

namespace Spinbound.Presentation.UI
{
    public sealed class AdventureHud : MonoBehaviour
    {
        private Text _time; private Text _hearts; private Text _course;
        public Text Hearts => _hearts;
        public void SetTime(float seconds){if(_time!=null)_time.text=$"TIME   {seconds:00.000}";}
        public void SetHearts(int value){if(_hearts!=null)_hearts.text=$"♥  {value}";}
        public static AdventureHud Build()
        {
            var root=new GameObject("Adventure HUD");var canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;root.AddComponent<GraphicRaycaster>();
            var hud=root.AddComponent<AdventureHud>();hud._course=CreatePanelLabel(root.transform,"Course Panel","W1-1   DAISY HIGHLANDS",new Vector2(48,-46),new Vector2(450,54),false,27,FontStyle.Bold);hud._time=CreatePanelLabel(root.transform,"Time Panel","TIME   00.000",new Vector2(48,-106),new Vector2(300,48),false,22,FontStyle.Normal);hud._hearts=CreatePanelLabel(root.transform,"Hearts Panel","♥  3",new Vector2(-48,-46),new Vector2(160,54),true,27,FontStyle.Bold);return hud;
        }
        private static Text CreatePanelLabel(Transform parent,string name,string text,Vector2 pos,Vector2 size,bool right,int fontSize,FontStyle style)
        {
            var panel=new GameObject(name);panel.transform.SetParent(parent,false);var pr=panel.AddComponent<RectTransform>();var anchor=right?new Vector2(1,1):new Vector2(0,1);pr.anchorMin=pr.anchorMax=anchor;pr.pivot=right?new Vector2(1,1):new Vector2(0,1);pr.anchoredPosition=pos;pr.sizeDelta=size;var image=panel.AddComponent<Image>();image.color=new Color(.018f,.040f,.065f,.72f);image.raycastTarget=false;
            var label=new GameObject("Label");label.transform.SetParent(panel.transform,false);var lr=label.AddComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(16,7);lr.offsetMax=new Vector2(-16,-7);var t=label.AddComponent<Text>();t.text=text;t.alignment=right?TextAnchor.MiddleRight:TextAnchor.MiddleLeft;t.fontSize=fontSize;t.fontStyle=style;t.color=Color.white;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.raycastTarget=false;return t;
        }
    }
}
