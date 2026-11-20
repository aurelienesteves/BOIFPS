using System.Collections.Generic;
using UnityEngine;

namespace BOIFPS
{
    public sealed class DungeonProjectile : MonoBehaviour
    {
        Vector3 direction;
        float speed, damage, remaining;
        bool friendly, burning;
        bool secondary,boomerang,returning,homing,absorbed;
        int pierce;
        Renderer skin;
        TrailRenderer trail;
        Color tint;
        bool wasSlowed, falloff, empowered;
        float traveled, lifetime;
        public Vector3 Direction => direction;
        public bool Boomerang => boomerang;
        public float DamageAmount => damage;
        public float RemainingRange => remaining;
        public static readonly HashSet<DungeonProjectile> Active=new HashSet<DungeonProjectile>();
        public bool Friendly => friendly;
        public bool Secondary => secondary;
        public bool Returning => returning;
        public float SpeedMultiplier => !friendly && DungeonGame.Instance.Player.Has(8) && DungeonGame.Instance.Player.Reloading && Vector3.Distance(transform.position,DungeonGame.Instance.Player.transform.position)<5 ? .25f : 1;
        readonly HashSet<DungeonEnemy> struck = new HashSet<DungeonEnemy>();
        public static DungeonProjectile Spawn(Vector3 p, Vector3 direction, float speed, float damage, bool friendly, int pierce = 1, bool burning = false,bool secondary=false,bool boomerang=false,bool homing=false,float range=-1,bool falloff=false,bool empowered=false)
        {
            var g = DungeonGame.Instance;
            var go = g.Cube(friendly ? "Pointe" : "Projectile hostile", p, Vector3.one * (friendly ? .12f : .35f), friendly ? Color.cyan : new Color(1, .25f, .08f), null, PrimitiveType.Sphere);
            go.GetComponent<Collider>().enabled=false; Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial=g.Emissive(friendly ? Color.cyan : new Color(1,.25f,.08f),3);
            var bullet = go.AddComponent<DungeonProjectile>(); bullet.direction = direction.normalized; bullet.speed = speed; bullet.damage = damage;
            bullet.friendly = friendly; bullet.pierce = pierce; bullet.burning = burning; bullet.remaining = range>0?range:boomerang?11:friendly?40:70; bullet.falloff=falloff; bullet.empowered=empowered;
            bullet.secondary=secondary; bullet.boomerang=boomerang; bullet.homing=homing; Active.Add(bullet);
            bullet.tint=empowered?DungeonRelics.ColorFor(10):!friendly?new Color(1,.25f,.08f):homing?DungeonRelics.ColorFor(6):boomerang?DungeonRelics.ColorFor(7):burning?DungeonRelics.ColorFor(3):Color.cyan;
            bullet.skin=go.GetComponent<Renderer>(); bullet.skin.sharedMaterial=g.Emissive(bullet.tint,3);
            bullet.trail = go.AddComponent<TrailRenderer>(); bullet.trail.sharedMaterial = g.Emissive(bullet.tint,2); bullet.trail.time = .12f; bullet.trail.startWidth = .09f; bullet.trail.endWidth = 0;
            return bullet;
        }
        DungeonWeapon weapon;
        WeaponTrait traits;
        bool bounced, planted, shieldSpent;
        float contactRadius=.07f;
        int curveSign;
        bool Forged(WeaponTrait trait) => (traits & trait)!=0;
        public void Imprint(DungeonWeapon kind,WeaponTrait modifiers,int handedness=1)
        {
            weapon=kind; traits=modifiers; curveSign=handedness;
            if(Forged(WeaponTrait.Allongé)) remaining*=1.5f;
            if(Forged(WeaponTrait.Fulgurant)) speed*=1.4f;
            if(kind==DungeonWeapon.Égide) contactRadius=.5f;
            if(Forged(WeaponTrait.Massif)) contactRadius*=2;
            transform.localScale*=contactRadius/.07f;
            if((int)kind>=3 && !empowered && !burning && !boomerang)
                skin.sharedMaterial=trail.sharedMaterial=DungeonGame.Instance.Emissive(DungeonArmament.Hue(kind),3);
            if(kind==DungeonWeapon.Semeur) { transform.localScale*=2; trail.startWidth=.18f; }
        }
        void Plant(Vector3 point,Transform target,float power)
        {
            if(weapon!=DungeonWeapon.Semeur || planted) return;
            planted=true; DungeonSeedCharge.Plant(point,target,power*2,burning);
        }
        void Guide()
        {
            DungeonEnemy best=null; float nearest=8;
            var room=DungeonGame.Instance.Current;
            if(room==null) return;
            foreach(var enemy in room.Enemies)
            {
                var to=enemy.AimPoint-transform.position;
                if(to.magnitude>=nearest || Vector3.Dot(direction,to.normalized)<.55f || !DungeonRelics.ClearPath(transform.position,enemy.AimPoint)) continue;
                nearest=to.magnitude; best=enemy;
            }
            if(best!=null) direction=Vector3.RotateTowards(direction,(best.AimPoint-transform.position).normalized,3*Time.deltaTime,0);
        }
        void Update()
        {
            if(absorbed) return;
            if((lifetime+=Time.deltaTime)>8 || DungeonGame.Instance.Finished) { Destroy(gameObject); return; }
            if(returning)
            {
                Vector3 toPlayer=DungeonGame.Instance.Player.View.transform.position-transform.position;
                if(toPlayer.magnitude<.6f) { Destroy(gameObject); return; }
                direction=toPlayer.normalized;
            }
            else if(homing)
            {
                var target=DungeonRelics.Nearest(transform.position,25);
                if(target!=null) direction=Vector3.RotateTowards(direction,(target.AimPoint-transform.position).normalized,7*Time.deltaTime,0);
            }
            if(!returning && (traits&WeaponTrait.Aimanté)!=0) Guide();
            if(!returning)
            {
                if(weapon==DungeonWeapon.Comète) speed+=75*Time.deltaTime;
                if(weapon==DungeonWeapon.Faucille || weapon==DungeonWeapon.Caducée)
                {
                    float turn=weapon==DungeonWeapon.Faucille?50:135*Mathf.Cos(lifetime*13);
                    direction=Quaternion.AngleAxis(curveSign*turn*Time.deltaTime,Vector3.up)*direction;
                }
            }
            float multiplier=SpeedMultiplier;
            bool slowed=multiplier<1;
            if(slowed!=wasSlowed)
            {
                wasSlowed=slowed; Color color=slowed?DungeonRelics.ColorFor(8):tint;
                skin.sharedMaterial=DungeonGame.Instance.Emissive(color,3); trail.sharedMaterial=DungeonGame.Instance.Emissive(color,2);
            }
            float step = Mathf.Min(speed * multiplier * Time.deltaTime, remaining);
            if(returning) step=Mathf.Min(step,Vector3.Distance(transform.position,DungeonGame.Instance.Player.View.transform.position)-.2f);
            if (step <= 0) return;
            if(weapon==DungeonWeapon.Égide && !shieldSpent)
            {
                foreach(var other in Active)
                {
                    if(other==null || other.Friendly) continue;
                    var offset=other.transform.position-transform.position;
                    float along=Mathf.Clamp(Vector3.Dot(offset,direction),0,step);
                    if((offset-direction*along).sqrMagnitude<.8f && DungeonRelics.ClearPath(transform.position,other.transform.position) && other.Absorb())
                    { shieldSpent=true; DungeonFeedback.Burst(other.transform.position,Vector3.up,false); break; }
                }
            }
            var hits = Physics.SphereCastAll(transform.position, friendly ? contactRadius : .16f, direction, step, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var enemy = hit.collider.GetComponentInParent<DungeonEnemy>();
                var player = hit.collider.GetComponentInParent<DungeonPlayer>();
                if (friendly && player != null || !friendly && enemy != null) continue;
                if (friendly && enemy != null)
                {
                    if (!struck.Add(enemy)) continue;
                    float impactDamage=damage*(falloff?Mathf.Lerp(1,.35f,Mathf.Clamp01((traveled+hit.distance)/10)):1);
                    if(enemy.Health<=0) continue;
                    float distance=traveled+hit.distance;
                    if(weapon==DungeonWeapon.Fuseau) impactDamage*=1+Mathf.Clamp01(distance/12);
                    if(Forged(WeaponTrait.Brutal) && distance<4) impactDamage*=1.4f;
                    if(Forged(WeaponTrait.Lointain) && distance>8) impactDamage*=1.5f;
                    if(Forged(WeaponTrait.Impitoyable) && enemy.Health<enemy.MaxHealth*.5f) impactDamage*=1.45f;
                    if(Forged(WeaponTrait.Chasseur) && enemy.SurfaceEnemy) impactDamage*=1.35f;
                    if(Forged(WeaponTrait.Mordant) && enemy.IsBurning) impactDamage*=1.4f;
                    if(weapon==DungeonWeapon.Pilonnier)
                    {
                        foreach(var neighbour in enemy.Room.Enemies.ToArray())
                            if(neighbour!=enemy && Vector3.Distance(neighbour.AimPoint,enemy.AimPoint)<=3 && DungeonRelics.ClearPath(enemy.AimPoint,neighbour.AimPoint))
                                neighbour.Damage(impactDamage*.65f,burning,neighbour.AimPoint-enemy.AimPoint,false);
                    }
                    Plant(hit.point,enemy.transform,impactDamage);
                    if(weapon==DungeonWeapon.Diapason) DungeonArmament.Arc(enemy,enemy.AimPoint,impactDamage,burning);
                    enemy.Damage(weapon==DungeonWeapon.Semeur?impactDamage*.4f:impactDamage,burning,direction,!secondary); DungeonGame.Instance.Hud.HitConfirm();
                    if(enemy.Health<=0)
                    {
                        var owner=DungeonGame.Instance.Player;
                        if(weapon==DungeonWeapon.Calice) owner.Heal(5);
                        if(Forged(WeaponTrait.Siphonnant)) owner.Heal(2);
                        if(Forged(WeaponTrait.Relanceur)) owner.DashCooldown=Mathf.Max(0,owner.DashCooldown-.4f);
                    }
                    DungeonFeedback.Burst(hit.point,-direction,true); DungeonGame.Instance.ImpactSound(false);
                    if (--pierce > 0) continue;
                }
                else if (!friendly && player != null)
                {
                    if(player.Reflect())
                    {
                        absorbed=true; Destroy(gameObject); return;
                    }
                    player.Damage(damage, transform.position);
                }
                else if(friendly)
                {
                    DungeonFeedback.Burst(hit.point,hit.normal,false);
                    if(!returning && !bounced && (traits&WeaponTrait.Réverbérant)!=0 && remaining>hit.distance+.1f)
                    {
                        bounced=true; remaining-=hit.distance; traveled+=hit.distance;
                        direction=Vector3.Reflect(direction,hit.normal); transform.position=hit.point+hit.normal*.15f; return;
                    }
                    Plant(hit.point+hit.normal*.15f,null,damage);
                    if(boomerang && !returning) { transform.position=hit.point+hit.normal*.2f; BeginReturn(); return; }
                }
                Destroy(gameObject); return;
            }
            transform.position += direction * step; remaining -= step; traveled+=step;
            if (remaining <= 0) { Plant(transform.position,null,damage); if(boomerang && !returning) BeginReturn(); else Destroy(gameObject); }
        }
        void BeginReturn() { DungeonRelics.Notify(7,"Retour • alignez les ennemis",transform.position); returning=true; speed=22; remaining=40; struck.Clear(); }
        public bool Absorb() { if(absorbed) return false; absorbed=true; Destroy(gameObject); return true; }
        void OnDestroy() { Active.Remove(this); }
    }

