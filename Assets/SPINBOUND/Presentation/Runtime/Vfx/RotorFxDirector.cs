using UnityEngine;
using Spinbound.Core.Simulation;

namespace Spinbound.Presentation.Vfx
{
    public sealed class RotorFxDirector : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _trail;
        [SerializeField] private Light _hubLight;
        private SpeedTier _tier;
        public void SetSpeedTier(SpeedTier tier)
        {
            _tier=tier;if(_trail==null)return;var emission=_trail.emission;emission.rateOverTime=tier switch{SpeedTier.Speed1=>4f,SpeedTier.Speed2=>11f,_=>22f};
            var main=_trail.main;main.startSpeed=tier switch{SpeedTier.Speed1=>.12f,SpeedTier.Speed2=>.22f,_=>.42f};
            if(_hubLight!=null)_hubLight.intensity=tier switch{SpeedTier.Speed1=>.55f,SpeedTier.Speed2=>.85f,_=>1.25f};
        }
        public static RotorFxDirector Build(Transform parent)
        {
            var root=new GameObject("Rotor FX");root.transform.SetParent(parent,false);var director=root.AddComponent<RotorFxDirector>();
            var ps=root.AddComponent<ParticleSystem>();var main=ps.main;main.duration=1f;main.loop=true;main.startLifetime=.32f;main.startSize=.055f;main.startColor=new Color(.35f,.78f,1f,.52f);main.simulationSpace=ParticleSystemSimulationSpace.World;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.13f;var emission=ps.emission;emission.rateOverTime=4f;
            var lgo=new GameObject("Hub Glow");lgo.transform.SetParent(parent,false);var light=lgo.AddComponent<Light>();light.type=LightType.Point;light.range=2.4f;light.intensity=.55f;light.color=new Color(.25f,.72f,1f);light.shadows=LightShadows.None;
            director._trail=ps;director._hubLight=light;return director;
        }
    }
}
