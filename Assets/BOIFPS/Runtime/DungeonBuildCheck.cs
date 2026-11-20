#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
namespace BOIFPS
{
    public sealed class DungeonBuildCheck : MonoBehaviour
    {
        readonly List<string> results=new List<string>(); bool failed;
        DungeonGame g; DungeonPlayer p;
        void Check(bool value,string text) { results.Add((value?"PASS ":"FAIL ")+text); failed|=!value; }
        DungeonEnemy Dummy(int kind,float x,float z)
        {
            var e=g.Rooms[0].Spawn(kind,x,z); e.StopAllCoroutines(); e.GetComponent<NavMeshAgent>().isStopped=true; return e;
        }
        IEnumerator Reset()
        {
            g.Upgrades.Clear();
            foreach(var room in g.Rooms) foreach(var e in room.Enemies.ToArray()) e.Damage(10000,false,Vector3.zero,false);
            g.CleanupHazards(); p.enabled=true; p.Health=100; p.Teleport(new Vector3(0,.1f,-5));
            p.Reload(); yield return new WaitForSeconds(1.3f);
        }
        IEnumerator Start()
        {
            if(Keyboard.current==null) InputSystem.AddDevice<Keyboard>();
            if(Mouse.current==null) InputSystem.AddDevice<Mouse>();
            yield return null; g=DungeonGame.Instance; p=g.Player;
            foreach(var room in g.Rooms) room.Entered=true;
            g.Hud.ShowPause(false); g.SetPaused(false);
            for(int i=0;i<30;i++)
            {
                var choices=g.Hud.RewardChoices();
                Check(choices.Count==3 && new HashSet<int>(choices).Count==3 && choices.TrueForAll(DungeonRelics.IsTransformation),"First reliquary: three distinct transformations "+i);
            }
            g.Upgrades.Add(2);
            var later=g.Hud.RewardChoices();
            Check(!later.Contains(2) && later.Exists(DungeonRelics.IsTransformation),"Later choices exclude owned relics and offer a new transformation");
            var near=Dummy(2,0,-3); float before=near.Health;
            p.Shoot();
            Check(DungeonProjectile.Active.Count==5 && p.Ammo==5,"Scatter fires five pellets for one cartridge");
            float spread=0; foreach(var b in DungeonProjectile.Active) spread=Mathf.Max(spread,Vector3.Angle(Vector3.forward,b.Direction));
            Check(spread>=13 && p.ShotInterval>.5f,"Scatter has a wide cone and slower cadence");
            p.Shoot(); yield return new WaitForSeconds(.2f); p.Shoot();
            Check(p.Ammo==5,"Scatter cannot bypass its slower cadence");
            float closeDamage=before-near.Health;
            yield return Reset(); g.Upgrades.Add(2); var far=Dummy(2,0,3); before=far.Health; p.Shoot();
            yield return new WaitForSeconds(.25f); float farDamage=before-far.Health;
            Check(closeDamage>35 && closeDamage>farDamage*1.7f,"Scatter rewards close range (near="+closeDamage+", far="+farDamage+")");
            yield return new WaitForSeconds(.25f);
            Check(DungeonProjectile.Active.Count==0,"Scatter pellets expire at ten meters");
            yield return Reset(); g.Upgrades.Add(7); var target=Dummy(2,0,-2); before=target.Health; p.Shoot();
            Check(p.Ammo==5 && System.Array.TrueForAll(FindObjectsByType<DungeonProjectile>(),b=>b.Boomerang),"Pendulum applies from the first cartridge");
            yield return new WaitForSeconds(.2f);
            Check(Mathf.Abs(before-target.Health-18)<.1f,"Pendulum hits on the outward flight");
            yield return new WaitForSeconds(.7f);
            Check(Mathf.Abs(before-target.Health-36)<.1f,"Pendulum hits the same target again on return");
            yield return Reset(); g.Upgrades.Add(7); p.Shoot(); yield return new WaitForSeconds(.4f);
            p.Teleport(new Vector3(4,.1f,-5)); yield return new WaitForSeconds(.05f);
            bool bends=false; foreach(var b in DungeonProjectile.Active) bends|=b.Returning && b.Direction.x>.1f;
            Check(bends,"Moving changes the return trajectory");
            yield return Reset(); g.Upgrades.Add(3);
            var seed=Dummy(0,0,3); var relay=Dummy(0,2,3); var end=Dummy(1,5,3); relay.Damage(relay.MaxHealth-10,false);
            seed.Damage(100,true); yield return new WaitForSeconds(.2f);
            Check(relay.Health<=0 && end.IsBurning && Mathf.Abs(end.Health-22)<.1f,"Burning deaths chain through another burning victim");
            float burnHealth=end.Health; yield return new WaitForSeconds(1.05f);
            Check(end.Health<=burnHealth-6,"Propagated fire deals damage over time");
            yield return Reset(); g.Upgrades.Add(3); seed=Dummy(0,0,3); target=Dummy(0,3,3);
            var wall=g.Cube("Test fire obstruction",new Vector3(1.5f,1.5f,3),new Vector3(.3f,3,4),Color.gray);
            Physics.SyncTransforms(); seed.Damage(100,true); yield return new WaitForSeconds(.2f);
            Check(target.Health==target.MaxHealth && !target.IsBurning,"Fire propagation respects walls"); Destroy(wall);
            yield return Reset(); g.Upgrades.Add(10); p.DashCooldown=0; p.Dash(Vector3.right); p.enabled=false;
            var incoming=DungeonProjectile.Spawn(p.transform.position+new Vector3(0,1,1.2f),Vector3.back,20,20,false);
            float health=p.Health; yield return new WaitForSeconds(.15f);
            Check(incoming==null && p.Health==health && p.EmpoweredShot && !p.ReflectReady,"Dash captures a hostile shot without damage and stores a riposte");
            Check(!p.Reflect(),"One dash cannot capture twice");
            target=Dummy(2,0,-1); var behind=Dummy(2,0,2); p.Shoot();
            Check(!p.EmpoweredShot,"Firing consumes the stored riposte once");
            yield return new WaitForSeconds(.2f);
            Check(Mathf.Abs(target.MaxHealth-target.Health-45)<.1f && Mathf.Abs(behind.MaxHealth-behind.Health-45)<.1f,"Riposte deals 45 damage and penetrates two enemies");
            yield return Reset(); foreach(int id in new[]{2,3,7,10}) g.Upgrades.Add(id);
            p.DashCooldown=0; p.Dash(Vector3.right); p.Reflect(); p.enabled=false; target=Dummy(3,0,-3); p.Shoot();
            bool combined=DungeonProjectile.Active.Count==5;
            foreach(var b in DungeonProjectile.Active) combined&=b.Boomerang && Mathf.Abs(b.DamageAmount-35)<.01f;
            Check(combined,"Scatter + pendulum + mirror produce five empowered returning pellets");
            yield return new WaitForSeconds(.12f);
            Check(target.IsBurning,"Combined pellets retain fire propagation");
            float frozen=target.Health; g.SetPaused(true); yield return new WaitForSecondsRealtime(.2f);
            Check(target.Health==frozen,"Pause freezes the transformed build"); g.SetPaused(false);
            yield return Reset();
            Check(DungeonProjectile.Active.Count==0,"Cleanup removes transformed projectiles");
            g.Upgrades.Add(13); target=Dummy(2,3,-5);
            p.Shoot(); before=target.Health; p.Reload();
            Check(before-target.Health==25,"Five remaining cartridges deal 25 reload damage");
            yield return Reset(); g.Upgrades.Add(12);
            p.Jump(); yield return new WaitForSeconds(.25f); float altitude=p.transform.position.y;
            p.Jump(); yield return new WaitForSeconds(.3f);
            Check(p.transform.position.y>altitude+1,"Second jump gains height in midair");
            yield return Reset(); p.Equip(DungeonWeapon.Diapason);
            var primary=Dummy(2,0,-1); var adjacent=Dummy(2,3,-1);
            p.Shoot(); yield return new WaitForSeconds(.2f);
            Check(primary.Health<primary.MaxHealth && adjacent.Health<adjacent.MaxHealth,"Diapason chains to a nearby second target");
            yield return Reset(); p.Equip(DungeonWeapon.Semeur); g.Upgrades.Add(3);
            primary=Dummy(2,0,-1); adjacent=Dummy(2,2,-1);
            p.Shoot(); yield return new WaitForSeconds(.35f);
            Check(primary.IsBurning && adjacent.Health==adjacent.MaxHealth,"Seed sticks before exploding");
            yield return new WaitForSeconds(.8f);
            Check(adjacent.IsBurning && adjacent.Health<adjacent.MaxHealth,"Seed blast carries the fire relic to its neighbours");
            float interval=p.ShotInterval; p.AddTrait(WeaponTrait.Vif);
            Check(p.ShotInterval<interval,"Vif accelerates the equipped weapon");
            yield return Reset(); p.Equip(DungeonWeapon.Cloueur); p.AddTrait(WeaponTrait.Réverbérant);
            var panel=g.Cube("Paroi de contrôle",new Vector3(0,2,-1),new Vector3(4,4,.3f),Color.gray);
            Physics.SyncTransforms(); p.Shoot(); yield return new WaitForSeconds(.1f);
            bool reflected=false; foreach(var shot in DungeonProjectile.Active) reflected|=shot.Direction.z<0;
            Check(reflected,"Réverbérant reflects a nail off a wall"); Destroy(panel);
            yield return Reset(); p.AddTrait(WeaponTrait.Aimanté); g.Current=g.Rooms[0];
            adjacent=Dummy(2,2,1); p.Shoot(); yield return new WaitForSeconds(.05f);
            bool guided=false; foreach(var shot in DungeonProjectile.Active) guided|=shot.Direction.x>.01f;
            Check(guided,"Aimanté bends the shot toward an off-axis enemy");
            Check(DungeonArmament.Names.Length==13 && DungeonArmament.Profiles.Length==13 && DungeonArmament.Descriptions.Length==13,"Catalogue of thirteen complete weapons");
            Check(new HashSet<WeaponTrait>(DungeonArmament.Traits).Count==23 && DungeonArmament.TraitDescriptions.Length==23,"Twenty-three distinct forge choices");
            yield return Reset();
            foreach(var modifier in DungeonArmament.Traits) p.AddTrait(modifier);
            foreach(DungeonWeapon kind in System.Enum.GetValues(typeof(DungeonWeapon)))
            {
                yield return Reset(); p.Equip(kind); g.Upgrades.Add(2); g.Upgrades.Add(7);
                p.Shoot();
                Check(p.Ammo==5 && DungeonProjectile.Active.Count>=6 && System.Array.TrueForAll(FindObjectsByType<DungeonProjectile>(),b=>b.Boomerang),"Forged "+kind+" combines with scatter and pendulum");
            }
            System.IO.Directory.CreateDirectory("Artifacts"); System.IO.File.WriteAllLines("Artifacts/build-check.txt",results);
            Debug.Log("BOIFPS BUILDS "+(failed?"FAILED":"PASSED")+" — "+results.Count+" checks");
            if(Application.isBatchMode) UnityEditor.EditorApplication.Exit(failed?1:0);
            Destroy(gameObject);
        }
    }
}
#endif
