using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BOIFPS
{
    public sealed class DungeonHud : MonoBehaviour
    {
        public bool Choosing { get; private set; }
        public string Prompt;
        Canvas canvas;
        RectTransform overlay;
        TextMeshProUGUI status, room, items, boss, hint, crosshair, direction;
        UnityEngine.UI.Image damageFlash;
        readonly List<TextMeshProUGUI> map = new List<TextMeshProUGUI>();
        float hitTime, confirmTime, shotTime;
        TextMeshProUGUI relicEvents;
        readonly float[] relicUntil=new float[DungeonGame.ItemNames.Length], relicLast=new float[DungeonGame.ItemNames.Length];
        readonly string[] relicMessage=new string[DungeonGame.ItemNames.Length];
        readonly int[] relicCount=new int[DungeonGame.ItemNames.Length];
        public string LastRelicFeedback { get; private set; }
        float acquiredUntil;
        string acquiredMessage;
        public void RelicFeedback(int id,string message,bool acquired=false)
        {
            if(!acquired && relicCount[id]>0 && relicMessage[id]==message && Time.time-relicLast[id]<.8f) return;
            relicLast[id]=Time.time; relicUntil[id]=Time.time+2.2f; relicMessage[id]=message; relicCount[id]++;
            LastRelicFeedback=DungeonGame.ItemNames[id]+" — "+message;
            if(acquired) { acquiredMessage="RELIQUE ACQUISE : "+DungeonGame.ItemNames[id]+"\n"+DungeonGame.ItemDescriptions[id]; acquiredUntil=Time.time+4; }
        }
        TMP_FontAsset font;
        void Awake()
        {
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var events = new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<InputSystemUIInputModule>();
            status = Text("Vitalité", transform, new Vector2(35, -30), new Vector2(680, 220), 30, new Vector2(0, 1));
            room = Text("Lieu", transform, new Vector2(0, -30), new Vector2(800, 80), 28, new Vector2(.5f, 1)); room.alignment = TextAlignmentOptions.Center;
            items = Text("Reliques", transform, new Vector2(35, 35), new Vector2(1100, 170), 22, Vector2.zero);
            relicEvents=Text("Activations des reliques",transform,new Vector2(0,300),new Vector2(1100,140),23,new Vector2(.5f,0)); relicEvents.alignment=TextAlignmentOptions.Center;
            boss = Text("Boss", transform, new Vector2(0, -100), new Vector2(1000, 70), 27, new Vector2(.5f, 1)); boss.alignment = TextAlignmentOptions.Center;
            hint = Text("Interaction", transform, new Vector2(0, 215), new Vector2(1100, 60), 26, new Vector2(.5f, 0)); hint.alignment = TextAlignmentOptions.Center;
            crosshair = Text("Réticule", transform, Vector2.zero, new Vector2(80, 80), 30, new Vector2(.5f, .5f)); crosshair.alignment = TextAlignmentOptions.Center; crosshair.text = "+";
            direction = Text("Impact", transform, new Vector2(0, -90), new Vector2(500, 70), 24, new Vector2(.5f, .5f)); direction.alignment = TextAlignmentOptions.Center;
            var dungeon=DungeonGame.Instance;
            var bounds=new Bounds(Vector3.zero,Vector3.zero);
            foreach(var tile in dungeon.Rooms) bounds.Encapsulate(tile.transform.position);
            float scale=Mathf.Min(1,220/Mathf.Max(1,bounds.size.x,bounds.size.z));
            for(int i=0;i<dungeon.Rooms.Count;i++)
            {
                var tile=dungeon.Rooms[i]; Vector3 delta=tile.transform.position-bounds.max;
                Vector2 pos=new Vector2(-30+delta.x*scale,-30+delta.z*scale);
                if(i>0)
                {
                    var parent=dungeon.Rooms[dungeon.Parents[i]].transform.position-tile.transform.position;
                    var link=Text("Passage",transform,pos+new Vector2(parent.x,parent.z)*scale*.5f,new Vector2(20,20),13,Vector2.one);
                    link.text=parent.x==0?"|":"–"; link.alignment=TextAlignmentOptions.Center;
                }
                var label=Text(tile.name,transform,pos,new Vector2(20,20),14,Vector2.one);
                label.alignment=TextAlignmentOptions.Center;
                label.text=tile.Index==9?"B":tile.Index==0?"E":tile.Index==4 || tile.Index==11?"R":tile.Index==8?"+":"●";
                map.Add(label);
            }
            var flash = new GameObject("Dégâts", typeof(RectTransform), typeof(UnityEngine.UI.Image)); flash.transform.SetParent(transform, false);
            damageFlash = flash.GetComponent<UnityEngine.UI.Image>(); damageFlash.color = Color.clear; damageFlash.raycastTarget = false;
            Stretch(flash.GetComponent<RectTransform>());
        }
        void LateUpdate()
        {
            var g = DungeonGame.Instance;
            if (g.Player == null) return;
            status.text = $"PV {Mathf.CeilToInt(g.Player.Health)} / 100\nDASH {(g.Player.DashCooldown <= 0 ? "PRÊT" : g.Player.DashCooldown.ToString("0.0") + " s")}";
            status.text += g.Player.Reloading ? $"\nRECHARGE  {g.Player.ReloadRemaining:0.0} s" : $"\n{g.Player.WeaponName}  {g.Player.Ammo} / 6   [R]";
            room.text = (g.Current == null ? "Les Fosses de la Communion" : g.Current.name) + (g.Current != null && g.Current.Active ? "  —  COMBAT" : "");
            string acquired = ""; for(int n=0;n<DungeonGame.ItemNames.Length;n++) if(g.Upgrades.Contains(n))
                acquired += "<color=#"+ColorUtility.ToHtmlStringRGB(DungeonRelics.ColorFor(n))+">"+DungeonGame.ItemNames[n]+"</color>   ";
            items.text = "RELIQUES  " + (acquired.Length == 0 ? "Aucune" : acquired) + $"\nSalles nettoyées : {g.Cleared}/{g.CombatTotal}   •   {Mathf.FloorToInt(g.Elapsed / 60):00}:{Mathf.FloorToInt(g.Elapsed % 60):00}";
            relicEvents.text=Time.time<acquiredUntil?acquiredMessage:"";
            if(Time.time>=acquiredUntil)
            {
                int shown=0;
                for(int n=0;n<DungeonGame.ItemNames.Length && shown<3;n++) if(relicUntil[n]>Time.time)
                {
                    relicEvents.text+="<color=#"+ColorUtility.ToHtmlStringRGB(DungeonRelics.ColorFor(n))+">"+DungeonGame.ItemNames[n]+"</color> — "+relicMessage[n]+"\n"; shown++;
                }
            }
            if(g.Player.Has(10) && g.Player.ReflectReady) status.text+="\n<color=#8CE6FF>CAPTURE ARMÉE</color>";
            if(g.Player.EmpoweredShot) status.text+="\n<color=#8CE6FF>RIPOSTE ×2,5 PRÊTE</color>";
            boss.text = "";
            if (g.Current != null) foreach (var enemy in g.Current.Enemies) if (enemy != null && enemy.Kind == 3) boss.text = $"LE CARILLONNEUR   {Mathf.CeilToInt(enemy.Health)} / {enemy.MaxHealth:0}";
            hint.text = Prompt; Prompt = "";
            damageFlash.color = new Color(.7f, .02f, .01f, Mathf.Max(0, hitTime - Time.unscaledTime) * .45f);
            crosshair.text = Time.unscaledTime < confirmTime ? "×" : g.Player.EmpoweredShot?"◇":g.Player.Has(2)?"( + )":g.Player.Has(7)?"↔":"+";
            crosshair.color = Time.unscaledTime < confirmTime ? new Color(1, .55f, .25f) : Color.white;
            crosshair.fontSize=30+Mathf.Clamp01((shotTime-Time.unscaledTime)/.12f)*15;
            if (Time.unscaledTime > hitTime) direction.text = "";
            for (int i = 0; i < map.Count; i++) map[i].color = g.Current == g.Rooms[i] ? Color.yellow : g.Rooms[i].Entered ? new Color(.65f, .85f, .8f) : new Color(.25f, .27f, .3f);
        }
        public void Hit(Vector3 source)
        {
            hitTime = Time.unscaledTime + .65f;
            Vector3 local = DungeonGame.Instance.Player.transform.InverseTransformPoint(source);
            direction.text = Mathf.Abs(local.x) > Mathf.Abs(local.z) ? local.x > 0 ? "IMPACT À DROITE →" : "← IMPACT À GAUCHE" : local.z > 0 ? "IMPACT DEVANT ↑" : "IMPACT DERRIÈRE ↓";
        }
        public void HitConfirm() { confirmTime = Time.unscaledTime + .12f; }
        public void Shot() { shotTime=Time.unscaledTime+.12f; }
        TextMeshProUGUI Text(string name, Transform parent, Vector2 position, Vector2 size, int pointSize, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor; rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = go.AddComponent<TextMeshProUGUI>(); if (font != null) text.font = font;
            text.fontSize = pointSize; text.enableAutoSizing = false; text.color = new Color(.91f, .89f, .81f); text.raycastTarget = false;
            return text;
        }
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        void Panel(string title, string subtitle)
        {
            Close();
            var go = new GameObject("Menu", typeof(RectTransform), typeof(UnityEngine.UI.Image)); go.transform.SetParent(transform, false);
            overlay = go.GetComponent<RectTransform>(); Stretch(overlay); go.GetComponent<UnityEngine.UI.Image>().color = new Color(.025f, .035f, .04f, .97f);
            var header = Text("Titre", overlay, new Vector2(0, -140), new Vector2(1500, 100), 52, new Vector2(.5f, 1)); header.text = title; header.alignment = TextAlignmentOptions.Center;
            var desc = Text("Description", overlay, new Vector2(0, -260), new Vector2(1400, 160), 26, new Vector2(.5f, 1)); desc.text = subtitle; desc.alignment = TextAlignmentOptions.Center;
        }
        void Button(string label, float y, UnityEngine.Events.UnityAction action,float height=68,int pointSize=25)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button)); go.transform.SetParent(overlay, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(.5f, 1); r.pivot = new Vector2(.5f, 1); r.anchoredPosition = new Vector2(0, -y); r.sizeDelta = new Vector2(1050, height);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.18f, .24f, .25f);
            var button = go.GetComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(action);
            var text = Text("Libellé", go.transform, Vector2.zero, Vector2.zero, pointSize, new Vector2(.5f, .5f)); Stretch(text.rectTransform); text.alignment = TextAlignmentOptions.Center; text.text = label;
        }
        void Close() { if (overlay != null) { overlay.gameObject.SetActive(false); Destroy(overlay.gameObject); overlay = null; } }
        public void ShowIntro()
        {
            Panel("LES FOSSES DE LA COMMUNION", "Un donjon. Cinq menaces. Une cloche à faire taire.\nZQSD / WASD : déplacement   •   Souris : viser et tirer\nEspace : saut   •   Maj : dash   •   R : recharger\nE : relique   •   Échap : pause");
            Button("Entrer dans les fosses", 460, () => { Close(); DungeonGame.Instance.SetPaused(false); });
            Button("Clavier : AZERTY / QWERTY", 550, () => { DungeonGame.Instance.Azerty = !DungeonGame.Instance.Azerty; ShowIntro(); });
            var note = Text("Clavier actif", overlay, new Vector2(0, -650), new Vector2(1000, 60), 24, new Vector2(.5f, 1)); note.text = DungeonGame.Instance.Azerty ? "Configuration active : AZERTY" : "Configuration active : QWERTY"; note.alignment = TextAlignmentOptions.Center;
        }
        public void ShowPause(bool visible)
        {
            if (!visible) { Close(); return; }
            var g = DungeonGame.Instance;
            Panel("PAUSE",g.Player.WeaponName+"\nGraine du donjon "+g.Seed);
            Button("Reprendre", 400, () => { Close(); g.SetPaused(false); });
            Button($"Sensibilité : {g.Sensitivity:0.00} — cliquer pour changer", 480, () => { g.Sensitivity = g.Sensitivity >= .2f ? .05f : g.Sensitivity + .05f; ShowPause(true); });
            Button($"Champ de vision : {g.FieldOfView:0}° — cliquer pour changer", 560, () => { g.FieldOfView = g.FieldOfView >= 110 ? 80 : g.FieldOfView + 5; ShowPause(true); });
            Button("Secousses et balancement : " + (g.CameraMotion ? "oui" : "non"), 640, () => { g.CameraMotion = !g.CameraMotion; ShowPause(true); });
            Button("Clavier : " + (g.Azerty ? "AZERTY" : "QWERTY"), 720, () => { g.Azerty = !g.Azerty; ShowPause(true); });
            Button("Recommencer", 800, g.Restart);
            Button("Quitter", 880, Quit);
            var inventory=Text("Effets des reliques",overlay,new Vector2(30,-310),new Vector2(390,700),20,new Vector2(0,1));
            inventory.text="RELIQUES ÉQUIPÉES\n\n";
            for(int id=0;id<DungeonGame.ItemNames.Length;id++) if(g.Upgrades.Contains(id)) inventory.text+="<color=#"+ColorUtility.ToHtmlStringRGB(DungeonRelics.ColorFor(id))+">"+DungeonGame.ItemNames[id]+"</color>\n"+DungeonGame.ItemDescriptions[id]+"\n\n";
        }
        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        public List<int> RewardChoices()
        {
            var g=DungeonGame.Instance;
            var available=new List<int>(); var choices=new List<int>();
            for(int i=0;i<DungeonGame.ItemNames.Length;i++)
                if(!g.Upgrades.Contains(i) && (g.Upgrades.Count>0 || DungeonRelics.IsTransformation(i))) available.Add(i);
            for(int n=0;n<3 && available.Count>0;n++)
            {
                var transformations=available.FindAll(DungeonRelics.IsTransformation);
                var pool=n==0 && transformations.Count>0?transformations:available;
                int id=pool[Random.Range(0,pool.Count)]; choices.Add(id); available.Remove(id);
            }
            return choices;
        }
        public void ShowRewards()
        {
            var g=DungeonGame.Instance; g.SetPaused(true); Choosing=true;
            if(!g.Player.Armed)
            {
                Panel("CHOISIR UNE ARME","Votre arme vous accompagne jusqu’à la fin de la descente.");
                for(int i=0;i<DungeonArmament.Names.Length;i++)
                {
                    var choice=(DungeonWeapon)i;
                    Button(DungeonArmament.Names[i]+"\n"+DungeonArmament.Descriptions[i],430+i*135,()=>{ g.Player.Equip(choice); ShowForge(); },110,22);
                }
            }
            else ShowForge();
        }
        void ShowForge()
        {
            var player=DungeonGame.Instance.Player;
            int count=0;
            foreach(var trait in DungeonArmament.Traits) if((player.Traits&trait)==0) count++;
            if(count==0) { ShowRelics(); return; }
            Panel("FORGER LE "+DungeonArmament.Names[(int)player.Weapon].ToUpperInvariant(),"Choisissez un adjectif. Ses effets se cumulent avec les reliques.");
            int row=0;
            for(int i=0;i<DungeonArmament.Traits.Length;i++)
            {
                var trait=DungeonArmament.Traits[i];
                if((player.Traits&trait)!=0) continue;
                Button(trait+"\n"+DungeonArmament.TraitDescriptions[i],430+row++*135,()=>{ player.AddTrait(trait); ShowRelics(); },110,22);
            }
        }
        void ShowRelics()
        {
            var g=DungeonGame.Instance;
            Panel("CHOISIR UNE RELIQUE",g.Upgrades.Count==0?"Choisissez votre première façon de combattre.\nLes reliques suivantes pourront transformer et combiner ses effets.":"Un seul don. Combinez ses effets avec vos reliques équipées.");
            var choices=RewardChoices();
            if(choices.Count==0) Button("Continuer",430,()=>{ Choosing=false; Close(); g.SetPaused(false); });
            for(int n=0;n<choices.Count;n++)
            {
                int id=choices[n];
                Button("<b>"+DungeonGame.ItemNames[id]+"</b>\n"+DungeonGame.ItemDescriptions[id],430+n*135,()=>
                {
                    g.Upgrades.Add(id); Choosing=false; Close(); g.SetPaused(false); g.Sound(2); RelicFeedback(id,"Équipée",true);
                },110,22);
            }
        }
        public void ShowEnd(bool won)
        {
            var g = DungeonGame.Instance;
            string acquired = ""; foreach (int i in g.Upgrades) acquired += DungeonGame.ItemNames[i] + "   ";
            Panel(won ? "LA CLOCHE S’EST TUE" : "LES FOSSES VOUS ONT GARDÉ", $"{(won ? "Victoire" : "Mort")}   •   {g.Elapsed / 60:0.0} minutes   •   {g.Cleared}/{g.CombatTotal} salles nettoyées\nReliques : {(acquired.Length == 0 ? "aucune" : acquired)}");
            Button("Nouvelle descente", 480, g.Restart); Button("Quitter", 580, Quit);
        }
    }
}
