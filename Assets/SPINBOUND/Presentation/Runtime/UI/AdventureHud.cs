using UnityEngine;
using UnityEngine.UI;

namespace Spinbound.Presentation.UI
{
    public sealed class AdventureHud : MonoBehaviour
    {
        private Text _time;
        private Text _hearts;
        private Text _course;

        public Text Hearts => _hearts;

        public void SetTime(float seconds)
        {
            if(_time==null) return;
            int minutes=Mathf.FloorToInt(Mathf.Max(0f,seconds)/60f);
            float remaining=Mathf.Max(0f,seconds)-minutes*60f;
            _time.text=$"{minutes:00}:{remaining:00.000}";
        }

        public void SetHearts(int value)
        {
            if(_hearts==null) return;
            int hearts=Mathf.Clamp(value,0,5);
            _hearts.text=hearts<=0?"—":new string('♥',hearts);
        }

        public static AdventureHud Build()
        {
            var root=new GameObject("Adventure HUD");
            var canvas=root.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=50;

            var scaler=root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.matchWidthOrHeight=.5f;
            root.AddComponent<GraphicRaycaster>();

            var hud=root.AddComponent<AdventureHud>();
            hud._course=CreateCourseCard(root.transform);
            hud._time=CreateTimeCard(root.transform);
            hud._hearts=CreateHeartsCard(root.transform);
            return hud;
        }

        private static Text CreateCourseCard(Transform parent)
        {
            var panel=CreateCard(parent,"Course Card",new Vector2(40,-38),new Vector2(468,62),false,new Color(.025f,.055f,.080f,.72f),new Color(.45f,.91f,.34f,1f));

            var tag=CreateText(panel,"World Tag","W1—1",new Vector2(16,9),new Vector2(78,44),TextAnchor.MiddleCenter,20,FontStyle.Bold,new Color(.08f,.17f,.12f,1f));
            var tagBg=tag.gameObject.AddComponent<Image>();
            tagBg.color=new Color(.62f,.94f,.38f,.96f);
            tagBg.raycastTarget=false;
            tag.transform.SetAsFirstSibling();

            var title=CreateText(panel,"Course Title","DAISY HIGHLANDS",new Vector2(112,8),new Vector2(334,46),TextAnchor.MiddleLeft,25,FontStyle.Bold,Color.white);
            AddTextOutline(title,new Color(0f,0f,0f,.34f),new Vector2(1,-1));
            return title;
        }

        private static Text CreateTimeCard(Transform parent)
        {
            var panel=CreateCard(parent,"Time Card",new Vector2(40,-108),new Vector2(238,48),false,new Color(.025f,.055f,.080f,.62f),new Color(.33f,.74f,1f,1f));
            var caption=CreateText(panel,"Time Caption","TIME",new Vector2(15,5),new Vector2(58,38),TextAnchor.MiddleLeft,14,FontStyle.Bold,new Color(.66f,.84f,.98f,1f));
            caption.horizontalOverflow=HorizontalWrapMode.Overflow;
            var value=CreateText(panel,"Time Value","00:00.000",new Vector2(78,5),new Vector2(144,38),TextAnchor.MiddleRight,21,FontStyle.Bold,Color.white);
            AddTextOutline(value,new Color(0f,0f,0f,.32f),new Vector2(1,-1));
            return value;
        }

        private static Text CreateHeartsCard(Transform parent)
        {
            var panel=CreateCard(parent,"Hearts Card",new Vector2(-40,-38),new Vector2(192,62),true,new Color(.025f,.055f,.080f,.70f),new Color(1f,.46f,.60f,1f));
            var caption=CreateText(panel,"Heart Caption","ENERGY",new Vector2(14,7),new Vector2(72,48),TextAnchor.MiddleLeft,13,FontStyle.Bold,new Color(1f,.72f,.78f,1f));
            caption.horizontalOverflow=HorizontalWrapMode.Overflow;
            var value=CreateText(panel,"Heart Value","♥♥♥",new Vector2(86,7),new Vector2(88,48),TextAnchor.MiddleRight,25,FontStyle.Bold,new Color(1f,.52f,.64f,1f));
            AddTextOutline(value,new Color(.20f,.01f,.04f,.42f),new Vector2(1,-1));
            return value;
        }

        private static RectTransform CreateCard(Transform parent,string name,Vector2 pos,Vector2 size,bool right,Color background,Color accent)
        {
            var shadow=new GameObject(name+" Shadow");
            shadow.transform.SetParent(parent,false);
            var sr=shadow.AddComponent<RectTransform>();
            ConfigureRect(sr,pos+new Vector2(right?-4f:4f,-5f),size,right);
            var si=shadow.AddComponent<Image>();
            si.color=new Color(0f,.02f,.04f,.24f);
            si.raycastTarget=false;

            var panel=new GameObject(name);
            panel.transform.SetParent(parent,false);
            var pr=panel.AddComponent<RectTransform>();
            ConfigureRect(pr,pos,size,right);
            var image=panel.AddComponent<Image>();
            image.color=background;
            image.raycastTarget=false;

            var accentBar=new GameObject("Accent");
            accentBar.transform.SetParent(panel.transform,false);
            var ar=accentBar.AddComponent<RectTransform>();
            ar.anchorMin=right?new Vector2(1,0):new Vector2(0,0);
            ar.anchorMax=right?new Vector2(1,1):new Vector2(0,1);
            ar.pivot=right?new Vector2(1,.5f):new Vector2(0,.5f);
            ar.anchoredPosition=Vector2.zero;
            ar.sizeDelta=new Vector2(6,0);
            var ai=accentBar.AddComponent<Image>();
            ai.color=accent;
            ai.raycastTarget=false;

            var highlight=new GameObject("Glass Highlight");
            highlight.transform.SetParent(panel.transform,false);
            var hr=highlight.AddComponent<RectTransform>();
            hr.anchorMin=new Vector2(0,1);
            hr.anchorMax=new Vector2(1,1);
            hr.pivot=new Vector2(.5f,1);
            hr.anchoredPosition=Vector2.zero;
            hr.sizeDelta=new Vector2(-12,1);
            var hi=highlight.AddComponent<Image>();
            hi.color=new Color(1f,1f,1f,.16f);
            hi.raycastTarget=false;
            return pr;
        }

        private static void ConfigureRect(RectTransform rect,Vector2 pos,Vector2 size,bool right)
        {
            var anchor=right?new Vector2(1,1):new Vector2(0,1);
            rect.anchorMin=rect.anchorMax=anchor;
            rect.pivot=right?new Vector2(1,1):new Vector2(0,1);
            rect.anchoredPosition=pos;
            rect.sizeDelta=size;
        }

        private static Text CreateText(RectTransform parent,string name,string value,Vector2 pos,Vector2 size,TextAnchor alignment,int fontSize,FontStyle style,Color color)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            var rect=go.AddComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);
            rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(pos.x,-pos.y);
            rect.sizeDelta=size;

            var text=go.AddComponent<Text>();
            text.text=value;
            text.alignment=alignment;
            text.fontSize=fontSize;
            text.fontStyle=style;
            text.color=color;
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget=false;
            text.resizeTextForBestFit=false;
            return text;
        }

        private static void AddTextOutline(Text text,Color color,Vector2 distance)
        {
            var outline=text.gameObject.AddComponent<Outline>();
            outline.effectColor=color;
            outline.effectDistance=distance;
            outline.useGraphicAlpha=true;
        }
    }
}
