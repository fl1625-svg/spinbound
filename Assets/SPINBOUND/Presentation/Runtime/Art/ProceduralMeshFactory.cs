using System.Collections.Generic;
using UnityEngine;

namespace Spinbound.Presentation.Art
{
    public static class ProceduralMeshFactory
    {
        public static Mesh CreateRotorArm(float halfLength = 1.44f, float halfWidth = 0.115f, float halfHeight = 0.07f)
            => CreateBeveledBox("RotorArm", new Vector3(halfLength * 2f, halfHeight * 2f, halfWidth * 2f), 0.035f);

        public static Mesh CreateBeveledBlock(string name, Vector3 size, float bevel = 0.08f)
            => CreateBeveledBox(name, size, bevel);

        public static Mesh CreateRotorHub(float radius = 0.34f, float height = 0.18f, int segments = 32)
            => CreateCylinder("RotorHub", radius, height, segments, 0.86f);

        public static Mesh CreateHighlandIsland(float width = 23f, float depth = 12f, float topY = 0f, float bottomY = -2.2f, int segments = 48)
        {
            var verts = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
            verts.Add(new Vector3(0, topY, 0)); normals.Add(Vector3.up); uv.Add(new Vector2(.5f,.5f));
            for (int i=0;i<segments;i++)
            {
                float a=Mathf.PI*2f*i/segments;
                float wobble=1f + 0.035f*Mathf.Sin(i*2.71f)+0.025f*Mathf.Cos(i*4.13f);
                float x=Mathf.Cos(a)*width*.5f*wobble; float z=Mathf.Sin(a)*depth*.5f*wobble;
                verts.Add(new Vector3(x,topY,z)); normals.Add(Vector3.up); uv.Add(new Vector2(x/width+.5f,z/depth+.5f));
            }
            for(int i=0;i<segments;i++){ int a=1+i,b=1+(i+1)%segments; tris.Add(0);tris.Add(b);tris.Add(a); }
            int sideStart=verts.Count;
            for(int i=0;i<segments;i++)
            {
                float a=Mathf.PI*2f*i/segments;
                float wobble=1f + 0.035f*Mathf.Sin(i*2.71f)+0.025f*Mathf.Cos(i*4.13f);
                float x=Mathf.Cos(a)*width*.5f*wobble; float z=Mathf.Sin(a)*depth*.5f*wobble;
                var n=new Vector3(x,0,z).normalized;
                verts.Add(new Vector3(x,topY,z)); normals.Add(n); uv.Add(new Vector2((float)i/segments,1));
                float taper=.76f + .05f*Mathf.Sin(i*1.37f);
                verts.Add(new Vector3(x*taper,bottomY,z*taper)); normals.Add(n); uv.Add(new Vector2((float)i/segments,0));
            }
            for(int i=0;i<segments;i++)
            {
                int n=(i+1)%segments; int t0=sideStart+i*2,b0=t0+1,t1=sideStart+n*2,b1=t1+1;
                tris.Add(t0);tris.Add(t1);tris.Add(b1); tris.Add(t0);tris.Add(b1);tris.Add(b0);
            }
            return Build("HighlandIsland",verts,normals,uv,tris);
        }

        public static Mesh CreateRock(float radius = 0.7f, float height = 0.85f, int rings = 4, int segments = 10)
        {
            var verts=new List<Vector3>(); var normals=new List<Vector3>(); var uv=new List<Vector2>(); var tris=new List<int>();
            for(int r=0;r<=rings;r++)
            {
                float v=(float)r/rings; float y=(v-.5f)*height; float rr=radius*Mathf.Sin(Mathf.PI*v);
                for(int s=0;s<segments;s++)
                {
                    float a=Mathf.PI*2f*s/segments; float jitter=1f+.09f*Mathf.Sin(s*3.17f+r*2.33f);
                    var p=new Vector3(Mathf.Cos(a)*rr*jitter,y,Mathf.Sin(a)*rr*jitter);
                    verts.Add(p); normals.Add(new Vector3(p.x, radius*0.65f, p.z).normalized); uv.Add(new Vector2((float)s/segments,v));
                }
            }
            for(int r=0;r<rings;r++) for(int s=0;s<segments;s++)
            { int n=(s+1)%segments; int a=r*segments+s,b=r*segments+n,c=(r+1)*segments+s,d=(r+1)*segments+n; tris.Add(a);tris.Add(d);tris.Add(c);tris.Add(a);tris.Add(b);tris.Add(d); }
            return Build("Rock",verts,normals,uv,tris);
        }

