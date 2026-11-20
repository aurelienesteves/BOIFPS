using System;
using UnityEngine;

namespace BOIFPS
{
    public enum DungeonWeapon { Cloueur, Diapason, Semeur, Comète, Faucille, Caducée, Sextant, Orgue, Égide, Fuseau, Chapelet, Calice, Pilonnier }
    [Flags] public enum WeaponTrait { Aucun=0, Vif=1, Réverbérant=2, Aimanté=4, Ardent=1<<3, Perforant=1<<4, Allongé=1<<5, Fulgurant=1<<6, Massif=1<<7, Aérien=1<<8, Ancré=1<<9, Intrépide=1<<10, Inaugural=1<<11, Terminal=1<<12, Hâtif=1<<13, Bondissant=1<<14, Siphonnant=1<<15, Impitoyable=1<<16, Chasseur=1<<17, Brutal=1<<18, Lointain=1<<19, Mordant=1<<20, Jumeau=1<<21, Relanceur=1<<22 }

    public static class DungeonArmament
    {
        public static readonly string[] Names={"Cloueur","Diapason","Semeur","Comète en cage","Croissant de suie","Caducée","Sextant des veilleurs","Orgue des côtes","Égide errante","Fuseau du vide","Chapelet brisé","Calice affamé","Pilonnier du glas"};
        public static readonly string[] Descriptions={
            "Tir précis et rapide. Maintenez la pression sur une cible.",
            "À l’impact, un arc touche un second ennemi visible à moins de 5 m.",
            "Plantez une graine dans le décor ou sur une cible. Elle éclate après 0,7 s dans un rayon de 3 m.",
            "Lâche un noyau lent qui accélère en vol.",
            "Décrit un arc horizontal, alternativement à gauche et à droite.",
            "Deux serpents ondulent en sens opposés le long du viseur.",
            "Trois aiguilles convergent à 6 m puis se séparent.",
            "Alternez entre une nappe horizontale et une salve verticale.",
            "Projette un disque qui efface le premier tir hostile rencontré.",
            "Traverse trois cibles et gagne en puissance avec la distance.",
            "La dernière perle libère une couronne de douze tirs autour de vous.",
            "Une mise à mort directe rend 5 PV. Visez les ennemis affaiblis.",
            "Un impact direct ébranle les autres ennemis visibles à 3 m."
        };
        public static readonly WeaponTrait[] Traits={WeaponTrait.Vif,WeaponTrait.Réverbérant,WeaponTrait.Aimanté,WeaponTrait.Ardent,WeaponTrait.Perforant,WeaponTrait.Allongé,WeaponTrait.Fulgurant,WeaponTrait.Massif,WeaponTrait.Aérien,WeaponTrait.Ancré,WeaponTrait.Intrépide,WeaponTrait.Inaugural,WeaponTrait.Terminal,WeaponTrait.Hâtif,WeaponTrait.Bondissant,WeaponTrait.Siphonnant,WeaponTrait.Impitoyable,WeaponTrait.Chasseur,WeaponTrait.Brutal,WeaponTrait.Lointain,WeaponTrait.Mordant,WeaponTrait.Jumeau,WeaponTrait.Relanceur};
        public static readonly string[] TraitDescriptions={
            "Réduit de 20 % le délai entre deux attaques.",
            "Les tirs rebondissent une fois sur le décor avant de poursuivre leur course.",
            "Les tirs dévient vers un ennemi visible devant eux.",
            "Les impacts directs embrasent les cibles.",
            "Traverse une cible de plus.",
            "Portée aller augmentée de 50 %.",
            "Vitesse initiale des projectiles augmentée de 40 %.",
            "Double la largeur des projectiles et leur zone de contact.",
            "Tirer en vol augmente les dégâts de 35 %.",
            "Tirer presque immobile augmente les dégâts de 30 %.",
            "Sous 40 PV, les tirs infligent 50 % de dégâts supplémentaires.",
            "La première cartouche du chargeur inflige 60 % de dégâts supplémentaires.",
            "La dernière cartouche inflige 80 % de dégâts supplémentaires.",
            "Recharge 30 % plus vite.",
            "Réduit de 25 % la récupération du dash.",
            "Une élimination directe rend 2 PV.",
            "Inflige 45 % de dégâts supplémentaires aux ennemis sous la moitié de leurs PV.",
            "Inflige 35 % de dégâts supplémentaires aux cibles volantes ou murales.",
            "À moins de 4 m de trajet, les impacts gagnent 40 % de dégâts.",
            "Après 8 m de trajet, les impacts gagnent 50 % de dégâts.",
            "Sur une cible déjà en feu, inflige 40 % de dégâts supplémentaires.",
            "Ajoute une pointe latérale à 55 % de puissance à chaque attaque.",
            "Chaque élimination directe retranche 0,4 s au délai du dash."
        };

