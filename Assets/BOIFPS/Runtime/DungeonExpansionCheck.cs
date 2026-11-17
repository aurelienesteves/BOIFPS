#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BOIFPS
{
    // Start alone on a new run. Uses the real weapon timers, collisions and movement.
    public sealed class DungeonExpansionCheck : MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        bool failed;
        void Check(bool value,string message)
        {
            results.Add((value?"PASS ":"FAIL ")+message);
            if(!value) { failed=true; Debug.LogError("BOIFPS EXPANSION: "+message); }
        }
        void Update() { if(DungeonGame.Instance!=null && !DungeonGame.Instance.Finished) DungeonGame.Instance.Player.Heal(100); }
        IEnumerator Start()
        {
            var g=DungeonGame.Instance; var player=g.Player;
            g.Hud.ShowPause(false); g.SetPaused(false);
            player.Reload(); Check(!player.Reloading && player.Ammo==6,"R sur un chargeur plein ne lance pas de recharge");
            for(int i=0;i<6;i++) { player.Shoot(); if(i<5) yield return new WaitForSeconds(.18f); }
            Check(player.Ammo==0 && player.Reloading,"Six tirs vident le chargeur et lancent la recharge automatique");
            float remaining=player.ReloadRemaining; player.Shoot(); player.Reload();
            Check(player.Ammo==0 && Mathf.Abs(player.ReloadRemaining-remaining)<.01f,"Tir et nouvelle demande de recharge bloqués pendant la recharge");
            g.SetPaused(true); float frozen=player.ReloadRemaining;
            yield return new WaitForSecondsRealtime(.25f);
            Check(Mathf.Abs(player.ReloadRemaining-frozen)<.01f && player.Ammo==0,"La pause fige la recharge");
            g.SetPaused(false); yield return new WaitForSeconds(1.25f);
            Check(player.Ammo==6 && !player.Reloading,"La recharge rend exactement six cartouches");
            player.Shoot(); yield return new WaitForSeconds(.18f); player.Reload();
            Check(player.Ammo==5 && player.Reloading,"R permet de recharger un chargeur entamé");
            player.Shoot(); Check(player.Ammo==5,"La recharge manuelle bloque aussi le tir");
            yield return new WaitForSeconds(1.25f);
            g.Upgrades.Add(2); player.Shoot();
            Check(player.Ammo==5 && FindObjectsByType<DungeonProjectile>().Length==5,"Os bifide produit cinq pointes pour une seule cartouche");
            g.Upgrades.Clear(); g.CleanupHazards();
            foreach(var destination in new[]{new Vector3(-7,2.6f,70),new Vector3(-7,2.4f,224),new Vector3(7,2.4f,224)})
            {
                var path=new NavMeshPath();
                bool valid=NavMesh.CalculatePath(Vector3.zero,destination,NavMesh.AllAreas,path);
                Check(valid && path.status==NavMeshPathStatus.PathComplete,"Navigation vers une galerie : "+destination);
                Check(System.Array.Exists(path.corners,p=>p.y>2),"Le chemin monte réellement à l’étage");
            }
            player.Teleport(new Vector3(-7,.1f,58.5f));
            float until=Time.time+1.4f;
            while(Time.time<until)
            {
                player.GetComponent<CharacterController>().Move(Vector3.forward*8*Time.deltaTime);
                yield return null;
            }
            Check(player.transform.position.y>2.4f,"Le contrôleur joueur gravit la rampe du chœur");
            player.Teleport(new Vector3(0,.1f,-5));
            var flyer=g.Rooms[0].Spawn(4,-4,4,3.4f);
            var crawler=g.Rooms[0].Spawn(5,-10,6,3.4f);
            Vector3 flyingStart=flyer.transform.position,wallStart=crawler.transform.position;
            yield return new WaitForSeconds(3.5f);
            Check(Vector3.Distance(flyer.transform.position,flyingStart)>1 && flyer.transform.position.y>.7f,"Le Séraphin se déplace en vol");
            var mover=crawler.GetComponent<DungeonSurfaceEnemy>();
            Check(mover.Attached && Vector3.Distance(crawler.transform.position,wallStart)>1,"Le Tisse-mur se déplace sur sa paroi");
            Check(Vector3.Dot(crawler.transform.up,mover.WallNormal)>.8f,"Le Tisse-mur est orienté selon la normale du mur");
            var contacts=Physics.RaycastAll(crawler.transform.position,-mover.WallNormal,1.1f);
            Check(System.Array.Exists(contacts,h=>h.collider.GetComponentInParent<DungeonEnemy>()==null && Mathf.Abs(h.normal.y)<.4f),"Une vraie surface soutient le Tisse-mur");
            flyer.GetComponent<DungeonSurfaceEnemy>().StopAllCoroutines(); flyer.GetComponent<DungeonSurfaceEnemy>().enabled=false;
            float before=flyer.Health;
            DungeonProjectile.Spawn(flyer.transform.position+Vector3.back*2,Vector3.forward,65,18,true);
            yield return new WaitForSeconds(.15f);
            Check(flyer.Health<before,"Les projectiles touchent l’ennemi volant");
            flyer.Damage(10000,false); crawler.Damage(10000,false);
            yield return null;
            Check(g.Rooms[0].Enemies.Count==0,"Les deux nouveaux ennemis quittent la liste de combat à leur mort");
            var underBridge=g.Rooms[7].Spawn(4,-7,0,1.3f);
            yield return new WaitForSeconds(5);
            Check(underBridge.transform.position.y>2.1f,"Un Séraphin sous une tribune peut glisser vers une sortie et remonter");
            underBridge.Damage(10000,false);
            System.IO.Directory.CreateDirectory("Artifacts"); System.IO.File.WriteAllLines("Artifacts/expansion-check.txt",results);
            Debug.Log("BOIFPS EXPANSION "+(failed?"FAILED":"PASSED")+" — "+results.Count+" vérifications.");
            g.SetPaused(true); Destroy(gameObject);
        }
    }
}
#endif
