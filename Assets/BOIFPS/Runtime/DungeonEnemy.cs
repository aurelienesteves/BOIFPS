using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BOIFPS
{
    public sealed partial class DungeonEnemy : MonoBehaviour
    {
        public int Kind { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public DungeonRoom Room { get; private set; }
        NavMeshAgent agent;
        Transform body;
        Renderer skin;
        Color baseColor;
        float burnUntil, flashUntil;
        float burnTick = float.PositiveInfinity;
        bool dead;
        bool windingUp;
        float hitReaction;
        Vector3 bodyScale, bodyPosition;
        public bool IsBurning => Time.time<burnUntil;
        public bool PhaseTwo { get; private set; }
        public bool Attacking { get; set; }
        public bool SurfaceEnemy => IsSurfaceKind(Kind);
        public static bool IsSurfaceKind(int kind) => kind==4 || kind==5;
        public Vector3 AimPoint => SurfaceEnemy?transform.position:transform.position+Vector3.up*(Kind==0?.55f:1.4f);
        public bool AttackAvailable
        {
            get { int count=0; foreach(var enemy in Room.Enemies) if(enemy!=this && enemy!=null && enemy.Attacking) count++; return count<4; }
        }
        Vector3 lastSeen;
        int lane;
        bool flanking;
        Vector3 flankWaypoint;
        float tacticUntil;
        public string Tactic { get; private set; }="Observer";
        public void Initialize(int kind, DungeonRoom room)
        {
            Kind = kind; Room = room;
            Health = MaxHealth = kind == 0 ? 18 : kind == 1 ? 36 : kind == 2 ? 90 : kind==3?1350:kind==4?30:45;
            if (IsBestiary) Health = MaxHealth = Profile.Life;
            lane=room.Enemies.Count; lastSeen=DungeonGame.Instance.Player.transform.position;
            if(!SurfaceEnemy)
            {
                agent = gameObject.AddComponent<NavMeshAgent>(); agent.radius = kind >= 2 ? .7f : .4f; agent.height = 2;
                agent.speed = kind == 0 ? 9.4f : kind == 1 ? 5.2f : 5.6f; agent.acceleration = 70; agent.angularSpeed = 720; agent.stoppingDistance = kind == 1 ? 1 : .8f;
            }
            baseColor = kind == 0 ? new Color(.62f, .48f, .43f) : kind == 1 ? new Color(.35f, .55f, .65f) : kind==4?new Color(.7f,.44f,.16f):kind==5?new Color(.35f,.65f,.30f):new Color(.42f, .32f, .27f);
            if (IsBestiary) { baseColor = Profile.Color; agent.speed = Profile.Speed; agent.radius = .45f; }
            var g = DungeonGame.Instance;
            body = g.Cube("Corps", transform.position + Vector3.up * (SurfaceEnemy?0:kind == 0 ? .55f : kind == 3 ? 2 : 1.2f), SurfaceEnemy?new Vector3(.9f,.6f,1.1f):kind == 0 ? new Vector3(1, .7f, 1.5f) : kind == 3 ? new Vector3(2.5f, 4, 2.5f) : new Vector3(kind == 2 ? 1.4f : .7f, 2.2f, .9f), baseColor, transform, PrimitiveType.Capsule).transform;
            skin = body.GetComponent<Renderer>();
            bodyScale=body.localScale; bodyPosition=body.localPosition;
            var face = g.Cube("Visage", transform.position + Vector3.up * (SurfaceEnemy?0:kind == 3 ? 3 : kind == 0 ? .7f : 1.8f) + Vector3.forward * .45f, new Vector3(.5f, .18f, .2f), Color.red, transform);
            Destroy(face.GetComponent<Collider>());
            if (kind == 0)
                for (int i = 0; i < 4; i++)
                {
                    var limb = g.Cube("Griffe", transform.position + new Vector3(i % 2 == 0 ? -.6f : .6f, .25f, i < 2 ? -.5f : .5f), new Vector3(.15f, .5f, .2f), baseColor, transform);
                    Destroy(limb.GetComponent<Collider>());
                }
            if (kind == 1)
            {
                var chain = g.Cube("Chaîne", transform.position + Vector3.up * 3.7f, new Vector3(.08f, 4, .08f), new Color(.45f,.43f,.35f), transform);
                Destroy(chain.GetComponent<Collider>());
            }
            if (kind == 2 || kind==3)
            {
                var bell = g.Cube("Cloche", transform.position + Vector3.up * (kind == 3 ? 2 : 1.2f), new Vector3(kind == 3 ? 2.8f : 1.8f, .8f, kind == 3 ? 2.8f : 1.8f), new Color(.55f, .4f, .15f), transform, PrimitiveType.Cylinder);
                Destroy(bell.GetComponent<Collider>());
            }
            if(IsBestiary) AddCrest();
            if(SurfaceEnemy) gameObject.AddComponent<DungeonSurfaceEnemy>().Initialize(this);
            else StartCoroutine(Brain());
        }
        void Update()
        {
            if (dead) return;
            while (!dead && Time.time >= burnTick && burnTick <= burnUntil) { burnTick += 1; Damage(6, false); }
            if (skin != null && Time.time > flashUntil) skin.sharedMaterial = DungeonGame.Instance.Material(Time.time<burnUntil?new Color(1,.38f,.08f):baseColor);
            hitReaction=Mathf.MoveTowards(hitReaction,0,Time.deltaTime*7);
            float stride=agent != null && agent.isOnNavMesh ? agent.velocity.magnitude : 0;
            float bob=Mathf.Sin(Time.time*(Kind==0?25:12)+Room.Index*2.3f)*(Kind==0?.08f:.045f)*Mathf.Min(stride,1);
            body.localPosition=bodyPosition+Vector3.up*(bob-(windingUp?.12f:0));
            body.localScale=Vector3.Scale(bodyScale,new Vector3(1+hitReaction*.15f,1-hitReaction*.18f,1+hitReaction*.1f));
            body.localRotation=Quaternion.Euler(windingUp?-12:stride*(Kind==0?2:.8f),0,Mathf.Sin(Time.time*14)*stride*.3f);
        }
        public bool Sight()
        {
            Vector3 from = AimPoint;
            Vector3 to = DungeonGame.Instance.Player.transform.position + Vector3.up;
            var hits=Physics.RaycastAll(from,(to-from).normalized,(to-from).magnitude,~0,QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponentInParent<DungeonEnemy>()!=null) continue;
                return hit.collider.GetComponentInParent<DungeonPlayer>()!=null;
            }
            return true;
        }
        IEnumerator Brain()
        {
            yield return new WaitForSeconds(.18f+(lane%4)*.05f);
            int attack = 0;
            while (!dead)
            {
                var g = DungeonGame.Instance;
                if (g.Finished) yield break;
                Attacking=false;
                if (!agent.isOnNavMesh) { Debug.LogError(name + " hors NavMesh"); yield break; }
                bool visible=Sight();
                if(visible) lastSeen=g.Player.transform.position;
                Vector3 target = lastSeen;
                float distance = Vector3.Distance(target, transform.position);
                agent.isStopped = false;
                if (IsBestiary)
                {
                    if (distance <= Profile.Range && visible && AttackAvailable) yield return BestiaryAttack(target);
                    else { MoveTo(target); yield return new WaitForSeconds(.15f); }
                }
                else if (Kind == 3)
                {
                    agent.isStopped = true;
                    if (!PhaseTwo && Health <= MaxHealth * .5f) { PhaseTwo = true; yield return Tell(1.1f); }
                    if (attack % 3 == 0)
                    {
                        yield return Tell(.6f);
                        Ring(0); if (PhaseTwo) { yield return new WaitForSeconds(.3f); Ring(11.25f); }
                    }
                    else if (attack % 3 == 1) yield return Charge(.65f, 16);
                    else
                    {
                        Vector3 center = DungeonDanger.GroundBelow(g.Player.transform.position);
                        DungeonDanger.Spawn(center);
                        // Side circles preserve a clear retreat lane.
                        DungeonDanger.Spawn(DungeonDanger.GroundBelow(ClampToRoom(center + Vector3.right * 4)));
                        DungeonDanger.Spawn(DungeonDanger.GroundBelow(ClampToRoom(center + Vector3.left * 4)));
                        g.Sound(.4f); yield return new WaitForSeconds(1.1f);
                    }
                    attack++; yield return new WaitForSeconds(PhaseTwo ? .35f : .6f);
                }
                else if (Kind == 0)
                {
                    MoveTo(Approach(target));
                    if (distance < 3.6f && (!flanking || distance<1.8f) && visible && Mathf.Abs(target.y-transform.position.y)<1.6f && AttackAvailable)
                    {
                        if((attack++ + lane)%3==2) yield return ClawSweep();
                        else yield return Charge(.25f, 3.8f);
                    }
                    else yield return new WaitForSeconds(.1f);
                }
                else if (Kind == 1)
                {
                    if(distance<6 && visible)
                    {
                        Tactic="Décrocher";
                        MoveTo(transform.position+(transform.position-target).normalized*4); yield return new WaitForSeconds(.3f);
                    }
                    else if (distance <= 17 && visible && AttackAvailable)
                    {
                        int pattern=(attack++ + lane)%3;
                        Tactic=pattern==0?"Éventail double":pattern==1?"Rafale de précision":"Salve en étages";
                        agent.isStopped = true; yield return Tell(pattern==1?.55f:.45f);
                        Vector3 locked=(g.Player.transform.position+Vector3.up+Vector3.ClampMagnitude(g.Player.PlanarVelocity*.2f,2)-AimPoint).normalized;
                        int volleys=pattern==1?3:2;
                        for(int volley=0;volley<volleys;volley++)
                        {
                            if(pattern==1) DungeonProjectile.Spawn(AimPoint,locked,18,12,false);
                            else
                            {
                                var aim=(g.Player.transform.position+Vector3.up-AimPoint).normalized;
                                for(int i=-1;i<=1;i++)
                                    DungeonProjectile.Spawn(AimPoint,pattern==2?Quaternion.AngleAxis(i*12,Vector3.Cross(aim,Vector3.up).normalized)*aim:Quaternion.Euler(0,i*15,0)*aim,pattern==2?10:12.5f,14,false);
                            }
                            yield return new WaitForSeconds(.18f);
                        }
                        agent.isStopped=false;
                        MoveTo(SidePoint(target,lane%2==0?1:-1,35,Mathf.Clamp(distance,7,12)));
                        yield return new WaitForSeconds(.65f);
                    }
                    else { Tactic="Chercher un angle"; MoveTo(SidePoint(target,lane%2==0?1:-1,55,9)); yield return new WaitForSeconds(.2f); }
                }
                else
                {
                    MoveTo(Approach(target));
                    if(distance<14 && visible && AttackAvailable && (Mathf.Abs(target.y-transform.position.y)>1 || (attack++ + lane)%3==2)) yield return Burial();
                    else if (distance < 14 && distance > 2.5f && visible && Mathf.Abs(target.y-transform.position.y)<1 && AttackAvailable) yield return Charge(.62f, 12);
                    else if(distance<=2.5f && visible && AttackAvailable)
                    {
                        yield return Tell(.34f);
                        if(Vector3.Distance(g.Player.transform.position,transform.position)<3) g.Player.Damage(18,transform.position);
                        yield return new WaitForSeconds(.4f);
                    }
                    else yield return new WaitForSeconds(.2f);
                }
            }
        }
        IEnumerator ClawSweep()
        {
            Tactic="Balayage des griffes";
            Vector3 direction=Vector3.ProjectOnPlane(DungeonGame.Instance.Player.transform.position-transform.position,Vector3.up).normalized;
            yield return Tell(.48f);
            var delta=DungeonGame.Instance.Player.transform.position-transform.position;
            if(delta.magnitude<3.1f && Vector3.Dot(delta.normalized,direction)>.35f)
                DungeonGame.Instance.Player.Damage(15,transform.position);
            yield return new WaitForSeconds(.55f);
        }
        IEnumerator Burial()
        {
            Tactic="Sépulture ciblée";
            Vector3 locked=DungeonDanger.GroundBelow(DungeonGame.Instance.Player.transform.position);
            // The target is locked before the tell: walking or jumping can evade it.
            DungeonDanger.Spawn(locked,1.8f,.95f,18);
            yield return Tell(.65f);
            yield return new WaitForSeconds(.7f);
        }
        Vector3 Approach(Vector3 target)
        {
            var player=DungeonGame.Instance.Player;
            if(Kind==2)
            {
                Tactic=lane%2==0?"Poursuite lourde":"Intercepter";
                return target+(lane%2==0?Vector3.zero:Vector3.ClampMagnitude(player.PlanarVelocity*.6f,4));
            }
            if(lane%3==0) { Tactic="Poursuite directe"; return target+Vector3.ClampMagnitude(player.PlanarVelocity*.12f,1); }
            int side=lane%3==1?1:-1;
            if(Time.time>=tacticUntil || flanking && Vector3.Distance(transform.position,flankWaypoint)<1.2f)
            {
                if(flanking) { flanking=false; tacticUntil=Time.time+1.1f; }
                else
                {
                    flanking=true; tacticUntil=Time.time+2.2f;
                    flankWaypoint=SidePoint(target,side,75,Mathf.Clamp(Vector3.Distance(target,transform.position)*.65f,4.5f,7.5f));
                }
            }
            Tactic=flanking?(side==1?"Contourner à droite":"Contourner à gauche"):"Refermer le piège";
            return flanking?flankWaypoint:target+Vector3.ClampMagnitude(player.PlanarVelocity*.35f,2.5f);
        }
        Vector3 SidePoint(Vector3 target,int side,float angle,float radius)
        {
            Vector3 radial=Vector3.ProjectOnPlane(transform.position-target,Vector3.up).normalized;
            if(radial.sqrMagnitude<.01f) radial=Vector3.forward;
            foreach(float turn in new[]{angle,angle+25,angle-25})
            {
                Vector3 desired=ClampToRoom(target+Quaternion.Euler(0,side*turn,0)*radial*radius);
                if(!NavMesh.SamplePosition(desired,out var hit,2,NavMesh.AllAreas)) continue;
                var route=new NavMeshPath();
                if(agent.CalculatePath(hit.position,route) && route.status==NavMeshPathStatus.PathComplete) return hit.position;
            }
            return target;
        }
        void MoveTo(Vector3 desired)
        {
            if(!agent.isOnNavMesh) return;
            desired=ClampToRoom(desired);
            if(!NavMesh.SamplePosition(desired,out var hit,2.5f,NavMesh.AllAreas)) return;
            var path=new NavMeshPath();
            if(agent.CalculatePath(hit.position,path) && path.status==NavMeshPathStatus.PathComplete) agent.SetPath(path);
            else if(NavMesh.SamplePosition(lastSeen,out hit,3,NavMesh.AllAreas)) agent.SetDestination(hit.position);
        }
        Vector3 ClampToRoom(Vector3 p)
        {
            Vector3 c = Room.transform.position; p.x = Mathf.Clamp(p.x, c.x - 9, c.x + 9); p.z = Mathf.Clamp(p.z, c.z - 9, c.z + 9); return p;
        }
        public IEnumerator Tell(float seconds)
        {
            windingUp=true; Attacking=true;
            if (agent!=null && agent.isOnNavMesh) agent.isStopped = true;
            Vector3 target = DungeonGame.Instance.Player.transform.position;
            if(!SurfaceEnemy) target.y = transform.position.y;
            if (Kind!=5 && (target - transform.position).sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(target - transform.position);
            flashUntil = Time.time + seconds; skin.sharedMaterial = DungeonGame.Instance.Material(new Color(1, .45f, .12f));
            DungeonGame.Instance.Sound(Kind >= 2 ? .5f : 1.2f);
            yield return new WaitForSeconds(seconds);
            windingUp=false;
        }
        IEnumerator Charge(float tell, float length)
        {
            Vector3 direction = DungeonGame.Instance.Player.transform.position - transform.position; direction.y = 0; direction.Normalize();
            GameObject mark=null;
            if(Kind>=2)
            {
                mark = DungeonGame.Instance.Cube("Axe de charge", transform.position + direction * (length * .5f) + Vector3.up * .04f, new Vector3(.6f, .04f, length), new Color(.85f, .25f, .05f));
                mark.GetComponent<Collider>().enabled=false; Destroy(mark.GetComponent<Collider>()); mark.transform.rotation = Quaternion.LookRotation(direction);
                Destroy(mark, tell + .1f);
            }
            yield return Tell(tell);
            Destroy(mark);
            bool hitPlayer = false; float traveled = 0;
            while (traveled < length)
            {
                float step = Mathf.Min((Kind==0?18:24) * Time.deltaTime,length-traveled); Vector3 next = transform.position + direction * step;
                if (NavMesh.Raycast(transform.position, next, out var hit, NavMesh.AllAreas)) break;
                if (!NavMesh.SamplePosition(next, out var ground, .6f, NavMesh.AllAreas)) break;
                if(Physics.SphereCast(transform.position+Vector3.up*.8f,agent.radius,direction,out var wall,step,~0,QueryTriggerInteraction.Ignore) && wall.collider.GetComponentInParent<DungeonEnemy>()==null && wall.collider.GetComponentInParent<DungeonPlayer>()==null) break;
                agent.Warp(ground.position); traveled += step;
                if (!hitPlayer && Vector3.Distance(DungeonGame.Instance.Player.transform.position, transform.position) < (Kind==0?1.3f:1.7f))
                { DungeonGame.Instance.Player.Damage(Kind==0?12:26, transform.position); hitPlayer = true; }
                yield return null;
            }
            yield return new WaitForSeconds(Kind == 0 ? .28f : Kind==3?.8f:.5f);
        }
        void Ring(float angle)
        {
            for (int n = 0; n < 16; n++) DungeonProjectile.Spawn(transform.position + Vector3.up, Quaternion.Euler(0, angle + n * 22.5f, 0) * Vector3.forward, 9.5f, 20, false);
            DungeonGame.Instance.Sound(.35f);
        }
        public void Damage(float amount, bool burn, Vector3 impulse = default(Vector3), bool relics=true)
        {
            if (dead) return;
            Health -= amount;
            if(impulse.sqrMagnitude>.001f)
            {
                hitReaction=1;
                if(agent!=null && agent.isOnNavMesh) agent.Move(impulse.normalized*(Kind==3?.05f:.22f));
            }
            if (burn) { if (Time.time >= burnUntil) burnTick = Time.time + 1; burnUntil = Time.time + 4; }
            flashUntil = Time.time + .08f; skin.sharedMaterial = DungeonGame.Instance.Material(Color.white);
            if (Health <= 0)
            {
                dead = true; Room.Enemies.Remove(this);
                if(IsBurning && DungeonGame.Instance.Player.Has(3)) DungeonGame.Instance.Player.GetComponent<DungeonRelics>().QueueFire(AimPoint,Room);
                if(relics && DungeonGame.Instance.Player.Has(6)) DungeonRelics.Souls(AimPoint,Room);
                DungeonFeedback.Burst(AimPoint,impulse,true,true); DungeonGame.Instance.ImpactSound(true); Destroy(gameObject);
            }
        }
    }
}