        public static Mesh CreatePetalBlade(float width=.12f, float height=.55f)
        {
            var verts=new[]{new Vector3(-width,0,0),new Vector3(width,0,0),new Vector3(width*.55f,height,0),new Vector3(0,height*1.15f,0),new Vector3(-width*.55f,height,0)};
            var normals=new[]{Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward};
            var uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,.8f),new Vector2(.5f,1),new Vector2(0,.8f)};
            var tris=new[]{0,1,2,0,2,4,4,2,3, 2,1,0,4,2,0,3,2,4};
            var m=new Mesh{name="PetalBlade"};m.vertices=verts;m.normals=normals;m.uv=uv;m.triangles=tris;m.RecalculateBounds();return m;
        }

        private static Mesh CreateCylinder(string name,float radius,float height,int segments,float topRadiusFactor)
        {
            var verts=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int y=0;y<2;y++) for(int s=0;s<segments;s++)
            { float a=Mathf.PI*2*s/segments; float rr=radius*(y==1?topRadiusFactor:1f); var n=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)); verts.Add(new Vector3(n.x*rr,(y-.5f)*height,n.z*rr));normals.Add(n);uv.Add(new Vector2((float)s/segments,y)); }
            for(int s=0;s<segments;s++){int n=(s+1)%segments,a=s,b=n,c=segments+s,d=segments+n;tris.Add(a);tris.Add(d);tris.Add(c);tris.Add(a);tris.Add(b);tris.Add(d);}
            return Build(name,verts,normals,uv,tris);
        }

        private static Mesh CreateBeveledBox(string name, Vector3 size, float bevel)
        {
            // A production-safe chamfered rectangular prism using 8 corner boxes collapsed into one mesh.
            float hx=size.x*.5f, hy=size.y*.5f, hz=size.z*.5f; bevel=Mathf.Min(bevel,Mathf.Min(hx,Mathf.Min(hy,hz))*.8f);
            var xs=new[]{-hx,-hx+bevel,hx-bevel,hx}; var ys=new[]{-hy,-hy+bevel,hy-bevel,hy}; var zs=new[]{-hz,-hz+bevel,hz-bevel,hz};
            var verts=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            AddFaceGrid(Vector3.up, ys[3], xs, zs, true, verts,normals,uv,tris);
            AddFaceGrid(Vector3.down, ys[0], xs, zs, false, verts,normals,uv,tris);
            AddSimpleFace(new Vector3(0,0,1),new Vector3(-hx,-hy,hz-bevel),new Vector3(hx,-hy,hz-bevel),new Vector3(hx,hy,hz-bevel),new Vector3(-hx,hy,hz-bevel),verts,normals,uv,tris);
            AddSimpleFace(new Vector3(0,0,-1),new Vector3(hx,-hy,-hz+bevel),new Vector3(-hx,-hy,-hz+bevel),new Vector3(-hx,hy,-hz+bevel),new Vector3(hx,hy,-hz+bevel),verts,normals,uv,tris);
            AddSimpleFace(Vector3.right,new Vector3(hx-bevel,-hy,hz),new Vector3(hx-bevel,-hy,-hz),new Vector3(hx-bevel,hy,-hz),new Vector3(hx-bevel,hy,hz),verts,normals,uv,tris);
            AddSimpleFace(Vector3.left,new Vector3(-hx+bevel,-hy,-hz),new Vector3(-hx+bevel,-hy,hz),new Vector3(-hx+bevel,hy,hz),new Vector3(-hx+bevel,hy,-hz),verts,normals,uv,tris);
            return Build(name,verts,normals,uv,tris);
        }

        private static void AddFaceGrid(Vector3 normal,float y,float[] xs,float[] zs,bool up,List<Vector3>v,List<Vector3>n,List<Vector2>uv,List<int>t)
        {
            for(int xi=0;xi<3;xi++) for(int zi=0;zi<3;zi++)
            { var a=new Vector3(xs[xi],y,zs[zi]);var b=new Vector3(xs[xi+1],y,zs[zi]);var c=new Vector3(xs[xi+1],y,zs[zi+1]);var d=new Vector3(xs[xi],y,zs[zi+1]); if(up)AddSimpleFace(normal,a,b,c,d,v,n,uv,t); else AddSimpleFace(normal,d,c,b,a,v,n,uv,t); }
        }
        private static void AddSimpleFace(Vector3 normal,Vector3 a,Vector3 b,Vector3 c,Vector3 d,List<Vector3>v,List<Vector3>n,List<Vector2>uv,List<int>t)
        { int i=v.Count;v.AddRange(new[]{a,b,c,d});n.AddRange(new[]{normal,normal,normal,normal});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3}); }
        private static Mesh Build(string name,List<Vector3>v,List<Vector3>n,List<Vector2>uv,List<int>t){var m=new Mesh{name=name};m.SetVertices(v);m.SetNormals(n);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateBounds();return m;}
    }
}