        public readonly struct Ballistics
        {
            public readonly float Interval, Speed, Power;
            public Ballistics(float interval,float speed,float power) { Interval=interval; Speed=speed; Power=power; }
        }
        public static readonly Ballistics[] Profiles={
            new Ballistics(0.16f,65f,1f),
            new Ballistics(0.32f,65f,1f),
            new Ballistics(0.7f,18f,1f),
            new Ballistics(0.48f,8f,1.8f),
            new Ballistics(0.3f,24f,1.3f),
            new Ballistics(0.36f,26f,0.8f),
            new Ballistics(0.42f,42f,0.8f),
            new Ballistics(0.48f,33f,0.7f),
            new Ballistics(0.52f,17f,1.2f),
            new Ballistics(0.55f,52f,1.1f),
            new Ballistics(0.24f,36f,1f),
            new Ballistics(0.43f,31f,1.7f),
            new Ballistics(0.65f,22f,2f)};
        public static Color Hue(DungeonWeapon kind) => Color.HSVToRGB(Mathf.Repeat((int)kind*.137f,1),.65f,.95f);

        public static void Arc(DungeonEnemy source,Vector3 origin,float damage,bool burning)
        {
            DungeonEnemy next=null;
            float distance=5;
            foreach(var enemy in source.Room.Enemies)
            {
                if(enemy==source || enemy.Health<=0) continue;
                float gap=Vector3.Distance(origin,enemy.AimPoint);
                if(gap>=distance || !DungeonRelics.ClearPath(origin,enemy.AimPoint)) continue;
                distance=gap; next=enemy;
            }
            if(next==null) return;
            var line=new GameObject("Arc du diapason").AddComponent<LineRenderer>();
            line.sharedMaterial=DungeonGame.Instance.Emissive(Color.cyan,3);
            line.positionCount=2; line.widthMultiplier=.09f;
            line.SetPosition(0,origin); line.SetPosition(1,next.AimPoint);
            UnityEngine.Object.Destroy(line.gameObject,.15f);
            next.Damage(damage*.8f,burning,Vector3.zero,false);
        }
    }

    public sealed class DungeonSeedCharge : MonoBehaviour
    {
        float damage, age;
        bool burning;
        Transform target;
        Vector3 offset;
        LineRenderer ring;
        public static void Plant(Vector3 point, Transform victim,float power,bool burn)
        {
            var ring=DungeonRelics.Ring("Graine du semeur",new Color(.7f,1,.3f));
            var seed=ring.gameObject.AddComponent<DungeonSeedCharge>();
            seed.transform.position=point; seed.ring=ring; seed.target=victim;
            if(victim!=null) seed.offset=point-victim.position;
            seed.damage=power; seed.burning=burn;
            DungeonRelics.DrawRing(ring,point,.3f);
        }
        void Update()
        {
            if(target!=null) transform.position=target.position+offset;
            age+=Time.deltaTime;
            DungeonRelics.DrawRing(ring,transform.position,Mathf.Lerp(.3f,3,age/.7f));
            if(age<.7f) return;
            foreach(var enemy in DungeonRelics.Targets())
                if(Vector3.Distance(enemy.AimPoint,transform.position)<=3 && DungeonRelics.ClearPath(transform.position,enemy.AimPoint))
                    enemy.Damage(damage,burning,Vector3.zero,false);
            DungeonFeedback.Burst(transform.position,Vector3.up,false,true);
            Destroy(gameObject);
        }
    }
}
