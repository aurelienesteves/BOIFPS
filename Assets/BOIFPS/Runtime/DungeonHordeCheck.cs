#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BOIFPS
{
    // Run alone on a fresh welcome screen. Each group exercises observable gameplay.
    public sealed class DungeonHordeCheck : MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        bool failed;
        void Check(bool value,string text) { results.Add((value?"PASS ":"FAIL ")+text); if(!value) { failed=true; Debug.LogError("BOIFPS HORDE: "+text); } }
        void ClearEnemies()
        {
            foreach(var enemy in DungeonRelics.Targets()) enemy.Damage(10000,false,Vector3.zero,false);
            DungeonGame.Instance.CleanupHazards();
        }
        DungeonEnemy Dummy(int kind,float x,float z)
        {
            var enemy=DungeonGame.Instance.Rooms[0].Spawn(kind,x,z); enemy.StopAllCoroutines();
            var agent=enemy.GetComponent<UnityEngine.AI.NavMeshAgent>(); if(agent!=null) agent.isStopped=true;
            return enemy;
        }
        IEnumerator Start()
        {
            var g=DungeonGame.Instance; var p=g.Player; g.Hud.ShowPause(false); g.SetPaused(false);
            foreach(int index in new[]{6,7,10})
            {
                var arena=g.Rooms.Find(room=>room.Index==index); if(arena==null) continue;
                Check(arena.Pits.Count==2,"Deux fosses physiques dans la salle "+index);
                var pit=arena.Pits[0]; Vector3 center=arena.transform.position+new Vector3(pit.center.x,1,pit.center.y);
                Check(!Physics.Raycast(center,Vector3.down,3,~0,QueryTriggerInteraction.Ignore),"Le centre de la fosse est dépourvu de sol : "+index);
            }
            var pitRoom=g.Rooms.Find(room=>room.Index==12); pitRoom.Entered=true;
            p.SetCheckpoint(pitRoom.transform.position+Vector3.back*8);
            p.Teleport(pitRoom.transform.position+new Vector3(0,.1f,3));
            yield return new WaitForSeconds(.7f);
            Check(p.Health==80 && p.transform.position.y>-.1f,"Une chute retire 20 PV et replace le joueur sur un point sûr");
            p.Heal(100); p.Teleport(new Vector3(0,.1f,-5));
            var direct=g.Rooms[0].Spawn(0,-2,7); var right=g.Rooms[0].Spawn(0,0,7); var left=g.Rooms[0].Spawn(0,2,7);
            yield return new WaitForSeconds(1.1f);
            Check(right.transform.position.x>2 && left.transform.position.x<-2,"Deux Rampants contournent réellement par des côtés opposés");
            Check(direct.transform.position.z<right.transform.position.z,"Le poursuivant direct atteint le joueur avant les contourneurs");
            ClearEnemies(); yield return null;
            // Stop the remote encounter started by the fall test.
            pitRoom.StopAllCoroutines();
            g.Upgrades.Add(6); var victim=Dummy(0,0,4); var target=Dummy(2,0,7); float before=target.Health;
            victim.Damage(10000,false);
            Check(FindObjectsByType<DungeonProjectile>().Length==3,"Chœur des morts libère trois esprits");
            yield return new WaitForSeconds(.4f);
            Check(target.Health<before,"Les esprits poursuivent et blessent une autre cible");
            ClearEnemies(); yield return null;
            var secondaryVictim=Dummy(0,3,4); secondaryVictim.Damage(10000,false,Vector3.zero,false);
            Check(FindObjectsByType<DungeonProjectile>().Length==0,"Les dégâts secondaires ne déclenchent pas de chaîne infinie");
            g.Upgrades.Clear(); g.Upgrades.Add(7); target=Dummy(2,0,2);
            p.Reload(); yield return new WaitForSeconds(1.25f);
            for(int i=0;i<6;i++) { p.Shoot(); if(i<5) yield return new WaitForSeconds(.18f); }
            yield return new WaitForSeconds(.25f);
            Check(System.Array.Exists(FindObjectsByType<DungeonProjectile>(),b=>b.Returning),"Les pointes du Pendule repartent vers le joueur");
            yield return new WaitForSeconds(.45f);
            Check(target.Health<36,"Le retour du Pendule inflige un deuxième impact");
            ClearEnemies(); g.Upgrades.Clear(); yield return new WaitForSeconds(.6f);
            g.Upgrades.Add(8); p.Shoot(); p.Reload();
            var slow=DungeonProjectile.Spawn(p.transform.position+new Vector3(0,1,3),Vector3.right,10,1,false);
            Check(slow.SpeedMultiplier==.25f,"Sablier ralentit les projectiles proches pendant la recharge");
            g.CleanupHazards(); g.Upgrades.Clear(); yield return new WaitForSeconds(1.25f);
            g.Upgrades.Add(9); target=Dummy(2,0,-1); before=target.Health; p.DashCooldown=0; p.Dash(Vector3.forward);
            yield return new WaitForSeconds(.16f);
            Check(FindObjectsByType<DungeonMine>().Length==3,"Le dash sème trois mines");
            yield return new WaitForSeconds(.7f);
            Check(target.Health<before && FindObjectsByType<DungeonMine>().Length==0,"Les mines explosent, blessent et se nettoient");
            ClearEnemies(); g.Upgrades.Clear(); p.Teleport(new Vector3(0,.1f,-5));
            g.Upgrades.Add(10); p.DashCooldown=0; p.Dash(Vector3.forward); target=Dummy(2,0,4);
            var reflected=DungeonProjectile.Spawn(p.transform.position+new Vector3(0,1,1.2f),Vector3.back,20,20,false);
            float health=p.Health; yield return new WaitForSeconds(.1f);
            Check(reflected==null && p.EmpoweredShot && p.Health==health,"Le miroir capture un projectile et stocke une riposte sans dégâts");
            Check(!p.ReflectReady,"Le miroir ne peut servir qu’une fois par dash");
            ClearEnemies(); g.Upgrades.Clear(); p.Teleport(new Vector3(0,.1f,-5)); yield return new WaitForSeconds(.3f);
            g.Upgrades.Add(11); p.Reload(); yield return new WaitForSeconds(1.25f);
            for(int i=0;i<6;i++) { p.Shoot(); if(i<5) yield return new WaitForSeconds(.18f); }
            yield return new WaitForSeconds(1.3f);
            Check(FindObjectsByType<DungeonOrbital>().Length==2,"Une recharge à vide crée deux satellites");
            DungeonRelics.CreateOrbitals(p); DungeonRelics.CreateOrbitals(p);
            Check(FindObjectsByType<DungeonOrbital>().Length==4,"Le nombre de satellites est plafonné à quatre");
            foreach(var orbital in FindObjectsByType<DungeonOrbital>()) Destroy(orbital.gameObject);
            g.Upgrades.Clear(); g.Hud.ShowRewards();
            var buttons=g.Hud.GetComponentsInChildren<UnityEngine.UI.Button>();
            bool novel=false; foreach(var button in buttons) foreach(var label in button.GetComponentsInChildren<TMPro.TMP_Text>()) for(int i=6;i<DungeonGame.ItemNames.Length;i++) if(label.text.Contains(DungeonGame.ItemNames[i])) novel=true;
            Check(buttons.Length==3 && novel,"Les reliquaires proposent une nouvelle relique parmi trois choix");
            System.IO.Directory.CreateDirectory("Artifacts"); System.IO.File.WriteAllLines("Artifacts/horde-check.txt",results);
            Debug.Log("BOIFPS HORDE "+(failed?"FAILED":"PASSED")+" — "+results.Count+" vérifications.");
            Destroy(gameObject);
        }
    }
}
#endif
