using System.Collections;
using UnityEngine;

namespace BOIFPS
{
    // Flying and wall movement use geometry probes rather than a ground NavMeshAgent.
    public sealed class DungeonSurfaceEnemy : MonoBehaviour
    {
        DungeonEnemy enemy;
        Transform[] limbs;
        Vector3 wallNormal=Vector3.right;
        float orbit, direction=1;
        bool attacking;
        public Vector3 WallNormal => wallNormal;
        public bool Attached { get; private set; }
        public string MotionState { get; private set; }
        float Ceiling => enemy.Room.IsCombat ? 6.4f : 4.8f;
        public void Initialize(DungeonEnemy owner)
        {
            enemy=owner; orbit=owner.Room.Enemies.Count*1.7f; direction=owner.Room.Enemies.Count%2==0?1:-1;
            limbs=new Transform[owner.Kind==4?2:6];
            for(int i=0;i<limbs.Length;i++)
            {
                var go=DungeonGame.Instance.Cube(owner.Kind==4?"Aile":"Patte murale",transform.position,owner.Kind==4?new Vector3(1.3f,.09f,.65f):new Vector3(.65f,.1f,.12f),owner.Kind==4?new Color(.42f,.22f,.12f):new Color(.2f,.4f,.16f),transform);
                go.GetComponent<Collider>().enabled=false; Destroy(go.GetComponent<Collider>());
                limbs[i]=go.transform;
                limbs[i].localPosition=owner.Kind==4?new Vector3(i==0?-.9f:.9f,0,0):new Vector3(i%2==0?-.65f:.65f,-.2f,(i/2-1)*.4f);
            }
            if(owner.Kind==5) Attach();
            StartCoroutine(Brain());
        }
        bool TerrainRay(Vector3 origin,Vector3 direction,float length,out RaycastHit result)
        {
            var hits=Physics.RaycastAll(origin,direction,length,~0,QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponentInParent<DungeonEnemy>()!=null || hit.collider.GetComponentInParent<DungeonPlayer>()!=null) continue;
                result=hit; return true;
            }
            result=default(RaycastHit); return false;
        }
        void Attach()
        {
            Vector3 c=enemy.Room.transform.position;
            Vector3 origin=c+new Vector3(0,Mathf.Min(3.4f,Ceiling),Mathf.Clamp(transform.position.z-c.z,-8,8));
            Vector3 outward=transform.position.x<c.x?Vector3.left:Vector3.right;
            if(TerrainRay(origin,outward,13,out var wall) && Mathf.Abs(wall.normal.y)<.4f)
            {
                wallNormal=wall.normal; transform.position=wall.point+wallNormal*.45f; Attached=true;
                transform.rotation=Quaternion.LookRotation(Vector3.Cross(Vector3.up,wallNormal)*direction,wallNormal);
            }
            else Debug.LogError("Tisse-mur : aucune paroi à l’apparition dans "+enemy.Room.name);
        }
        void Update()
        {
            var g=DungeonGame.Instance;
            if(g.Paused || g.Finished || enemy==null) return;
            if(!attacking)
            {
                if(enemy.Kind==4)
                {
                    MotionState="Orbiter"; orbit+=Time.deltaTime*.95f*direction;
                    Vector3 goal=g.Player.transform.position+new Vector3(Mathf.Cos(orbit)*6,2.7f+Mathf.Sin(Time.time*2)*.3f,Mathf.Sin(orbit)*6);
                    goal.y=Mathf.Clamp(goal.y,2.6f,Ceiling); MoveAir(goal,7.5f,2.2f);
                    Vector3 facing=g.Player.transform.position+Vector3.up-transform.position;
                    if(facing.sqrMagnitude>.01f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(facing),Time.deltaTime*5);
                }
                else Crawl();
            }
            for(int i=0;i<limbs.Length;i++)
                limbs[i].localRotation=Quaternion.Euler(enemy.Kind==4?0:Mathf.Sin(Time.time*20+i)*20,enemy.Kind==4?0:(i/2-1)*25,enemy.Kind==4?Mathf.Sin(Time.time*22)*(i==0?35:-35):Mathf.Sin(Time.time*20+i)*15);
        }
        void Crawl()
        {
            if(!Attached) { Attach(); return; }
            MotionState="Ramper";
            Vector3 tangent=Vector3.Cross(Vector3.up,wallNormal)*direction;
            Vector3 height=Vector3.up*Mathf.Clamp(DungeonGame.Instance.Player.transform.position.y+2.2f-transform.position.y,-1,1)*1.3f;
            float step=4.8f*Time.deltaTime;
            if(TerrainRay(transform.position,tangent,step+.55f,out var corner) && Mathf.Abs(corner.normal.y)<.4f)
            {
                wallNormal=corner.normal; transform.position=corner.point+wallNormal*.45f;
                tangent=Vector3.Cross(Vector3.up,wallNormal)*direction;
            }
            Vector3 candidate=transform.position+tangent*step+height*Time.deltaTime;
            Vector3 c=enemy.Room.transform.position; candidate.y=Mathf.Clamp(candidate.y,1.8f,Ceiling);
            if(TerrainRay(candidate,-wallNormal,1.1f,out var wall) && Mathf.Abs(wall.normal.y)<.4f)
            {
                wallNormal=wall.normal; transform.position=wall.point+wallNormal*.45f;
                transform.rotation=Quaternion.LookRotation((tangent+height*.3f).normalized,wallNormal);
            }
            else direction=-direction; // Reverse at a doorway or a broken surface.
        }
        bool MoveAir(Vector3 goal,float speed,float minHeight)
        {
            Vector3 c=enemy.Room.transform.position;
            goal.x=Mathf.Clamp(goal.x,c.x-10.5f,c.x+10.5f); goal.z=Mathf.Clamp(goal.z,c.z-10.5f,c.z+10.5f); goal.y=Mathf.Clamp(goal.y,minHeight,Ceiling);
            Vector3 delta=Vector3.ClampMagnitude(goal-transform.position,speed*Time.deltaTime);
            if(delta.sqrMagnitude<.000001f) return false;
            bool moved=false;
            for(int pass=0;pass<2;pass++)
            {
                if(delta.sqrMagnitude<.000001f) break;
                var hits=Physics.SphereCastAll(transform.position,.6f,delta.normalized,delta.magnitude+.04f,~0,QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                bool blocked=false;
                foreach(var hit in hits)
                {
                    if(hit.collider.GetComponentInParent<DungeonEnemy>()!=null || hit.collider.GetComponentInParent<DungeonPlayer>()!=null) continue;
                    float safe=Mathf.Clamp(hit.distance-.04f,0,delta.magnitude);
                    transform.position+=delta.normalized*safe; moved|=safe>.0001f;
                    delta=Vector3.ProjectOnPlane(delta.normalized*(delta.magnitude-safe),hit.normal);
                    blocked=true; orbit+=Time.deltaTime; break;
                }
                if(!blocked) { transform.position+=delta; return true; }
            }
            return moved;
        }
        IEnumerator Brain()
        {
            yield return new WaitForSeconds(.4f+(enemy.Room.Enemies.Count%4)*.05f);
            int shots=0;
            while(enemy!=null && !DungeonGame.Instance.Finished)
            {
                if(!enemy.Sight() || !enemy.AttackAvailable) { yield return new WaitForSeconds(.25f); continue; }
                attacking=true; enemy.Attacking=true;
                var player=DungeonGame.Instance.Player;
                if(enemy.Kind==4 && shots%3==2)
                {
                    MotionState="Annoncer un piqué";
                    Vector3 locked=player.transform.position+Vector3.up*.9f;
                    yield return enemy.Tell(.6f);
                    MotionState="Piquer"; float until=Time.time+.45f; bool damaged=false;
                    while(Time.time<until)
                    {
                        MoveAir(locked,17,.75f);
                        if(!damaged && Vector3.Distance(transform.position,player.transform.position+Vector3.up*.9f)<1.25f) { player.Damage(14,transform.position); damaged=true; }
                        yield return null;
                    }
                    MotionState="Remonter";
                    float recover=Time.time+.5f;
                    while(Time.time<recover) { MoveAir(transform.position+Vector3.up*2,4,.75f); yield return null; }
                }
                else
                {
                    MotionState="Annoncer un tir"; yield return enemy.Tell(enemy.Kind==4?.5f:.55f);
                    if(enemy.Sight())
                    {
                        Vector3 aim=(player.transform.position+Vector3.up+Vector3.ClampMagnitude(player.PlanarVelocity*.1f,1)-transform.position).normalized;
                        if(enemy.Kind==5 && shots%2==1)
                        {
                            MotionState="Éventail mural";
                            for(int i=-1;i<=1;i++) DungeonProjectile.Spawn(transform.position+aim*.55f,Quaternion.AngleAxis(i*16,Vector3.up)*aim,11,12,false);
                        }
                        else DungeonProjectile.Spawn(transform.position+aim*.55f,aim,14,enemy.Kind==4?12:16,false);
                        DungeonGame.Instance.Sound(enemy.Kind==4?1.7f:.6f);
                    }
                }
                shots++; MotionState="Récupérer";
                yield return new WaitForSeconds(enemy.Kind==4?.8f:.9f);
                enemy.Attacking=false; attacking=false;
                yield return new WaitForSeconds(.25f);
            }
        }
    }
}
