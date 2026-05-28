using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
	private int damage;
	private float lifetime;

	public void Initialize(int projectileDamage, float projectileLifetime)
	{
		damage = projectileDamage;
		lifetime = projectileLifetime;
		Destroy(gameObject, lifetime);
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
		if (playerHealth != null)
		{
			playerHealth.TakeDamage(damage);
			Destroy(gameObject);
		}
	}
}