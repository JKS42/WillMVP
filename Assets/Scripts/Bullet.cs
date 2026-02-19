using System.Threading;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Timer timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Invoke("DestroyBullet", 3f); // Destroy the bullet after 3 seconds
    }
    void DestroyBullet()
    {
        Destroy(this.gameObject);
    }
    private void OnCollisionEnter(Collision collision)
{
    EnemyAI enemy = collision.collider.GetComponent<EnemyAI>();
    if (enemy != null)
    {
        enemy.TakeDamage(50);
    }

    Destroy(gameObject);
}

}
