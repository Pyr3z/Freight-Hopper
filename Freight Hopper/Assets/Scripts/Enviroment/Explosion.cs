using UnityEngine;
using UnityEngine.VFX;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float radius = 1;
    [SerializeField] private float centerForce = 5; // Note, this will be cubed and decrease with radius
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float explosionOffsetTime = 0.6f;
    [SerializeField] private float explosionDuration;
    private VisualEffect explosion;
    Collider[] results = new Collider[]{};

    private void Awake()
    {
        explosion = GetComponent<VisualEffect>();
        Explode();
        explosion.Play();
        Invoke(nameof(Explode), explosionOffsetTime);
        Destroy(this.gameObject, explosionDuration);
    }

    private void Explode()
    {
        Vector3 center = this.transform.position;
        Physics.OverlapSphereNonAlloc(center, radius, results, targetLayer);
        foreach (Collider hitObject in results)
        {
            Rigidbody rb = hitObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(centerForce, center, radius, 3);
            }
        }
    }
}