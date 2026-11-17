using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BOIFPS
{
    public sealed class DungeonRelics : MonoBehaviour
    {
        DungeonPlayer player;
        LineRenderer aura;
        struct FireDeath { public Vector3 Point; public DungeonRoom Room; }
        readonly Queue<FireDeath> fireDeaths=new Queue<FireDeath>();
        public static bool IsTransformation(int id) => id==2 || id==3 || id==7 || id==10;
        public void QueueFire(Vector3 point,DungeonRoom room)
        {
            if(fireDeaths.Count<64) fireDeaths.Enqueue(new FireDeath { Point=point,Room=room });
        }
        public void ClearChains() { fireDeaths.Clear(); }
        void SpreadFire()
        {
            if(DungeonGame.Instance.Paused) return;
            if(DungeonGame.Instance.Finished) { fireDeaths.Clear(); return; }
            // Limit work per frame; dead enemies are removed before queuing a new blast.
            for(int n=0;n<4 && fireDeaths.Count>0;n++)
            {
                var death=fireDeaths.Dequeue(); if(death.Room==null) continue;
                Notify(3,"Embrasement • propagation à 4 m",death.Point);
                var wave=Ring("Propagation de la moelle",ColorFor(3)); wave.transform.position=death.Point;
                wave.gameObject.AddComponent<DungeonRelicPulse>().Radius=4;
                foreach(var enemy in death.Room.Enemies.ToArray())
                    if(enemy!=null && Vector3.Distance(enemy.AimPoint,death.Point)<=4 && ClearPath(death.Point,enemy.AimPoint))
                        enemy.Damage(14,true,Vector3.zero,false);
            }
        }
        public static Color ColorFor(int id)
        {
            switch(id)
            {
                case 2: return new Color(1,.65f,.3f);
                case 3: return new Color(1,.5f,.15f);
                case 6: return new Color(.75f,.5f,1);
                case 7: return new Color(1,.85f,.35f);
                case 8: return new Color(.35f,.65f,1);
                case 9: return new Color(.9f,.35f,1);
                case 10: return new Color(.55f,.9f,1);
                case 11: return new Color(.7f,1,.75f);
                default: return new Color(.85f,.8f,.6f);
            }
        }
        public static void Notify(int id,string message,Vector3 point)
        {
            var g=DungeonGame.Instance;
            if(g.Hud!=null) g.Hud.RelicFeedback(id,message);
            var ring=Ring(DungeonGame.ItemNames[id]+" — activation",ColorFor(id));
            ring.transform.position=point; ring.gameObject.AddComponent<DungeonRelicPulse>();
        }
        void Start() { player=GetComponent<DungeonPlayer>(); }
        void Update()
        {
            if(player==null) return;
            SpreadFire();
            bool visible=player.Has(8) && player.Reloading;
            if(visible && aura==null) aura=Ring("Sablier affamé",new Color(.3f,.6f,1));
            if(aura!=null)
            {
                aura.gameObject.SetActive(visible);
                if(visible) DrawRing(aura,player.transform.position+Vector3.up*.08f,5);
            }
        }
        void OnDestroy() { if(aura!=null) Destroy(aura.gameObject); }
        public static LineRenderer Ring(string label,Color color)
        {
            var go=new GameObject(label); var line=go.AddComponent<LineRenderer>();
            line.sharedMaterial=DungeonGame.Instance.Emissive(color,2); line.positionCount=40; line.loop=true; line.widthMultiplier=.06f; return line;
        }
        public static void DrawRing(LineRenderer line,Vector3 center,float radius)
        {
            for(int i=0;i<40;i++) { float a=i*Mathf.PI/20; line.SetPosition(i,center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius); }
        }
        public static List<DungeonEnemy> Targets()
        {
            var list=new List<DungeonEnemy>();
            foreach(var room in DungeonGame.Instance.Rooms) foreach(var enemy in room.Enemies) if(enemy!=null && enemy.Health>0) list.Add(enemy);
            return list;
        }
        public static DungeonEnemy Nearest(Vector3 point,float range)
        {
            DungeonEnemy best=null; float distance=range*range;
            foreach(var enemy in Targets()) { float d=(enemy.AimPoint-point).sqrMagnitude; if(d<distance) { best=enemy; distance=d; } }
            return best;
        }
        public static bool ClearPath(Vector3 from,Vector3 to)
        {
            foreach(var hit in Physics.RaycastAll(from,(to-from).normalized,Vector3.Distance(from,to),~0,QueryTriggerInteraction.Ignore))
                if(hit.collider.GetComponentInParent<DungeonEnemy>()==null && hit.collider.GetComponentInParent<DungeonPlayer>()==null) return false;
            return true;
        }
        public static void Souls(Vector3 position,DungeonRoom room)
        {
            var target=Nearest(position,22); if(target==null) return;
            Notify(6,"3 esprits libérés",position);
            Vector3 aim=(target.AimPoint-position).normalized;
            for(int i=-1;i<=1;i++) DungeonProjectile.Spawn(position+Vector3.up*.1f,Quaternion.Euler(0,i*25,0)*aim,22,8,true,1,false,true,false,true);
        }
        public static IEnumerator DashMines(DungeonPlayer player)
        {
            for(int i=0;i<3;i++)
            {
                if(DungeonGame.Instance.Finished) yield break;
                if(Physics.Raycast(player.transform.position+Vector3.up*.2f,Vector3.down,out var floor,4,~0,QueryTriggerInteraction.Ignore) && floor.point.y>=-.05f)
                    DungeonMine.Spawn(floor.point+Vector3.up*.05f);
                yield return new WaitForSeconds(.06f);
            }
        }
        public static void CreateOrbitals(DungeonPlayer player)
        {
            int count=FindObjectsByType<DungeonOrbital>().Length;
            if(count<4) Notify(11,"+"+Mathf.Min(2,4-count)+" satellites • 8 s",player.transform.position+Vector3.up*.1f);
            for(int i=0;i<2 && count+i<4;i++)
            {
                var go=DungeonGame.Instance.Cube("Satellite d’os",player.transform.position, new Vector3(.13f,.3f,.13f),ColorFor(11),null,PrimitiveType.Capsule);
                go.GetComponent<Collider>().enabled=false; Destroy(go.GetComponent<Collider>());
                var orbital=go.AddComponent<DungeonOrbital>(); orbital.Owner=player; orbital.Phase=(count==0?0:Mathf.PI*.5f)+i*Mathf.PI;
            }
        }
    }
    public sealed class DungeonRelicPulse : MonoBehaviour
    {
        float life;
        public float Radius=1.6f;
        void Update()
        {
            life+=Time.deltaTime;
            var line=GetComponent<LineRenderer>();
            DungeonRelics.DrawRing(line,transform.position,.25f+Mathf.Clamp01(life/.45f)*(Radius-.25f));
            line.widthMultiplier=.08f*Mathf.Clamp01(1-life/.45f);
            if(life>=.45f) Destroy(gameObject);
        }
    }
    public sealed class DungeonMine : MonoBehaviour
    {
        float timer;
        public static void Spawn(Vector3 position)
        {
            var go=DungeonGame.Instance.Cube("Épine du vide",position,new Vector3(.35f,.18f,.35f),new Color(.6f,.2f,1),null,PrimitiveType.Sphere);
            go.GetComponent<Collider>().enabled=false; Destroy(go.GetComponent<Collider>()); go.AddComponent<DungeonMine>();
        }
        void Update()
        {
            timer+=Time.deltaTime; transform.localScale=Vector3.one*(.25f+timer*.3f);
            if(timer<.55f) return;
            foreach(var enemy in DungeonRelics.Targets())
                if(Vector3.Distance(enemy.AimPoint,transform.position)<2.5f && DungeonRelics.ClearPath(transform.position+Vector3.up*.2f,enemy.AimPoint)) enemy.Damage(22,false,(enemy.AimPoint-transform.position).normalized,false);
            DungeonRelics.Notify(9,"Mine déclenchée • rayon 2,5 m",transform.position); DungeonFeedback.Burst(transform.position,Vector3.up,false,true); DungeonGame.Instance.ImpactSound(true); Destroy(gameObject);
        }
    }
    public sealed class DungeonOrbital : MonoBehaviour
    {
        public DungeonPlayer Owner;
        public float Phase;
        float life=8;
        Renderer skin;
        void Start() { skin=GetComponent<Renderer>(); }
        void Update()
        {
            if(Owner==null || (life-=Time.deltaTime)<=0 || DungeonGame.Instance.Finished) { Destroy(gameObject); return; }
            Vector3 center=Owner.transform.position+Vector3.up*1.2f;
            float angle=Time.time*4+Phase;
            transform.position=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*1.3f;
            bool clear=DungeonRelics.ClearPath(center,transform.position); skin.enabled=clear; if(!clear) return;
            foreach(var bullet in DungeonProjectile.Active)
                if(bullet!=null && !bullet.Friendly && Vector3.Distance(bullet.transform.position,transform.position)<.55f && bullet.Absorb()) { DungeonRelics.Notify(11,"Projectile absorbé",transform.position); Destroy(gameObject); return; }
            foreach(var collider in Physics.OverlapSphere(transform.position,.4f,~0,QueryTriggerInteraction.Ignore))
            {
                var enemy=collider.GetComponentInParent<DungeonEnemy>();
                if(enemy!=null) { DungeonRelics.Notify(11,"Impact orbital • 18 dégâts",transform.position); enemy.Damage(18,false,(enemy.AimPoint-center).normalized,false); Destroy(gameObject); return; }
            }
        }
    }
}
