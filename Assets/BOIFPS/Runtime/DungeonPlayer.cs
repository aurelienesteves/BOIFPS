using UnityEngine;
using UnityEngine.InputSystem;

namespace BOIFPS
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DungeonPlayer : MonoBehaviour
    {
        public float Health = 100, DashCooldown;
        public Camera View { get; private set; }
        CharacterController controller;
        float pitch, nextShot, protectedUntil, dashRemaining;
        Vector3 dashDirection;
        Transform weapon;
        float kick;
        float verticalSpeed, groundedUntil, jumpBufferedUntil = -1, recoil, trauma, muzzleUntil;
        bool wasGrounded;
        Transform muzzleFlash;
        Light muzzleLight;
        public bool Grounded => controller.isGrounded;
        public const float JumpSpeed = 9.8f;
        public const float Gravity = 28;
        public const int MagazineCapacity = 6;
        public const float ReloadDuration = 1.15f;
        public int Ammo { get; private set; } = MagazineCapacity;
        public bool Reloading { get; private set; }
        public float ReloadRemaining => Reloading ? Mathf.Max(0, reloadEnds - Time.time) : 0;
        float reloadEnds;
        Vector3 lastSafe;
        bool reloadWasEmpty,mirrorReady;
        float mirrorUntil;
        public bool EmpoweredShot { get; private set; }
        public float ShotInterval => (Has(2)?.55f:.16f)*(Has(0)?.8f:1);
        public Vector3 PlanarVelocity { get; private set; }
        Keyboard boundKeyboard;
        bool boundAzerty;
        UnityEngine.InputSystem.Controls.KeyControl forwardKey, leftKey;
        public bool Has(int item) => DungeonGame.Instance.Upgrades.Contains(item);
        void Awake()
        {
            controller = GetComponent<CharacterController>(); controller.height = 1.8f; controller.radius = .35f; controller.center = Vector3.up * .9f;
            var eye = new GameObject("Eyes"); eye.transform.SetParent(transform, false); eye.transform.localPosition = Vector3.up * 1.6f;
            View = eye.AddComponent<Camera>(); View.nearClipPlane = .05f; View.farClipPlane = 70; eye.tag = "MainCamera"; eye.AddComponent<AudioListener>();
            var g = DungeonGame.Instance;
            lastSafe=transform.position;
            gameObject.AddComponent<DungeonRelics>();
            View.fieldOfView = g.FieldOfView;
            weapon = g.Cube("Le Cloueur", Vector3.zero, new Vector3(.18f, .20f, .65f), new Color(.60f, .52f, .37f), eye.transform).transform;
            Destroy(weapon.GetComponent<Collider>()); weapon.localPosition = new Vector3(.28f, -.27f, .55f); weapon.localRotation = Quaternion.identity;
            var muzzle = g.Cube("Moelle", Vector3.zero, Vector3.one * .13f, Color.cyan, weapon, PrimitiveType.Sphere);
            Destroy(muzzle.GetComponent<Collider>()); muzzle.transform.localPosition = new Vector3(0, 0, .5f);
            muzzleFlash = g.Cube("Éclair de bouche", Vector3.zero, new Vector3(.4f, .4f, .7f), new Color(1,.65f,.2f), eye.transform, PrimitiveType.Sphere).transform;
            var flashCollider = muzzleFlash.GetComponent<Collider>(); flashCollider.enabled = false; Destroy(flashCollider);
            muzzleFlash.localPosition = new Vector3(.28f, -.24f, 1.05f); muzzleFlash.GetComponent<Renderer>().sharedMaterial = g.Emissive(new Color(1,.65f,.2f), 5);
            muzzleLight = muzzleFlash.gameObject.AddComponent<Light>(); muzzleLight.color = new Color(1,.65f,.25f); muzzleLight.range = 7; muzzleLight.intensity = 5;
            muzzleFlash.gameObject.SetActive(false);
        }
        void Update()
        {
            var g = DungeonGame.Instance; var k = Keyboard.current; var m = Mouse.current;
            if (g.Paused || g.Finished || k == null || m == null) return;
            if (Reloading && Time.time >= reloadEnds)
            {
                Reloading = false; Ammo = MagazineCapacity; g.Sound(1.8f);
                if(reloadWasEmpty && Has(11)) DungeonRelics.CreateOrbitals(this);
            }
            if (k.rKey.wasPressedThisFrame) Reload();
            var delta = m.delta.ReadValue() * g.Sensitivity;
            transform.Rotate(0, delta.x, 0); pitch = Mathf.Clamp(pitch - delta.y, -85, 85);
            bool grounded = controller.isGrounded;
            if(transform.position.y < -2.2f) { RecoverFromPit(); return; }
            if (grounded)
            {
                if((g.Current==null || transform.position.y>1.5f || !g.Current.ContainsPit(transform.position,.6f)) && transform.position.y>=0)
                    lastSafe=transform.position;
                groundedUntil = Time.time + .1f;
                if (!wasGrounded && verticalSpeed < -5) { trauma = Mathf.Max(trauma, .045f); g.Sound(.65f); }
                if (verticalSpeed < 0) verticalSpeed = -2;
            }
            wasGrounded = grounded;
            if (k.spaceKey.wasPressedThisFrame) Jump();
            if (jumpBufferedUntil >= Time.time && Time.time <= groundedUntil && verticalSpeed <= 0)
            {
                verticalSpeed = JumpSpeed; jumpBufferedUntil = -1; groundedUntil = -1; g.Sound(.9f);
            }
            if (boundKeyboard != k || boundAzerty != g.Azerty || forwardKey == null)
            {
                boundKeyboard = k; boundAzerty = g.Azerty;
                forwardKey = k.FindKeyOnCurrentKeyboardLayout(g.Azerty ? "z" : "w") ?? k.wKey;
                leftKey = k.FindKeyOnCurrentKeyboardLayout(g.Azerty ? "q" : "a") ?? k.aKey;
            }
            float x = (k.dKey.isPressed ? 1 : 0) - (leftKey.isPressed ? 1 : 0);
            float z = (forwardKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0);
            Vector3 move = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1);
            DashCooldown = Mathf.Max(0, DashCooldown - Time.deltaTime);
            if (k.leftShiftKey.wasPressedThisFrame && DashCooldown <= 0)
            {
                Dash(move);
            }
            if (dashRemaining > 0)
            {
                float step = Mathf.Min(Time.deltaTime, dashRemaining);
                move = dashDirection * (4f / .18f) * (step / Mathf.Max(Time.deltaTime, .00001f)); dashRemaining -= step;
            }
            else move *= 8;
            verticalSpeed -= Gravity * Time.deltaTime;
            Vector3 previousPosition=transform.position;
            var collision = controller.Move((move + Vector3.up * verticalSpeed) * Time.deltaTime);
            PlanarVelocity=Vector3.ProjectOnPlane((transform.position-previousPosition)/Mathf.Max(Time.deltaTime,.00001f),Vector3.up);
            if ((collision & CollisionFlags.Above) != 0 && verticalSpeed > 0) verticalSpeed = 0;
            recoil = Mathf.MoveTowards(recoil, 0, Time.deltaTime * 18);
            trauma = Mathf.MoveTowards(trauma, 0, Time.deltaTime * .8f);
            Vector3 shake = g.CameraMotion ? new Vector3(Mathf.Sin(Time.time * 111), Mathf.Cos(Time.time * 137), 0) * trauma : Vector3.zero;
            View.transform.localPosition = Vector3.up * 1.6f + shake;
            View.transform.localRotation = Quaternion.Euler(pitch - (g.CameraMotion ? recoil : 0), 0, g.CameraMotion ? Mathf.Sin(Time.time * 83) * trauma * 5 : 0);
            View.fieldOfView = Mathf.Lerp(View.fieldOfView, g.FieldOfView + (g.CameraMotion && dashRemaining > 0 ? 6 : 0), Time.deltaTime * 12);
            if (m.leftButton.isPressed && Time.time >= nextShot)
            {
                Shoot();
            }
            muzzleFlash.gameObject.SetActive(Time.time < muzzleUntil);
            kick = Mathf.MoveTowards(kick, 0, Time.deltaTime * 1.3f);
            float reloadPose=Reloading?Mathf.Sin(Mathf.PI*(1-ReloadRemaining/ReloadDuration)):0;
            weapon.localPosition = new Vector3(.28f, -.27f - reloadPose*.22f + (g.CameraMotion ? Mathf.Sin(Time.time * 12) * move.magnitude * .0008f : 0), .55f - kick);
            weapon.localRotation = Quaternion.Euler(-kick * 70+reloadPose*35, 0, kick * 18-reloadPose*40);
        }
        public void Shoot()
        {
            var g = DungeonGame.Instance;
            if (g.Paused || g.Finished || Reloading || Time.time < nextShot) return;
            if (Ammo<=0) { Reload(); return; }
            Ammo--;
            nextShot = Time.time + ShotInterval; kick = .17f; recoil = Mathf.Min(recoil + 1.8f, 3); trauma = Mathf.Max(trauma, .018f); muzzleUntil = Time.time + .045f;
            bool empowered=EmpoweredShot; EmpoweredShot=false;
            if(Has(2))
            {
                for(int i=-2;i<=2;i++) Fire(i*7,14f/18,empowered,i%2==0?0:4);
            }
            else Fire(0,1,empowered);
            if(empowered) DungeonRelics.Notify(10,"Riposte tirée • dégâts ×2,5 + pénétration",transform.position+Vector3.up*.1f);
            g.Gunshot(); g.Hud.Shot();
            
            if (Ammo==0) Reload();
        }
        public void Reload()
        {
            var g=DungeonGame.Instance;
            if(g.Paused || g.Finished || Reloading || Ammo==MagazineCapacity) return;
            if(Has(8)) DungeonRelics.Notify(8,"Champ actif • tirs hostiles à 25 %",transform.position+Vector3.up*.1f);
            reloadWasEmpty=Ammo==0; Reloading=true; reloadEnds=Time.time+ReloadDuration; g.Sound(.75f);
        }
        public bool Dash(Vector3 direction)
        {
            var g=DungeonGame.Instance;
            if(g.Paused || g.Finished || DashCooldown>0) return false;
            direction=Vector3.ProjectOnPlane(direction,Vector3.up);
            dashDirection=direction.sqrMagnitude>.01f?direction.normalized:transform.forward; dashRemaining=.18f;
            DashCooldown=Has(5)?.9f:1.2f; mirrorReady=Has(10) && !EmpoweredShot; mirrorUntil=Time.time+.55f; g.Sound(.6f);
            if(Has(9)) { DungeonRelics.Notify(9,"3 mines semées",transform.position+Vector3.up*.1f); StartCoroutine(DungeonRelics.DashMines(this)); }
            if(ReflectReady) DungeonRelics.Notify(10,"Capture armée • 0,55 s",transform.position+Vector3.up*.1f);
            return true;
        }
        public bool ReflectReady => mirrorReady && Time.time<=mirrorUntil;
        public bool Reflect()
        {
            if(!ReflectReady) return false;
            mirrorReady=false; EmpoweredShot=true; DungeonRelics.Notify(10,"Projectile capturé • prochaine attaque ×2,5",transform.position+Vector3.up); DungeonGame.Instance.Sound(2.4f); return true;
        }
        public void SetCheckpoint(Vector3 point) { lastSafe=point+Vector3.up*.1f; }
        public void RecoverFromPit()
        {
            var g=DungeonGame.Instance;
            dashRemaining=0; Teleport(lastSafe+Vector3.up*.05f); Health=Mathf.Max(0,Health-20);
            protectedUntil=Time.time+.75f; trauma=.1f; g.Hud.Hit(transform.position); g.ImpactSound(false);
            if(Health<=0) g.End(false);
        }
        public void Jump() { jumpBufferedUntil=Time.time+.12f; }
        void Fire(float angle,float coefficient,bool empowered,float elevation=0)
        {
            Vector3 direction=Quaternion.AngleAxis(angle,View.transform.up)*Quaternion.AngleAxis(elevation,View.transform.right)*View.transform.forward;
            bool pendulum=Has(7);
            float damage=(Has(1)?23.4f:18)*coefficient*(empowered?2.5f:1);
            int penetration=pendulum?99:1+(Has(4)?1:0)+(empowered?1:0);
            DungeonProjectile.Spawn(View.transform.position+direction*.5f,direction,pendulum?30:65,damage,true,penetration,Has(3),false,pendulum,false,Has(2)?10:pendulum?11:40,Has(2),empowered);
        }
        public void Damage(float amount, Vector3 source)
        {
            var g = DungeonGame.Instance;
            if (g.Finished || Time.time < protectedUntil) return;
            Health = Mathf.Max(0, Health - amount); protectedUntil = Time.time + .5f; trauma = .075f; g.ImpactSound(false);
            g.Hud.Hit(source); if (Health <= 0) g.End(false);
        }
        public void Heal(float amount) { Health = Mathf.Min(100, Health + amount); }
        public void Teleport(Vector3 position) { controller.enabled = false; transform.position = position; verticalSpeed = 0; dashRemaining=0; PlanarVelocity=Vector3.zero; controller.enabled = true; Physics.SyncTransforms(); }
    }
}
