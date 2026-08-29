import math
from dataclasses import dataclass

HZ=120; DT=1/HZ; ANG=60; RADIUS=.16
SPEED={'precision':2.2,'fast':3.3,'turbo':4.4}
COLLIDERS=[
('north-rim',(-10,6.5),(12,7.2)),('south-rim',(-10,-4.2),(12,-3.5)),
('west-rim',(-10.7,-4.2),(-10,7.2)),('east-rim',(12,-4.2),(12.7,7.2)),
('garden-rock-mass',(-.8,-1.5),(1.2,1.5)),('upper-grove',(6.2,4.8),(8,6.5)),('lower-grove',(5.8,-3.5),(7.4,-2))]
ACTIONS=[((-3,0),'fast',3),((-3,4),'precision',3),((4,4),'fast',4),('wait',.75),((4,0),'precision',3),((10,0),'fast',4)]

def dot(a,b):return a[0]*b[0]+a[1]*b[1]
def sub(a,b):return (a[0]-b[0],a[1]-b[1])
def add(a,b):return (a[0]+b[0],a[1]+b[1])
def mul(a,s):return (a[0]*s,a[1]*s)
def ds(a,b):return (a[0]-b[0])**2+(a[1]-b[1])**2
def point_seg(p,a,b):
 ab=sub(b,a); den=dot(ab,ab)
 if den<=1e-7:return ds(p,a)
 t=max(0,min(1,dot(sub(p,a),ab)/den)); q=add(a,mul(ab,t)); return ds(p,q)
def cross(a,b):return a[0]*b[1]-a[1]*b[0]
def onseg(a,b,p):return min(a[0],b[0])-1e-7<=p[0]<=max(a[0],b[0])+1e-7 and min(a[1],b[1])-1e-7<=p[1]<=max(a[1],b[1])+1e-7
def inter(a,b,c,d):
 o1=cross(sub(b,a),sub(c,a));o2=cross(sub(b,a),sub(d,a));o3=cross(sub(d,c),sub(a,c));o4=cross(sub(d,c),sub(b,c))
 if ((o1>1e-7 and o2<-1e-7) or (o1<-1e-7 and o2>1e-7)) and ((o3>1e-7 and o4<-1e-7) or (o3<-1e-7 and o4>1e-7)):return True
 return (abs(o1)<=1e-7 and onseg(a,b,c)) or (abs(o2)<=1e-7 and onseg(a,b,d)) or (abs(o3)<=1e-7 and onseg(c,d,a)) or (abs(o4)<=1e-7 and onseg(c,d,b))
def segseg(a,b,c,d):
 if inter(a,b,c,d):return 0
 return min(point_seg(a,c,d),point_seg(b,c,d),point_seg(c,a,b),point_seg(d,a,b))
def segaabb(a,b,mn,mx):
 if mn[0]<=a[0]<=mx[0] and mn[1]<=a[1]<=mx[1] or mn[0]<=b[0]<=mx[0] and mn[1]<=b[1]<=mx[1]:return 0
 bl=mn;br=(mx[0],mn[1]);tr=mx;tl=(mn[0],mx[1])
 return min(segseg(a,b,bl,br),segseg(a,b,br,tr),segseg(a,b,tr,tl),segseg(a,b,tl,bl))
def capsule(center,angle,half):
 r=math.radians(angle);u=(math.cos(r),math.sin(r));return sub(center,mul(u,half)),add(center,mul(u,half))
def test(center,angle,half):
 a,b=capsule(center,angle,half);best=1e9
 for name,mn,mx in COLLIDERS:
  d=math.sqrt(max(0,segaabb(a,b,mn,mx)))-RADIUS;best=min(best,d)
  if d<=1e-8:return False,best,name
 return True,best,''
def run(half):
 pos=(-8.,0.);angle=0.;elapsed=0;minimum=1e9
 def step(direction,tier):
  nonlocal pos,angle,elapsed,minimum
  cand=(pos[0]+direction[0]*SPEED[tier]*DT,pos[1]+direction[1]*SPEED[tier]*DT);nang=(angle-ANG*DT)%360
  # interpolation slices, same bounds as C#; this actual step is already small
  travel=math.dist(pos,cand); da=((nang-angle+180)%360)-180
  slices=max(1,math.ceil(travel/max(.01,RADIUS*.25)),math.ceil(abs(da)/1))
  for i in range(slices+1):
   t=i/slices;c=(pos[0]+(cand[0]-pos[0])*t,pos[1]+(cand[1]-pos[1])*t);a=angle+da*t
   ok,cl,name=test(c,a,half);minimum=min(minimum,cl)
   if not ok:return False,name
  pos=cand;angle=nang;elapsed+=DT;return True,''
 for action in ACTIONS:
  if action[0]=='wait':
   for _ in range(math.ceil(action[1]*HZ)):
    ok,name=step((0,0),'precision')
    if not ok:return False,pos,angle,elapsed,minimum,name
   continue
  target,tier,maxs=action; reached=False
  for _ in range(math.ceil(maxs*HZ)):
   dx,dy=target[0]-pos[0],target[1]-pos[1];d2=dx*dx+dy*dy
   if d2<=.0009:reached=True;break
   d=math.sqrt(d2);ok,name=step((dx/d,dy/d),tier)
   if not ok:return False,pos,angle,elapsed,minimum,name
  if not reached and (target[0]-pos[0])**2+(target[1]-pos[1])**2>.0036:return False,pos,angle,elapsed,minimum,'timeout'
 cleared=(pos[0]-10)**2+(pos[1]-0)**2<=.65**2
 return cleared,pos,angle,elapsed,minimum,'' if cleared else 'finish'
for mode,half in [('normal',1.44),('assist',1.08)]:
 r=run(half);print(mode,r)
 assert r[0],r
 assert r[4]>0,r
print('PASS W01-01 independent no-hit reference oracle')
