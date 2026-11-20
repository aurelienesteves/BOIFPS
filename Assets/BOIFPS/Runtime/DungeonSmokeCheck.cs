#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BOIFPS
{
    // Attached only by the editor verification command, never by the game scene.
    public sealed class DungeonSmokeCheck : MonoBehaviour
    {
        readonly List<string> results = new List<string>();
        bool failed;
        void Check(bool condition, string label)
        {
            results.Add((condition ? "PASS " : "FAIL ") + label);
            if (!condition) { failed = true; Debug.LogError("BOIFPS CHECK: " + label); }
        }
        void Update() { if (DungeonGame.Instance != null && !DungeonGame.Instance.Finished) DungeonGame.Instance.Player.Heal(100); }
        static void PickFromLastPage(DungeonHud hud)
        {
            for(int page=0;page<8;page++)
            {
                var buttons=hud.GetComponentsInChildren<UnityEngine.UI.Button>();
                var next=System.Array.Find(buttons,button=>button.name=="Page suivante");
                if(next==null) { buttons[0].onClick.Invoke(); return; }
                next.onClick.Invoke();
            }
        }
        IEnumerator Start()
        {
            var g = DungeonGame.Instance;
            Check(g.Rooms.Count >= 11 && g.Rooms.Count <= 15 && g.Paused && g.Player.Health == 100, "Initialisation et pause d’accueil");
            for(int i=1;i<g.Rooms.Count;i++)
            {
                var tile=g.Rooms[i];
                Vector3 towardParent=(g.Rooms[g.Parents[i]].transform.position-tile.transform.position).normalized;
                Vector3 destination=tile.transform.position+towardParent*10.8f;
                var path = new NavMeshPath();
                Check(NavMesh.CalculatePath(Vector3.zero, destination, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete, "Navigation complète vers " + destination);
            }
            g.Hud.ShowPause(false); g.SetPaused(false); g.Player.enabled = false;
            var encounters=g.Rooms.FindAll(tile=>tile.IsCombat && tile.Index!=9);
            encounters.Add(g.Rooms.Find(tile=>tile.Index==9));
            foreach(var room in encounters)
            {
                int index=room.Index;
                g.Player.Teleport(DungeonDanger.GroundBelow(room.transform.position+Vector3.back*7+Vector3.up*6)+Vector3.up*.1f);
                yield return new WaitForSeconds(1.4f);
                bool opening=g.Cleared<3 && index!=9;
                int expected = opening?2+g.Cleared:index==1?5:index==2?6:index==3?5:index==6?8:index==7?8:index==10?8:index>=12?6:1;
                if(opening) Check(room.Enemies.TrueForAll(e=>e.Kind==0?e.Health==18:e.Kind==1 && e.Health==36),"Ouverture avec ennemis à un ou deux tirs");
                Check(room.Active && room.Enemies.Count == expected, "Composition salle " + index);
                Check(room.Gates.TrueForAll(gate => gate.activeSelf), "Portes verrouillées salle " + index);
                foreach (var e in room.Enemies)
                {
                    var agent=e.GetComponent<NavMeshAgent>();
                    if(agent!=null)
                    {
                        Check(agent.isOnNavMesh, "Agent sur NavMesh " + e.name);
                        var route=new NavMeshPath();
                        Check(agent.CalculatePath(g.Player.transform.position,route) && route.status==NavMeshPathStatus.PathComplete,"Ennemi peut rejoindre l’entrée de la salle "+index);
                    }
                    else
                    {
                        var mover=e.GetComponent<DungeonSurfaceEnemy>();
                        Check(mover!=null && e.transform.position.y>1.5f,"Ennemi hors sol actif : "+e.name);
                        if(e.Kind==5) Check(mover.Attached,"Tisse-mur attaché à une paroi");
                    }
                }
                if (index == 1 && room.Enemies.Count > 0)
                {
                    var enemy = room.Enemies[0]; enemy.GetComponent<NavMeshAgent>().Warp(room.transform.position+Vector3.forward*4); enemy.GetComponent<NavMeshAgent>().isStopped = true;
                    enemy.StopAllCoroutines(); float before = enemy.Health;
                    DungeonProjectile.Spawn(enemy.transform.position + Vector3.up * .55f + Vector3.left * 2, Vector3.right, 45, 10, true);
                    yield return new WaitForSeconds(.15f);
                    Check(enemy.Health < before, "Projectile joueur applique les dégâts");
                }
                if (index == 9 && room.Enemies.Count > 0)
                {
                    var boss = room.Enemies[0];
                    yield return new WaitForSeconds(2);
                    boss.Damage(boss.MaxHealth*.55f, false);
                    yield return new WaitForSeconds(11);
                    Check(boss != null && boss.PhaseTwo, "Boss actif sous 50 pour cent et cycle de phase 2 sans exception");
                }
                foreach (var e in room.Enemies.ToArray()) e.Damage(10000, false,Vector3.zero,false);
                if (!opening && index == 3)
                {
                    yield return new WaitForSeconds(1.2f);
                    Check(room.Enemies.Count == 7 && room.Active, "Deuxième vague C et maintien du verrouillage");
                    foreach (var e in room.Enemies.ToArray()) e.Damage(10000, false,Vector3.zero,false);
                }
                if(!opening && (index==6 || index==7))
                {
                    Check(room.Active && room.ReinforcementsPending,"Portes maintenues avant les renforts temporisés");
                    yield return new WaitForSeconds(4.2f);
                    Check(room.Enemies.Count==4,"Quatre renforts arrivent pendant le combat");
                    foreach(var e in room.Enemies.ToArray()) e.Damage(10000,false,Vector3.zero,false);
                }
                yield return new WaitForSecondsRealtime(.2f);
                Check(room.Cleared && room.Gates.TrueForAll(gate => !gate.activeSelf), "Fin du combat et portes ouvertes salle " + index);
                if (!g.Player.Armed)
                {
                    g.Hud.ShowRewards();
                    var buttons = g.Hud.GetComponentsInChildren<UnityEngine.UI.Button>();
                    Check(buttons.Length == 5 && g.Paused, "Quatre armes et navigation du catalogue en pause");
                    PickFromLastPage(g.Hud);
                    Check(g.Player.Armed && g.Player.Weapon==DungeonWeapon.Pilonnier && g.Hud.Choosing,"Dernière page de l’arsenal accessible");
                    PickFromLastPage(g.Hud);
                    Check(g.Player.Traits!=0 && g.Hud.Choosing,"Adjectif forgé avant la relique");
                    g.Hud.GetComponentsInChildren<UnityEngine.UI.Button>()[0].onClick.Invoke();
                    Check(g.Upgrades.Count == 1 && !g.Paused, "Acquisition d’une seule relique et reprise");
                }
            }
            Check(g.Finished && g.Paused && g.Cleared == g.CombatTotal, "Victoire et bilan final");
            yield return null;
            Check(FindObjectsByType<DungeonProjectile>().Length == 0 && FindObjectsByType<DungeonDanger>().Length == 0, "Nettoyage des dangers");
            System.IO.Directory.CreateDirectory("Artifacts");
            System.IO.File.WriteAllLines("Artifacts/smoke-check.txt", results);
            Debug.Log("BOIFPS SMOKE " + (failed ? "FAILED" : "PASSED") + " — " + results.Count + " vérifications. Artifacts/smoke-check.txt");
            Destroy(gameObject);
        }
    }
}
#endif
