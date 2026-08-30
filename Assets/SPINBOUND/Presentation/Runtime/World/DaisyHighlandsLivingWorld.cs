using UnityEngine;

namespace Spinbound.Presentation.World
{
    public sealed class DaisyHighlandsLivingWorld : MonoBehaviour
    {
        [SerializeField] private float _windPhase;
        [SerializeField] private float _breathingAmplitude=.018f;
        private Transform[] _daisies;
        private Quaternion[] _baseRotations;
        private Vector3[] _baseScales;

        private void Start()
        {
            var list=new System.Collections.Generic.List<Transform>();
            foreach(Transform t in transform)
            {
                if(t.name=="Daisy") list.Add(t);
            }
            _daisies=list.ToArray();
            _baseRotations=new Quaternion[_daisies.Length];
            _baseScales=new Vector3[_daisies.Length];
            for(int i=0;i<_daisies.Length;i++)
            {
                _baseRotations[i]=_daisies[i].localRotation;
                _baseScales[i]=_daisies[i].localScale;
            }
        }

        private void LateUpdate()
        {
            _windPhase += Time.deltaTime;
            if(_daisies==null)return;
            for(int i=0;i<_daisies.Length;i++)
            {
                var t=_daisies[i];
                float a=Mathf.Sin(_windPhase*1.1f+i*0.73f)*2.4f;
                t.localRotation=_baseRotations[i]*Quaternion.Euler(a,0f,-a*.55f);
                float s=1f+Mathf.Sin(_windPhase*.7f+i)*_breathingAmplitude;
                t.localScale=_baseScales[i]*s;
            }
        }
    }
}