    public sealed class DungeonDanger : MonoBehaviour
    {
        float timer, radius=2, delay=1, damage=25;
        public static Vector3 GroundBelow(Vector3 point)
        {
            var hits=Physics.RaycastAll(point+Vector3.up*.2f,Vector3.down,15,~0,QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
                if(hit.collider.GetComponentInParent<DungeonEnemy>()==null && hit.collider.GetComponentInParent<DungeonPlayer>()==null) return hit.point;
            return point;
        }
        public static void Spawn(Vector3 p,float radius=2,float delay=1,float damage=25)
        {
            var go = DungeonGame.Instance.Cube("Sépulture annoncée", p + Vector3.up * .04f, new Vector3(4, .04f, 4), new Color(.75f, .12f, .06f), null, PrimitiveType.Cylinder);
            go.GetComponent<Collider>().enabled=false; Destroy(go.GetComponent<Collider>());
            var danger=go.AddComponent<DungeonDanger>(); danger.radius=radius; danger.delay=delay; danger.damage=damage;
            go.transform.localScale=new Vector3(radius*2,.04f,radius*2);
            go.GetComponent<Renderer>().sharedMaterial=DungeonGame.Instance.Emissive(new Color(1,.2f,.06f),2);
        }
        void Update()
        {
            timer += Time.deltaTime;
            transform.localScale = new Vector3(radius*2, .04f + timer/delay * .16f, radius*2);
            if (timer < delay) return;
            var player = DungeonGame.Instance.Player;
            Vector3 delta=player.transform.position-transform.position;
            if(delta.y>-.25f && delta.y<1.1f && Vector3.ProjectOnPlane(delta,Vector3.up).magnitude<radius+.35f && DungeonRelics.ClearPath(transform.position+Vector3.up*.2f,player.transform.position+Vector3.up*.8f)) player.Damage(damage,transform.position);
            Destroy(gameObject);
        }
    }
}
