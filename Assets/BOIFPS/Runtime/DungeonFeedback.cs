using UnityEngine;

namespace BOIFPS
{
    // Short, collider-free fragments. A cap bounds the cost of automatic fire.
    public sealed class DungeonFeedback : MonoBehaviour
    {
        static int active;
        Vector3 velocity;
        float life, duration;
        Vector3 size;
        public static void Burst(Vector3 point, Vector3 normal, bool flesh, bool death = false)
        {
            int count = death ? 12 : flesh ? 5 : 3;
            for (int i = 0; i < count && active < 100; i++)
            {
                Color color = flesh ? new Color(.65f, .13f, .07f) : new Color(1, .65f, .2f);
                var go = DungeonGame.Instance.Cube(death ? "Débris" : "Impact", point, Vector3.one * (death ? .2f : .065f), color);
                var collider = go.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
                go.GetComponent<Renderer>().sharedMaterial = DungeonGame.Instance.Emissive(color, flesh ? .25f : 3);
                var fragment = go.AddComponent<DungeonFeedback>();
                fragment.velocity = (normal + Random.insideUnitSphere * 1.5f + Vector3.up * .7f) * (death ? 5 : 3);
                fragment.duration = fragment.life = death ? .65f : .24f; fragment.size = go.transform.localScale; active++;
            }
        }
        void Update()
        {
            life -= Time.deltaTime; if (life <= 0) { Destroy(gameObject); return; }
            velocity += Vector3.down * 12 * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(new Vector3(180, 240, 120) * Time.deltaTime);
            transform.localScale = size * Mathf.Clamp01(life / duration);
        }
        void OnDestroy() { active = Mathf.Max(0, active - 1); }
    }
}
