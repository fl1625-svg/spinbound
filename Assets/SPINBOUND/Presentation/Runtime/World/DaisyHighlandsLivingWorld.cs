using UnityEngine;

namespace Spinbound.Presentation.World
{
    public sealed class DaisyHighlandsLivingWorld : MonoBehaviour
    {
        [SerializeField] private float _windPhase;
        [SerializeField] private float _breathingAmplitude=.018f;
        private Transform[] _daisies;
        private void Start(){var list=new System.Collections.Generic.List<Transform>();foreach(Transform t in transform)if(t.name=="Daisy")list.Add(t);_daisies=list.ToArray();}
        private void LateUpdate()
        {
            _windPhase += Time.deltaTime;
            if(_daisies==null)return;
            for(int i=0;i<_daisies.Length;i++){var t=_daisies[i];float a=Mathf.Sin(_windPhase*1.1f+i*0.73f)*2.4f;t.localRotation=Quaternion.Euler(a, t.localEulerAngles.y, -a*.55f);float s=1f+Mathf.Sin(_windPhase*.7f+i)*_breathingAmplitude;t.localScale=new Vector3(s,s,s);}
        }
    }
}
