using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class BulletScript : NetworkBehaviour
{
    

    [SerializeField] private float speed;
    [SerializeField] protected int damage;
    [SerializeField] protected float lifeTime;
    private PlayerRef shooter;

    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        DespawnAfterTime();
    }

    private async void DespawnAfterTime()
    {
        await Task.Delay((int)(lifeTime * 100));
        if(Object != null)
        Runner.Despawn(Object);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Runner.Despawn(Object);

        if(collision.collider.TryGetComponent(out Health health))
        {
            health.Rpc_TakeDamage(damage, shooter);
        }
        else
        {
            Debug.Log("No hay componente de vida :(");
        }
    }
}
