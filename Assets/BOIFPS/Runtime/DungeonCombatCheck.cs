#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BOIFPS
{
    // Run alone from the welcome screen, before the full dungeon smoke check.
    public sealed class DungeonCombatCheck : MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        bool failed;
        void Check(bool value,string description)
        {
            results.Add((value?"PASS ":"FAIL ")+description);
            if(!value) { failed=true; Debug.LogError("BOIFPS COMBAT: "+description); }
        }
        IEnumerator Start()
        {
            var g=DungeonGame.Instance;
            g.Hud.ShowPause(false); g.SetPaused(false);
            g.Player.Teleport(new Vector3(0,.1f,-5));
            yield return new WaitForSeconds(.3f);
            Check(g.Player.Grounded,"Joueur au sol avant le saut");
            g.Player.Jump();
            float start=Time.time,maxY=0; bool retried=false;
            while(Time.time-start<1.05f)
            {
                maxY=Mathf.Max(maxY,g.Player.transform.position.y);
                if(!retried && Time.time-start>.18f) { retried=true; g.Player.Jump(); }
                yield return null;
            }
            Check(maxY>1.2f && maxY<2,"Saut atteint une hauteur utile sans double saut");
            Check(g.Player.Grounded && g.Player.transform.position.y<.15f,"Retour au sol après le saut");
            var enemy=g.Rooms[0].Spawn(2,0,3); enemy.StopAllCoroutines(); enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped=true;
            float before=enemy.Health;
            g.Player.Shoot();
            Check(FindObjectsByType<DungeonProjectile>().Length>0,"Le tir émet un projectile");
            var sound=g.GetComponents<AudioSource>();
            Check(System.Array.Exists(sound,s=>s.isPlaying),"Une source audio joue la détonation");
            yield return new WaitForSeconds(.2f);
            Check(Mathf.Abs(enemy.Health-(before-18))<.01f,"Le Cloueur inflige 18 dégâts directs");
            Check(enemy.transform.position.z>3.1f,"Un impact repousse légèrement la cible");
            enemy.Damage(10000,false,Vector3.forward);
            Check(FindObjectsByType<DungeonFeedback>().Length>0,"Des fragments accompagnent impacts et mort");
            yield return new WaitForSeconds(.85f);
            Check(FindObjectsByType<DungeonFeedback>().Length==0,"Les fragments se nettoient après leur durée de vie");
            // Leap onto the first arena's low sarcophagus, outside its combat trigger.
            g.Player.Teleport(new Vector3(6,.1f,25.6f));
            yield return new WaitForSeconds(.2f);
            g.Player.Jump(); yield return new WaitForSeconds(.25f);
            g.Player.GetComponent<CharacterController>().Move(Vector3.forward*3);
            yield return new WaitForSeconds(.7f);
            Check(g.Player.Grounded && g.Player.transform.position.y>1,"Le saut permet de monter sur un sarcophage bas");
            // This isolated test can trigger room A; restart before running the route check.
            System.IO.Directory.CreateDirectory("Artifacts"); System.IO.File.WriteAllLines("Artifacts/combat-check.txt",results);
            Debug.Log("BOIFPS COMBAT "+(failed?"FAILED":"PASSED")+" — "+results.Count+" vérifications.");
            g.SetPaused(true); Destroy(gameObject);
        }
    }
}
#endif
