using System;
using UnityEngine;

namespace BOIFPS
{
    public enum DungeonWeapon { Cloueur, Diapason, Semeur }
    [Flags] public enum WeaponTrait { Aucun=0, Vif=1, Réverbérant=2, Aimanté=4 }

    public static class DungeonArmament
    {
        public static readonly string[] Names={"Cloueur","Diapason","Semeur"};
        public static readonly string[] Descriptions={
            "Tir précis et rapide. Maintenez la pression sur une cible.",
            "À l’impact, un arc touche un second ennemi visible à moins de 5 m.",
            "Plantez une graine dans le décor ou sur une cible. Elle éclate après 0,7 s dans un rayon de 3 m."
        };
        public static readonly WeaponTrait[] Traits={WeaponTrait.Vif,WeaponTrait.Réverbérant,WeaponTrait.Aimanté};
        public static readonly string[] TraitDescriptions={
            "Réduit de 20 % le délai entre deux attaques.",
            "Les tirs rebondissent une fois sur le décor avant de poursuivre leur course.",
            "Les tirs dévient vers un ennemi visible devant eux."
        };

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
