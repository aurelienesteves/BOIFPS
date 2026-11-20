using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BOIFPS
{
    public sealed class DungeonGame : MonoBehaviour
    {
        public static DungeonGame Instance { get; private set; }
        public DungeonPlayer Player { get; private set; }
        public DungeonHud Hud { get; private set; }
        public readonly List<DungeonRoom> Rooms = new List<DungeonRoom>();
        public readonly HashSet<int> Upgrades = new HashSet<int>();
        public bool Paused, Finished;
        public int Cleared, CombatTotal;
        [SerializeField] int dungeonSeed;
        public int Seed { get; private set; }
        public readonly List<int> Parents = new List<int>();
        public float Elapsed;
        public float Sensitivity = .10f, FieldOfView = 95;
        public bool CameraMotion = true;
        public bool Azerty = true;
        public DungeonRoom Current;
        [SerializeField] Shader dungeonShader;
        public static readonly string[] ItemNames = { "Nerf à vif", "Dent creuse", "Os bifide", "Moelle brûlante", "Vertèbre percée", "Tendon noir", "Chœur des morts", "Pendule d’os", "Sablier affamé", "Épine du vide", "Paupière miroir", "Satellite d’os", "Plume du clocher", "Poumon de cendre" };
        public static readonly string[] ItemDescriptions = { "+25 % de cadence", "+30 % de dégâts directs", "Gerbe de 5 pointes : forte de près, portée 10 m, tir plus lent", "Feu : 6 dégâts/s, 4 s ; une victime embrasée propage 14 dégâts + feu à 4 m", "Traverse un ennemi supplémentaire", "Recharge du dash réduite de 25 %", "Vos victimes libèrent 3 esprits traqueurs", "Toutes les pointes traversent et reviennent : déplacez-vous pour aligner le retour", "Recharger ralentit les tirs hostiles proches", "Le dash sème 3 mines à retardement", "Dash : capture un tir pendant 0,55 s ; prochaine attaque ×2,5 et pénétrante", "Une recharge à vide crée 2 os en orbite", "Appuyez de nouveau sur saut en vol pour bondir une seconde fois", "Recharge anticipée : chaque cartouche restante inflige 5 dégâts aux ennemis visibles à 5 m" };
        Transform geometry;
        readonly List<Material> materials = new List<Material>();
        readonly Dictionary<Color, Material> emissiveMaterials = new Dictionary<Color, Material>();
        readonly List<Mesh> generatedMeshes = new List<Mesh>();
        AudioSource audioSource;
        AudioSource weaponAudio, impactAudio;
        AudioClip tone, shotClip, impactClip, killClip;

        void Awake() { Instance = this; Time.timeScale = 1; Application.runInBackground = true; }
        void Start()
        {
            RenderSettings.ambientLight = new Color(.22f, .25f, .28f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.025f, .035f, .04f);
            RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .018f;
            geometry = new GameObject("Architecture").transform; geometry.SetParent(transform);
            GenerateDungeon();
            Physics.SyncTransforms();
            var surface = geometry.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            var player = new GameObject("Player"); player.transform.position = new Vector3(0, .1f, -5);
            Player = player.AddComponent<DungeonPlayer>();
            Hud = new GameObject("Interface").AddComponent<DungeonHud>();
            audioSource = gameObject.AddComponent<AudioSource>();
            weaponAudio = gameObject.AddComponent<AudioSource>(); impactAudio = gameObject.AddComponent<AudioSource>();
            float[] samples = new float[4410];
            for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 180 / 44100f) * (1 - i / (float)samples.Length) * .15f;
            tone = AudioClip.Create("Pulse", samples.Length, 1, 44100, false); tone.SetData(samples, 0);
            shotClip = Percussion("Cloueur — détonation", .24f, 125, .75f);
            impactClip = Percussion("Impact organique", .10f, 200, .4f);
            killClip = Percussion("Rupture", .25f, 65, .6f);
            for (int i = 0; i < Rooms.Count; i++) Rooms[i].Prepare();
            SetPaused(true);
            Hud.ShowIntro();
        }

        void GenerateDungeon()
        {
            Seed = dungeonSeed == 0 ? System.Guid.NewGuid().GetHashCode() : dungeonSeed;
            var random = new System.Random(Seed);
            var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            // Grow a winding main route along two perpendicular axes so it cannot trap itself.
            int heading = random.Next(4);
            int side = (heading + (random.Next(2)==0 ? 1 : 3)) % 4;
            var cells = new List<Vector2Int> { Vector2Int.zero };
            var roles = new List<int> { 0 };
            var encounters = new List<int> { 1,2,3,6,7,10,12,13 };
            for(int i=encounters.Count-1;i>0;i--)
            {
                int j=random.Next(i+1); int temp=encounters[i]; encounters[i]=encounters[j]; encounters[j]=temp;
            }
            int length=random.Next(7,10);
            Parents.Add(-1);
            for(int i=1;i<=length;i++)
            {
                cells.Add(cells[i-1]+directions[random.Next(2)==0?heading:side]);
                Parents.Add(i-1);
                roles.Add(i==length?9:i==length-1?8:i==3?4:encounters[(i-1)%encounters.Count]);
            }
            int branches=random.Next(3,6);
            for(int i=0;i<branches;i++)
            {
                var options=new List<(int parent,Vector2Int cell)>();
                for(int j=1;j<cells.Count;j++)
                {
                    if(roles[j]==9 || roles[j]==11) continue;
                    foreach(var d in directions)
                        if(!cells.Contains(cells[j]+d)) options.Add((j,cells[j]+d));
                }
                var option=options[random.Next(options.Count)];
                Parents.Add(option.parent); cells.Add(option.cell);
                roles.Add(i==branches-1?11:i==0?12:i==1?13:encounters[random.Next(encounters.Count)]);
            }
            string[] names={"Entrée","Nef des griffes","Chœur","Crypte","Reliquaire","Carrefour","Communion","Fosses","Antichambre","Le Carillonneur","Sacrifice","Reliquaire secret","Cloître suspendu","Escaliers du beffroi"};
            for(int i=0;i<cells.Count;i++)
            {
                var room=new GameObject(names[roles[i]]).AddComponent<DungeonRoom>();
                room.Index=roles[i]; room.Height=random.Next(27,36)*.1f; room.GallerySpan=random.Next(5,8); room.GalleryDepth=random.Next(5,8);
                room.transform.SetParent(transform); room.transform.position=new Vector3(cells[i].x*32,0,cells[i].y*32);
                Rooms.Add(room); if(room.IsCombat) CombatTotal++;
                var doors=new HashSet<Vector2Int>();
                for(int j=1;j<cells.Count;j++)
                    if(Parents[j]==i) doors.Add(cells[j]-cells[i]);
                    else if(j==i) doors.Add(cells[Parents[j]]-cells[i]);
                BuildRoom(room,doors.Contains(Vector2Int.down),doors.Contains(Vector2Int.up),doors.Contains(Vector2Int.right),doors.Contains(Vector2Int.left));
                if(i>0) Corridor((room.transform.position+Rooms[Parents[i]].transform.position)*.5f,cells[i].x!=cells[Parents[i]].x);
            }
        }
        void BuildRoom(DungeonRoom room, bool south, bool north, bool east, bool west)
        {
            Vector3 c = room.transform.position;
            Color floorColor = room.Index == 2 ? new Color(.19f,.14f,.13f) : room.Index == 9 ? new Color(.17f,.13f,.19f) : room.Index == 10 ? new Color(.12f,.18f,.15f) : new Color(.13f,.16f,.17f);
            BuildFloor(room,floorColor);
            Cube("Voûte", c + Vector3.up * (room.IsCombat ? 8 : 6.5f), new Vector3(24, 1, 24), new Color(.09f, .1f, .12f), geometry);
            for (int side = 0; side < 4; side++)
            {
                bool opening = side == 0 ? north : side == 1 ? south : side == 2 ? east : west;
                Vector3 outward = side == 0 ? Vector3.forward : side == 1 ? Vector3.back : side == 2 ? Vector3.right : Vector3.left;
                Vector3 tangent = new Vector3(outward.z, 0, -outward.x);
                if (!opening) Wall(c + outward * 12, side, 24);
                else
                {
                    Wall(c + outward * 12 + tangent * 7.5f, side, 9);
                    Wall(c + outward * 12 - tangent * 7.5f, side, 9);
                    var gate = Cube("Herse", c + outward * 12 + Vector3.up * 2.5f, side < 2 ? new Vector3(6, 5, .5f) : new Vector3(.5f, 5, 6), new Color(.35f, .12f, .10f), room.transform);
                    var modifier = gate.AddComponent<NavMeshModifier>(); modifier.ignoreFromBuild = true;
                    gate.SetActive(false); room.Gates.Add(gate);
                    Cube("Linteau", c + outward * 12 + Vector3.up * 5.5f, side < 2 ? new Vector3(6, 1, 1) : new Vector3(1, 1, 6), new Color(.22f, .23f, .22f), geometry);
                }
            }
            BuildLayout(room);
            // A continuous line on the floor points toward the main route.
            if(room.Pits.Count==0)
                Cube("Chemin", c + new Vector3(0, .015f, 0), new Vector3(.12f, .02f, 22), new Color(.52f, .37f, .21f), geometry).GetComponent<Collider>().enabled=false;
        }
        void BuildFloor(DungeonRoom room,Color color)
        {
            if(room.Index==12) room.Pits.Add(new Rect(-2,1,4,4));
            if(room.Index==6) { room.Pits.Add(new Rect(-7,3,4,4)); room.Pits.Add(new Rect(3,-7,4,4)); }
            if(room.Index==7) { room.Pits.Add(new Rect(-2,-6,4,4)); room.Pits.Add(new Rect(-2,0,4,4)); }
            if(room.Index==10) { room.Pits.Add(new Rect(-7.5f,-2,3,4)); room.Pits.Add(new Rect(4.5f,-2,3,4)); }
            Vector3 c=room.transform.position;
            var xs=new SortedSet<float>{-12,12}; var zs=new SortedSet<float>{-12,12};
            foreach(var pit in room.Pits) { xs.Add(pit.xMin); xs.Add(pit.xMax); zs.Add(pit.yMin); zs.Add(pit.yMax); }
            var x=new List<float>(xs); var z=new List<float>(zs);
            for(int i=0;i<x.Count-1;i++) for(int j=0;j<z.Count-1;j++)
            {
                var center=c+new Vector3((x[i]+x[i+1])*.5f,0,(z[j]+z[j+1])*.5f);
                if(!room.ContainsPit(center)) Cube("Dalle",center+Vector3.down*.5f,new Vector3(x[i+1]-x[i],1,z[j+1]-z[j]),color,geometry);
            }
            foreach(var pit in room.Pits)
            {
                Vector3 p=c+new Vector3(pit.center.x,0,pit.center.y);
                foreach(int sign in new[]{-1,1})
                {
                    Cube("Paroi de fosse",p+new Vector3(sign*pit.width*.5f,-3,0),new Vector3(.15f,6,pit.height),new Color(.06f,.07f,.08f),geometry);
                    Cube("Paroi de fosse",p+new Vector3(0,-3,sign*pit.height*.5f),new Vector3(pit.width,6,.15f),new Color(.06f,.07f,.08f),geometry);
                    var edge=Cube("Bord de fosse",p+new Vector3(sign*pit.width*.5f,.035f,0),new Vector3(.12f,.03f,pit.height),new Color(.65f,.18f,.05f),geometry);
                    edge.GetComponent<Collider>().enabled=false; edge.GetComponent<Renderer>().sharedMaterial=Emissive(new Color(.65f,.18f,.05f),1.5f);
                    edge=Cube("Bord de fosse",p+new Vector3(0,.035f,sign*pit.height*.5f),new Vector3(pit.width,.03f,.12f),new Color(.65f,.18f,.05f),geometry);
                    edge.GetComponent<Collider>().enabled=false; edge.GetComponent<Renderer>().sharedMaterial=Emissive(new Color(.65f,.18f,.05f),1.5f);
                }
                var bottom=Cube("Profondeur de fosse",p+Vector3.down*6,new Vector3(pit.width,.1f,pit.height),Color.black,geometry);
                bottom.GetComponent<Collider>().enabled=false;
            }
        }
        void Cover(Vector3 center, float x, float z, float width, float height, float depth, string label = "Sarcophage")
        {
            var cover = Cube(label, center + new Vector3(x,height*.5f,z), new Vector3(width,height,depth), new Color(.29f,.27f,.25f), geometry);
            var modifier = cover.AddComponent<NavMeshModifier>(); modifier.overrideArea = true; modifier.area = 1;
            Cube("Corniche", center + new Vector3(x,height+.06f,z), new Vector3(width+.1f,.12f,depth+.1f), new Color(.39f,.34f,.27f), geometry).GetComponent<Collider>().enabled = false;
        }
        void BuildLayout(DungeonRoom room)
        {
            Vector3 c = room.transform.position;
            switch (room.Index)
            {
                case 12: case 13:
                    float height=room.Height;
                    float span=room.GallerySpan; float depth=room.GalleryDepth;
                    foreach(int sign in new[]{-1,1})
                    {
                        float z=room.Index==12?3:sign*3;
                        Platform(c,new Vector3(sign*span,height,z),new Vector2(4,depth),"Palier du cloître");
                        Ramp(c+new Vector3(sign*span,0,z-8.4f),c+new Vector3(sign*span,height,z-depth*.5f),3);
                    }
                    if(room.Index==12) Platform(c,new Vector3(0,height,3),new Vector2(span*2,3),"Pont suspendu");
                    else
                    {
                        Platform(c,new Vector3(0,height,-3),new Vector2(span*2,3),"Coursive basse");
                        Platform(c,new Vector3(span-1,height,0),new Vector2(3,7),"Coursive en retour");
                    }
                    break;
                case 1: // A long central nave with low cover that the player can vault.
                    Cover(c,-6,0,3,1.1f,10); Cover(c,6,0,3,1.1f,10);
                    Platform(c,new Vector3(0,3,7),new Vector2(21,3),"Balcon de la nef");
                    Ramp(c+new Vector3(-9,0,-3),c+new Vector3(-9,3,5.5f),2.5f);
                    Ramp(c+new Vector3(9,0,-3),c+new Vector3(9,3,5.5f),2.5f); break;
                case 2:
                    Cover(c,0,0,5,1.2f,4,"Autel du chœur");
                    Cover(c,-4,-3,1.3f,5,1.3f,"Colonne"); Cover(c,4,-3,1.3f,5,1.3f,"Colonne");
                    Platform(c,new Vector3(0,2.6f,6),new Vector2(18,4),"Galerie du chœur");
                    Ramp(c+new Vector3(-7,0,-5),c+new Vector3(-7,2.6f,4),3);
                    Ramp(c+new Vector3(7,0,-5),c+new Vector3(7,2.6f,4),3); break;
                case 3: // Filled corners produce a cross-shaped combat space.
                    for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) Cover(c,x*8,z*8,7,4.5f,7,"Crypte fermée");
                    Platform(c,new Vector3(0,2.6f,0),new Vector2(3,12),"Pont de la pénitence");
                    Ramp(c+new Vector3(0,0,-11),c+new Vector3(0,2.6f,-6),3);
                    Ramp(c+new Vector3(0,0,11),c+new Vector3(0,2.6f,6),3); break;
                case 6:
                    Cover(c,-4,-2,8,1.15f,2,"Tombeau décalé"); Cover(c,4,3,8,1.15f,2,"Tombeau décalé");
                    Platform(c,new Vector3(-9,3.2f,2),new Vector2(3,10),"Tribune de communion");
                    Platform(c,new Vector3(0,3.2f,8),new Vector2(21,3),"Pont des fosses");
                    Ramp(c+new Vector3(-9,0,-11),c+new Vector3(-9,3.2f,-3),3);
                    Ramp(c+new Vector3(9,0,-3),c+new Vector3(9,3.2f,6.5f),3); break;
                case 7:
                    for(int x=-1;x<=1;x+=2)
                    {
                        Platform(c,new Vector3(x*7,2.4f,0),new Vector2(4,8),"Tribune des fosses");
                        Ramp(c+new Vector3(x*7,0,-11),c+new Vector3(x*7,2.4f,-4),3);
                        Ramp(c+new Vector3(x*7,0,11),c+new Vector3(x*7,2.4f,4),3);
                    }
                    Platform(c,new Vector3(0,2.4f,2),new Vector2(10,2.5f),"Passerelle");
                    Cover(c,-3,-5,1.3f,4,1.3f,"Ossuaire"); Cover(c,3,6,1.3f,4,1.3f,"Ossuaire");
                    Cover(c,0,-1,3,1,2,"Tombe centrale"); break;
                case 10:
                    Cover(c,0,0,2,4,10,"Cloison de la fosse");
                    foreach(int side in new[]{-1,1})
                    {
                        Platform(c,new Vector3(side*8,2.8f,5),new Vector2(4,5),"Balcon du sacrifice");
                        Ramp(c+new Vector3(side*8,0,-11),c+new Vector3(side*8,2.8f,2.5f),3);
                    }
                    Platform(c,new Vector3(0,2.8f,8),new Vector2(20,3),"Passerelle du sacrifice"); break;
                case 9:
                    foreach(int side in new[]{-1,1})
                    {
                        Platform(c,new Vector3(side*9.5f,2.8f,2),new Vector2(2.8f,10),"Tribune du glas");
                        Ramp(c+new Vector3(side*9.5f,0,-11),c+new Vector3(side*9.5f,2.8f,-3),2.8f);
                        Ramp(c+new Vector3(side*9.5f,0,11),c+new Vector3(side*9.5f,2.8f,7),2.8f);
                    }
                    Cover(c,-7,0,1.7f,5,1.7f,"Pilier du glas"); Cover(c,7,0,1.7f,5,1.7f,"Pilier du glas");
                    for(int n=0;n<24;n++)
                    {
                        float a=n*Mathf.PI/12;
                        var rune=Cube("Cercle rituel",c+new Vector3(Mathf.Sin(a)*8,.02f,Mathf.Cos(a)*8),new Vector3(.3f,.025f,1),new Color(.6f,.23f,.1f),geometry);
                        rune.GetComponent<Collider>().enabled=false; rune.transform.rotation=Quaternion.Euler(0,n*15,0);
                    }
                    break;
                case 4: case 11:
                    Cover(c,-7,0,4,4,10,"Niche du reliquaire"); Cover(c,7,0,4,4,10,"Niche du reliquaire"); break;
                case 8:
                    Cover(c,-8,1,3,1,6,"Banc"); Cover(c,8,1,3,1,6,"Banc"); break;
                case 5:
                    Cover(c,-7,3,2,5,2,"Obélisque"); break;
            }
            for(int i=0;i<4;i++)
            {
                Vector3 p=c+new Vector3(i%2==0?-10:10,3,i<2?-7:7);
                Glow(p,room.Index==9?new Color(.8f,.15f,.08f):i%2==0?new Color(.3f,.7f,.65f):new Color(1,.35f,.16f),geometry);
            }
        }
        void Platform(Vector3 room,Vector3 offset,Vector2 size,string label)
        {
            Cube(label,room+offset-Vector3.up*.15f,new Vector3(size.x,.3f,size.y),new Color(.34f,.29f,.25f),geometry);
            // Edge markings are visual; both players and agents may leave by the ramps.
            var edge=Cube("Bord de galerie",room+offset+Vector3.up*.015f,new Vector3(size.x,.025f,.12f),new Color(.7f,.5f,.22f),geometry);
            edge.GetComponent<Collider>().enabled=false;
            edge.GetComponent<Renderer>().sharedMaterial=Emissive(new Color(.7f,.5f,.22f),2);
            foreach(int sign in new[]{-1,1})
            {
                var rail=Cube("Repère de hauteur",room+offset+new Vector3(sign*size.x*.5f,.02f,0),new Vector3(.08f,.04f,size.y),new Color(.3f,.8f,.7f),geometry);
                rail.GetComponent<Collider>().enabled=false;
                rail.GetComponent<Renderer>().sharedMaterial=Emissive(new Color(.3f,.8f,.7f),2);
            }
        }
        void Ramp(Vector3 start,Vector3 end,float width)
        {
            Vector3 forward=Vector3.ProjectOnPlane(end-start,Vector3.up).normalized;
            Vector3 side=Vector3.Cross(Vector3.up,forward)*width*.5f;
            Vector3 bottomStart=new Vector3(start.x,-.15f,start.z),bottomEnd=new Vector3(end.x,-.15f,end.z);
            var mesh=new Mesh { name="Rampe de galerie" };
            mesh.vertices=new[]{start-side,start+side,end-side,end+side,bottomStart-side,bottomStart+side,bottomEnd-side,bottomEnd+side};
            mesh.triangles=new[]{0,2,1,1,2,3,4,5,6,5,7,6,0,4,2,2,4,6,1,3,5,3,7,5,2,6,3,3,6,7,0,1,4,1,5,4};
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); generatedMeshes.Add(mesh);
            var go=new GameObject("Rampe accessible"); go.transform.SetParent(geometry,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=Material(new Color(.31f,.29f,.26f));
            go.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        void Wall(Vector3 p, int side, float length) { Cube("Mur", p + Vector3.up * 3, side < 2 ? new Vector3(length, 6, 1) : new Vector3(1, 6, length), new Color(.19f, .21f, .22f), geometry); }
        void Corridor(Vector3 p, bool horizontal)
        {
            Cube("Passage", p + Vector3.down * .5f, horizontal ? new Vector3(8, 1, 6) : new Vector3(6, 1, 8), new Color(.16f, .16f, .17f), geometry);
            Cube("Plafond passage", p + Vector3.up * 5.5f, horizontal ? new Vector3(8, 1, 6) : new Vector3(6, 1, 8), new Color(.1f, .11f, .12f), geometry);
            for (int i = -1; i <= 1; i += 2) Cube("Mur passage", p + (horizontal ? Vector3.forward : Vector3.right) * 3 * i + Vector3.up * 2.5f, horizontal ? new Vector3(8, 5, .5f) : new Vector3(.5f, 5, 8), new Color(.2f, .2f, .22f), geometry);
        }
        public GameObject Cube(string label, Vector3 position, Vector3 scale, Color color, Transform parent = null, PrimitiveType shape = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = label; go.transform.SetParent(parent);
            go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Material(color); return go;
        }
        public Material Material(Color color)
        {
            foreach (var m in materials) if (m.color == color) return m;
            var mat = new Material(dungeonShader != null ? dungeonShader : Shader.Find("Universal Render Pipeline/Lit")); mat.color = color;
            materials.Add(mat); return mat;
        }
        public Material Emissive(Color color, float strength)
        {
            if (emissiveMaterials.TryGetValue(color, out var material)) return material;
            material = new Material(dungeonShader != null ? dungeonShader : Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * strength);
            emissiveMaterials.Add(color, material); materials.Add(material); return material;
        }
        public void Glow(Vector3 p, Color color, Transform parent)
        {
            var go = new GameObject("Veilleuse"); go.transform.SetParent(parent); go.transform.position = p;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.range = 12; light.intensity = 2.5f;
            var bulb = Cube("Flamme", p, Vector3.one * .2f, color, parent, PrimitiveType.Sphere); Destroy(bulb.GetComponent<Collider>());
        }
        void Update()
        {
            if (Keyboard.current == null || Player == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame && !Finished && !Hud.Choosing) { SetPaused(!Paused); Hud.ShowPause(Paused); }
            if (!Paused && !Finished) Elapsed += Time.deltaTime;
        }
        public void Sound(float pitch = 1) { if (audioSource != null) { audioSource.pitch = pitch; audioSource.PlayOneShot(tone); } }
        AudioClip Percussion(string label, float duration, float bass, float volume)
        {
            var noise = new System.Random(137); int length = Mathf.CeilToInt(44100 * duration); var data = new float[length]; float low = 0;
            for(int i=0;i<length;i++)
            {
                float t=i/44100f, envelope=Mathf.Exp(-t*22);
                float n=(float)noise.NextDouble()*2-1; low=Mathf.Lerp(low,n,.24f);
                float thump=Mathf.Sin(2*Mathf.PI*(bass*t-140*t*t));
                data[i]=Mathf.Clamp((low*.9f+thump*.6f)*envelope+ n*Mathf.Exp(-t*180)*.4f,-1,1)*volume;
            }
            var clip=AudioClip.Create(label,length,1,44100,false); clip.SetData(data,0); return clip;
        }
        public void Gunshot() { weaponAudio.pitch=Random.Range(.94f,1.06f); weaponAudio.PlayOneShot(shotClip); }
        public void ImpactSound(bool kill) { impactAudio.pitch=Random.Range(.9f,1.1f); impactAudio.PlayOneShot(kill?killClip:impactClip,kill?.9f:.6f); }
        public void SetPaused(bool value)
        {
            Paused = value; Time.timeScale = value ? 0 : 1;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = value;
        }
        public void End(bool victory)
        {
            if (Finished) return; Finished = true; CleanupHazards(); SetPaused(true); Hud.ShowEnd(victory);
        }
        public void Restart() { Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public void CleanupHazards()
        {
            if(Player!=null) Player.GetComponent<DungeonRelics>().ClearChains();
            foreach(var seed in FindObjectsByType<DungeonSeedCharge>()) { seed.enabled=false; Destroy(seed.gameObject); }
            foreach (var p in FindObjectsByType<DungeonProjectile>()) Destroy(p.gameObject);
            foreach (var z in FindObjectsByType<DungeonDanger>()) Destroy(z.gameObject);
            foreach(var mine in FindObjectsByType<DungeonMine>()) Destroy(mine.gameObject);
        }
        void OnDestroy()
        {
            Time.timeScale = 1; Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            foreach (var m in materials) if (m != null) Destroy(m);
            foreach(var mesh in generatedMeshes) if(mesh!=null) Destroy(mesh);
            if (tone != null) Destroy(tone);
            if (shotClip != null) Destroy(shotClip); if (impactClip != null) Destroy(impactClip); if (killClip != null) Destroy(killClip);
            if (Instance == this) Instance = null;
        }
    }
}
