#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BOIFPS
{
    public sealed class DungeonDepthCheck : MonoBehaviour
    {
        readonly List<string> results=new List<string>();
        bool failed;
        void Check(bool value,string label) { results.Add((value?"PASS ":"FAIL ")+label); failed|=!value; }
        IEnumerator Start()
        {
            if(UnityEngine.InputSystem.Keyboard.current==null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            if(UnityEngine.InputSystem.Mouse.current==null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();
            yield return null;
            var g=DungeonGame.Instance; var player=g.Player;
            g.Hud.ShowPause(false); g.SetPaused(false);
            foreach(var room in g.Rooms) room.Entered=true; // Isolate checks from encounters.
            int[] indices={1,2,3,6,7,9,10};
            Vector3[] offsets={new Vector3(0,3,7),new Vector3(0,2.6f,6),new Vector3(0,2.6f,0),new Vector3(0,3.2f,8),new Vector3(0,2.4f,2),new Vector3(9.5f,2.8f,2),new Vector3(0,2.8f,8)};
            for(int i=0;i<indices.Length;i++)
            {
                Vector3 destination=g.Rooms[indices[i]].transform.position+offsets[i];
                var path=new NavMeshPath();
                bool sampled=NavMesh.SamplePosition(destination,out var hit,.45f,NavMesh.AllAreas);
                Check(sampled && Mathf.Abs(hit.position.y-destination.y)<.3f,"Walkable upper level in room "+indices[i]);
                Check(sampled && NavMesh.CalculatePath(Vector3.zero,hit.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"Connected ramp route in room "+indices[i]+" ("+path.status+", last="+(path.corners.Length>0?path.corners[path.corners.Length-1].ToString():"none")+")");
                player.Teleport(destination+Vector3.up*.05f);
                yield return new WaitForSeconds(.15f);
                Check(Mathf.Abs(player.transform.position.y-destination.y)<.15f,"Player stands on upper level "+indices[i]);
                var spawned=g.Rooms[indices[i]].Spawn(0,offsets[i].x,offsets[i].z);
                spawned.StopAllCoroutines();
                Check(spawned.GetComponent<NavMeshAgent>().isOnNavMesh && Mathf.Abs(spawned.transform.position.y-destination.y)<.3f,"Enemy spawns on the correct level "+indices[i]);
                spawned.Damage(10000,false,Vector3.zero,false); yield return null;
            }
            int[] rampRooms={1,3,6,10,9};
            Vector3[] starts={new Vector3(9,.1f,-3),new Vector3(0,.1f,-11),new Vector3(-9,.1f,-11),new Vector3(8,.1f,-11),new Vector3(9.5f,.1f,-11)};
            float[] lengths={8.5f,5,8,13.5f,8}; float[] heights={3,2.6f,3.2f,2.8f,2.8f};
            for(int i=0;i<rampRooms.Length;i++)
            {
                player.Teleport(g.Rooms[rampRooms[i]].transform.position+starts[i]);
                float end=Time.time+(lengths[i]+.6f)/8;
                while(Time.time<end) { player.GetComponent<CharacterController>().Move(Vector3.forward*8*Time.deltaTime); yield return null; }
                yield return new WaitForSeconds(.2f);
                Check(Physics.Raycast(player.transform.position+Vector3.up*.05f,Vector3.down,.25f) && Mathf.Abs(player.transform.position.y-heights[i])<.15f,"Player physically climbs new ramp "+rampRooms[i]+" (y="+player.transform.position.y+")");
            }
            // Damage must resolve on the targeted floor, and jumping must evade it.
            Vector3 balcony=g.Rooms[3].transform.position+new Vector3(0,2.6f,0);
            player.Teleport(balcony+Vector3.up*.03f); player.Health=100;
            DungeonDanger.Spawn(balcony,1.8f,.15f,18); yield return new WaitForSeconds(.25f);
            Check(player.Health==82,"Burial damages the player on the upper floor");
            yield return new WaitForSeconds(.55f); player.Health=100;
            DungeonDanger.Spawn(new Vector3(balcony.x,0,balcony.z),1.8f,.15f,18); yield return new WaitForSeconds(.25f);
            Check(player.Health==100,"Ground burial does not damage the balcony");
            player.Teleport(new Vector3(0,.03f,-5)); yield return new WaitForSeconds(.15f);
            DungeonDanger.Spawn(new Vector3(0,0,-5),1.8f,.2f,18); player.Jump(); yield return new WaitForSeconds(.3f);
            Check(player.Health==100 && player.transform.position.y>1.1f,"Jump evades burial");
            yield return new WaitForSeconds(.5f);
            g.Upgrades.Add(8); player.Shoot(); player.Reload();
            Check(g.Hud.LastRelicFeedback.Contains("Champ actif"),"Reload announces the slowing field");
            yield return new WaitForSeconds(1.3f);
            g.Upgrades.Add(9); g.Upgrades.Add(10); player.DashCooldown=0; player.Dash(Vector3.forward);
            Check(player.ReflectReady && g.Hud.LastRelicFeedback.Contains("Capture armée"),"Dash announces mirror readiness");
            player.Reflect();
            Check(!player.ReflectReady && g.Hud.LastRelicFeedback.Contains("capturé"),"Reflection confirmation is not throttled by readiness");
            yield return new WaitForSeconds(.25f);
            Check(FindObjectsByType<DungeonMine>().Length>0,"Dash creates mines");
            DungeonRelics.CreateOrbitals(player);
            Check(g.Hud.LastRelicFeedback.Contains("satellites"),"Orbitals announce their count and duration");
            yield return new WaitForSeconds(1.1f);
            Check(FindObjectsByType<DungeonRelicPulse>().Length==0,"Relic pulses clean themselves up");
            g.CleanupHazards(); g.Upgrades.Clear(); player.Teleport(new Vector3(0,.03f,-5));
            var shooter=g.Rooms[0].Spawn(1,0,6);
            var patterns=new HashSet<string>(); float until=Time.time+9;
            while(Time.time<until) { player.Heal(100); patterns.Add(shooter.Tactic); yield return null; }
            Check(patterns.Contains("Éventail double") && patterns.Contains("Rafale de précision") && patterns.Contains("Salve en étages"),"Shooter cycles three attack patterns");
            shooter.Damage(10000,false,Vector3.zero,false); g.CleanupHazards();
            System.IO.Directory.CreateDirectory("Artifacts");
            System.IO.File.WriteAllLines("Artifacts/depth-check.txt",results);
            Debug.Log("BOIFPS DEPTH "+(failed?"FAILED":"PASSED"));
            if(Application.isBatchMode) UnityEditor.EditorApplication.Exit(failed?1:0);
            Destroy(gameObject);
        }
    }
}
#endif
