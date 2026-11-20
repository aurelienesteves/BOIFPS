using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace BOIFPS
{
    public sealed class DungeonRoom : MonoBehaviour
    {
        public int Index;
        public float Height, GallerySpan, GalleryDepth;
        public bool Entered, Cleared, Active;
        public readonly List<GameObject> Gates = new List<GameObject>();
        public readonly List<DungeonEnemy> Enemies = new List<DungeonEnemy>();
        public readonly List<Rect> Pits=new List<Rect>();
        public bool ReinforcementsPending { get; private set; }
        public bool ContainsPit(Vector3 world,float margin=0)
        {
            Vector3 p=world-transform.position;
            foreach(var pit in Pits) if(p.x>pit.xMin-margin && p.x<pit.xMax+margin && p.z>pit.yMin-margin && p.z<pit.yMax+margin) return true;
            return false;
        }
        public bool IsCombat => Index == 1 || Index == 2 || Index == 3 || Index == 6 || Index == 7 || Index == 9 || Index == 10 || Index==12 || Index==13;
        public void Prepare()
        {
            if (Index == 0 || Index == 4 || Index == 11) Pickup(false, transform.position+(Index==0?Vector3.forward*3:Vector3.zero), this);
            if (Index == 8) Pickup(true, transform.position);
        }
        void Update()
        {
            var g = DungeonGame.Instance;
            if (g.Player == null || g.Paused) return;
            Vector3 d = g.Player.transform.position - transform.position;
            if (Mathf.Abs(d.x) < 9 && Mathf.Abs(d.z) < 9)
            {
                g.Current = this;
                if (!Entered)
                {
                    Entered = true;
                    if (IsCombat) StartCoroutine(Encounter());
                }
            }
        }
        IEnumerator Encounter()
        {
            Active = true;
            DungeonGame.Instance.Player.SetCheckpoint(DungeonGame.Instance.Player.transform.position);
            foreach (var gate in Gates) gate.SetActive(true);
            DungeonGame.Instance.Sound(.7f);
            yield return new WaitForSeconds(.6f);
            int cleared=DungeonGame.Instance.Cleared;
            if(Index!=9 && cleared<3)
            {
                SpawnOpening(cleared);
                yield return new WaitUntil(() => Enemies.Count==0);
                Complete();
                yield break;
            }
            switch (Index)
            {
                case 12: case 13:
                    Spawn(1,-GallerySpan,Index==12?3:-3,Height); Spawn(1,GallerySpan,3,Height);
                    Spawn(4,0,0,Height+1); Spawn(0,-2,-5); Spawn(0,2,7); Spawn(2,0,7); break;
                case 1:
                    Spawn(0,-3,3); Spawn(0,3,3); Spawn(0,0,8); Spawn(0,-2,0); Spawn(0,2,0); break;
                case 2:
                    Spawn(1,-7,6,2.6f); Spawn(1,7,6,2.6f);
                    Spawn(0,-3,3); Spawn(0,3,3); Spawn(4,-4,2,3.8f); Spawn(4,4,2,3.8f); break;
                case 3:
                    Spawn(2,0,6); Spawn(2,0,0); Spawn(1,-3,7); Spawn(5,-10,-6); Spawn(5,10,-6);
                    yield return new WaitUntil(() => Enemies.Count == 0);
                    Warn(new[]{new Vector3(-3,0,2),new Vector3(3,0,2),new Vector3(-3,0,6),new Vector3(3,0,6),new Vector3(0,0,6),new Vector3(0,3.2f,-2),new Vector3(0,3.2f,3)});
                    DungeonGame.Instance.Sound(.45f);
                    yield return new WaitForSeconds(.65f);
                    Spawn(0,-3,2); Spawn(0,3,2); Spawn(0,-3,6); Spawn(0,3,6); Spawn(0,0,6); Spawn(4,0,-2,3.2f); Spawn(4,0,3,3.2f); break;
                case 6:
                    Spawn(0,-4,1); Spawn(0,4,-1); Spawn(0,3,7);
                    Spawn(1,-8,7); Spawn(1,8,7); Spawn(2,0,5); Spawn(4,0,-4,3.5f); Spawn(5,10,7); break;
                case 7:
                    Spawn(0,-3,2); Spawn(0,3,2); Spawn(0,-3,8);
                    Spawn(1,-7,0,2.4f); Spawn(1,7,0,2.4f); Spawn(2,0,8);
                    Spawn(4,-2,7,3.8f); Spawn(5,10,7); break;
                case 10:
                    Spawn(0,-5,3); Spawn(0,5,3); Spawn(0,0,7);
                    Spawn(2,-5,8); Spawn(2,5,8); Spawn(4,-5,-6,3.2f); Spawn(4,5,-6,3.2f); Spawn(5,-10,-7); break;
                case 9: Spawn(3, 0, 6); break;
            }
            if(Index==6 || Index==7) { ReinforcementsPending=true; StartCoroutine(Reinforcements()); }
            yield return new WaitUntil(() => Enemies.Count == 0 && !ReinforcementsPending);
            Complete();
        }
        void SpawnOpening(int cleared)
        {
            var player=DungeonGame.Instance.Player.transform.position;
            var positions=new List<Vector3>();
            var path=new NavMeshPath();
            // Choose reachable ground away from whichever entrance the player used.
            for(int x=-8;x<=8;x+=4)
                for(int z=-8;z<=8;z+=4)
                {
                    Vector3 point=transform.position+new Vector3(x,0,z);
                    if(ContainsPit(point,.6f) || !NavMesh.SamplePosition(point,out var ground,1,NavMesh.AllAreas)) continue;
                    if(NavMesh.CalculatePath(ground.position,player,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete)
                        positions.Add(ground.position);
                }
            positions.Sort((a,b)=>(b-player).sqrMagnitude.CompareTo((a-player).sqrMagnitude));
            int count=2+cleared;
            for(int i=0;i<count;i++)
            {
                Vector3 spot=positions.Count>0?positions[0]:DungeonDanger.GroundBelow(transform.position+Vector3.up*6);
                Vector3 local=spot-transform.position;
                Spawn(i==1?1:0,local.x,local.z,local.y+.05f);
                positions.RemoveAll(p=>(p-spot).sqrMagnitude<12);
            }
        }
        void Warn(Vector3[] positions)
        {
            foreach(var offset in positions)
            {
                var marker=DungeonGame.Instance.Cube("Renfort annoncé",transform.position+offset+Vector3.up*.04f,new Vector3(1.5f,.04f,1.5f),new Color(1,.25f,.08f),null,PrimitiveType.Cylinder);
                marker.GetComponent<Collider>().enabled=false; Destroy(marker.GetComponent<Collider>()); Destroy(marker,.8f);
            }
            DungeonGame.Instance.Sound(.45f);
        }
        IEnumerator Reinforcements()
        {
            yield return new WaitForSeconds(3.5f);
            var spots=Index==6?new[]{new Vector3(-2,0,8),new Vector3(2,0,8),new Vector3(-4,3.4f,0),new Vector3(-10,3.4f,-6)}:new[]{new Vector3(-4,0,-8),new Vector3(4,0,-8),new Vector3(-4,3.6f,-7),new Vector3(4,3.6f,-7)};
            Warn(spots); yield return new WaitForSeconds(.65f);
            for(int i=0;i<spots.Length;i++) Spawn(i<2?0:Index==6 && i==3?5:4,spots[i].x,spots[i].z,spots[i].y);
            ReinforcementsPending=false;
        }
        public DungeonEnemy Spawn(int kind, float x, float z, float height=0)
        {
            if(kind>=4 && height==0) height=3.4f;
            Vector3 position = transform.position + new Vector3(x, height, z);
            if(kind<4 && height==0) position=DungeonDanger.GroundBelow(position+Vector3.up*6);
            if (kind<4 && NavMesh.SamplePosition(position, out var hit, 1.5f, NavMesh.AllAreas)) position = hit.position;
            var go = new GameObject(kind == 0 ? "Rampant" : kind == 1 ? "Psalmodieur" : kind == 2 ? "Pénitent" : kind==3?"Le Carillonneur":kind==4?"Séraphin":"Tisse-mur");
            go.transform.position = position;
            var enemy = go.AddComponent<DungeonEnemy>(); enemy.Initialize(kind, this); Enemies.Add(enemy); return enemy;
        }
        public void Complete()
        {
            if (Cleared) return;
            Active = false; Cleared = true;
            foreach (var gate in Gates) gate.SetActive(false);
            var g = DungeonGame.Instance; g.Cleared++; g.CleanupHazards(); g.Sound(1.5f);
            if (Index == 9) { g.End(true); return; }
            if (Index == 1 || Index==3 || Random.value < .25f) Pickup(true, transform.position+(Index==7?Vector3.back*8:Index==10?Vector3.left*9:Vector3.zero));
        }
        public static void Pickup(bool heal, Vector3 p, DungeonRoom room = null)
        {
            var go = DungeonGame.Instance.Cube(heal ? "Soin +20" : "Reliquaire", p + Vector3.up * .8f, heal ? Vector3.one * .55f : new Vector3(1, 1.2f, 1), heal ? new Color(.3f, .85f, .65f) : new Color(.85f, .65f, .25f), null, heal ? PrimitiveType.Sphere : PrimitiveType.Cube);
            Destroy(go.GetComponent<Collider>()); var item = go.AddComponent<DungeonPickup>(); item.Heal = heal; item.Room = room;
        }
    }
    public sealed class DungeonPickup : MonoBehaviour
    {
        public bool Heal;
        public DungeonRoom Room;
        void Update()
        {
            var g = DungeonGame.Instance;
            transform.Rotate(0, Time.deltaTime * 35, 0);
            if (g.Player == null || g.Paused || Vector3.Distance(g.Player.transform.position + Vector3.up * .8f, transform.position) > 2) return;
            if (Heal)
            {
                if (g.Player.Health >= 100) return;
                g.Player.Heal(20); g.Sound(2); Destroy(gameObject);
            }
            else
            {
                g.Hud.Prompt = "E — Choisir une relique";
                if (Keyboard.current.eKey.wasPressedThisFrame) { g.Hud.ShowRewards(); Destroy(gameObject); }
            }
        }
    }
}
