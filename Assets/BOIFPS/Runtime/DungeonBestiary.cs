using System.Collections;
using UnityEngine;

namespace BOIFPS
{
    public sealed partial class DungeonEnemy
    {
        internal const int FirstBestiaryKind = 6;
        enum Rite { Lance, Scissors, Sweep, Echo, Wheel, Spiral, Pincer, March, Trap, Pulse, Rush, Claw, Medic, Sentinel, Retreat }

        sealed class Species
        {
            public readonly string Name;
            public readonly Rite Attack;
            public readonly int Life;
            public readonly float Speed, Range;
            public readonly Color Color;
            public readonly PrimitiveType Crest;
            public readonly int Points;

            public Species(string name, Rite attack, int life, float speed, float range, int tint, PrimitiveType crest, int points)
            {
                Name=name; Attack=attack; Life=life; Speed=speed; Range=range;
                Color=new Color32((byte)(tint>>16),(byte)(tint>>8),(byte)tint,255);
                Crest=crest; Points=points;
            }
        }

        static readonly Species[] Bestiary = {
            new Species("Aiguilleur", Rite.Lance, 36, 5, 17, 0x528bb5, PrimitiveType.Capsule, 1),
            new Species("Cisaille", Rite.Scissors, 36, 5, 13, 0xb89358, PrimitiveType.Cube, 2),
            new Species("Balayeur d’encens", Rite.Sweep, 45, 4, 14, 0xa979c8, PrimitiveType.Capsule, 3),
            new Species("Écholâtre", Rite.Echo, 36, 5, 15, 0x508e82, PrimitiveType.Sphere, 2),
            new Species("Roue votive", Rite.Wheel, 54, 5, 8, 0xc9933b, PrimitiveType.Cylinder, 5),
            new Species("Vrilleur", Rite.Spiral, 45, 4, 11, 0x7959aa, PrimitiveType.Cube, 3),
            new Species("Fossoyeur bifide", Rite.Pincer, 54, 4, 12, 0x96964a, PrimitiveType.Cylinder, 2),
            new Species("Arpenteur funèbre", Rite.March, 54, 4, 14, 0x657f9b, PrimitiveType.Cylinder, 3),
            new Species("Guetteur de traces", Rite.Trap, 36, 6, 14, 0x5bba82, PrimitiveType.Sphere, 3),
            new Species("Bourdon creux", Rite.Pulse, 54, 6, 3, 0xbb743c, PrimitiveType.Cylinder, 1),
            new Species("Bélier de suie", Rite.Rush, 54, 6, 8, 0x946358, PrimitiveType.Capsule, 2),
            new Species("Veuve des cryptes", Rite.Claw, 18, 8, 2.8f, 0xb3496b, PrimitiveType.Capsule, 5),
            new Species("Sutureur", Rite.Medic, 36, 5, 13, 0x81ad92, PrimitiveType.Sphere, 1),
            new Species("Veilleur du seuil", Rite.Sentinel, 45, 4, 15, 0x807694, PrimitiveType.Cube, 1),
            new Species("Fuyard aux braises", Rite.Retreat, 36, 7, 8, 0xc35c43, PrimitiveType.Sphere, 5)
        };
        internal static int BestiaryCount => Bestiary.Length;
        internal static string BestiaryName(int kind) => Bestiary[kind-FirstBestiaryKind].Name;
        bool IsBestiary => Kind>=FirstBestiaryKind;
        Species Profile => Bestiary[Kind-FirstBestiaryKind];

        void AddCrest()
        {
            for(int i=0;i<Profile.Points;i++)
            {
                float angle=Profile.Points==1?0:Mathf.Lerp(-60,60,(float)i/(Profile.Points-1));
                var tilt=Quaternion.Euler(0,0,angle);
                var center=transform.position+Vector3.up*2+tilt*Vector3.up*.55f;
                var piece=DungeonGame.Instance.Cube("Crête",center,new Vector3(.27f,.68f,.32f),Color.Lerp(baseColor,Color.white,.4f),transform,Profile.Crest);
                piece.transform.rotation=tilt;
                piece.GetComponent<Collider>().enabled=false;
                Destroy(piece.GetComponent<Collider>());
            }
        }

