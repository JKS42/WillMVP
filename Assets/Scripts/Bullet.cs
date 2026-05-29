using System.Threading;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Timer timer;
    [SerializeField] private int damage = 50;
    void Start()
    {
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        EnemyAI enemy = collision.collider.GetComponentInParent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

}