        void Bolt(Vector3 axis,float angle,float speed=11)
        {
            DungeonProjectile.Spawn(AimPoint,Quaternion.Euler(0,angle,0)*axis,speed,9,false);
        }
        void Seal(Vector3 point,float radius,float delay)
        {
            DungeonDanger.Spawn(DungeonDanger.GroundBelow(ClampToRoom(point)),radius,delay,12);
        }
        IEnumerator BestiaryAttack(Vector3 target)
        {
            var player=DungeonGame.Instance.Player;
            var aim=(target+Vector3.up-AimPoint).normalized;
            var forward=Vector3.ProjectOnPlane(aim,Vector3.up).normalized;
            var side=Vector3.Cross(Vector3.up,forward);
            var origin=transform.position;
            Tactic=Profile.Name;
            yield return Tell(.6f);
            switch(Profile.Attack)
            {
                case Rite.Lance:
                    Bolt(aim,0,22);
                    break;
                case Rite.Scissors:
                    // Parallel origins create two paths that cross at the locked target.
                    foreach(int sign in new[]{-1,1}) {
                        var muzzle=AimPoint+side*.65f*sign;
                        DungeonProjectile.Spawn(muzzle,(target+Vector3.up-muzzle).normalized,13,9,false);
                    }
                    break;
                case Rite.Sweep:
                    for(int i=-2;i<=2;i++) { Bolt(aim,i*12); yield return new WaitForSeconds(.12f); }
                    break;
                case Rite.Echo:
                    Bolt(aim,0,8);
                    yield return new WaitForSeconds(.48f);
                    Bolt(aim,0,18);
                    break;
                case Rite.Wheel:
                    for(int i=0;i<8;i++) Bolt(forward,i*45,8);
                    break;
                case Rite.Spiral:
                    for(int i=0;i<6;i++) {
                        Bolt(forward,i*30); Bolt(forward,i*30+180);
                        yield return new WaitForSeconds(.17f);
                    }
                    break;
                case Rite.Pincer:
                    Seal(target-side*2,1.15f,.85f); Seal(target+side*2,1.15f,.85f);
                    break;
                case Rite.March:
                    for(int i=1;i<=3;i++) Seal(Vector3.Lerp(origin,target,i/3f),1.35f,.55f+i*.22f);
                    break;
                case Rite.Trap:
                    Seal(target+Vector3.ClampMagnitude(player.PlanarVelocity*.65f,4),1.7f,.9f);
                    break;
                case Rite.Pulse:
                    Seal(origin,3,1);
                    break;
                case Rite.Rush:
                    yield return Charge(.45f,6);
                    break;
                case Rite.Claw:
                    yield return ClawSweep();
                    yield return ClawSweep();
                    break;
                case Rite.Medic:
                    DungeonEnemy patient=null;
                    foreach(var ally in Room.Enemies)
                        if(ally!=this && ally.Health<ally.MaxHealth && (ally.transform.position-origin).sqrMagnitude<36 &&
                           (patient==null || ally.Health/ally.MaxHealth<patient.Health/patient.MaxHealth)) patient=ally;
                    if(patient==null) Bolt(aim,0,9);
                    else {
                        patient.Health=Mathf.Min(patient.MaxHealth,patient.Health+9);
                        patient.flashUntil=Time.time+.35f;
                        patient.skin.sharedMaterial=DungeonGame.Instance.Material(Color.green);
                    }
                    break;
                case Rite.Sentinel:
                    // Punishes holding the same line through the tell.
                    if(Vector3.Distance(player.transform.position,target)<1.4f) Bolt(aim,0,20);
                    break;
                case Rite.Retreat:
                    Seal(origin,1.8f,1.3f);
                    agent.isStopped=false;
                    MoveTo(origin-forward*5);
                    break;
            }
            Attacking=false;
            yield return new WaitForSeconds(1.35f);
        }
    }
}
