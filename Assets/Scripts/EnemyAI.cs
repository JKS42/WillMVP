using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;



[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
	[SerializeField] private Transform player;
	[SerializeField] private Transform shootPoint;
	[SerializeField] private Transform[] patrolPoints;
	[SerializeField] private float patrolInterval = 2f;
	[SerializeField] private float sightRange = 15f;
	[SerializeField] private float attackRange = 12f;
	[SerializeField] private int attackDamage = 10;
	[SerializeField] private float attackCooldown = 1f;
	[SerializeField] private float projectileSpeed = 18f;
	[SerializeField] private float projectileLifetime = 4f;
	[SerializeField] private float chaseUpdateInterval = 0.2f;
	[SerializeField] private int health = 100;
    [SerializeField] private float lostSightGrace = 0.2f;

	private NavMeshAgent agent;
	private int currentPatrolPointIndex;
	private bool canSeePlayer;
	private bool isEngaged;
	private float timeSinceLostSight;
	private float nextAttackTime;

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	private void OnValidate()
	{
		if (sightRange < 0f)
		{
			sightRange = 0f;
		}

		if (patrolInterval < 0.1f)
		{
			patrolInterval = 0.1f;
		}

		if (chaseUpdateInterval < 0.05f)
		{
			chaseUpdateInterval = 0.05f;
		}

		if (attackRange < 0f)
		{
			attackRange = 0f;
		}

		if (attackCooldown < 0.05f)
		{
			attackCooldown = 0.05f;
		}

		if (projectileSpeed < 0.1f)
		{
			projectileSpeed = 0.1f;
		}

		if (projectileLifetime < 0.1f)
		{
			projectileLifetime = 0.1f;
		}
	}

	private void Start()
	{
		if (player == null)
		{
			GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
			if (playerObject != null)
			{
				player = playerObject.transform;
			}
		}

		agent.stoppingDistance = attackRange;
		StartCoroutine(PatrolRoutine());
	}

	private void Update()
	{
		bool currentlyCanSee = CanSeePlayer();
		canSeePlayer = currentlyCanSee;

		if (currentlyCanSee)
		{
			// We have line of sight — become/ remain engaged and handle combat
			isEngaged = true;
			timeSinceLostSight = 0f;
			HandleRangedCombat();
		}
		else if (isEngaged)
		{
			// Lost sight briefly — allow a small grace window before disengaging.
			timeSinceLostSight += Time.deltaTime;
			if (timeSinceLostSight >= lostSightGrace)
			{
				isEngaged = false;
				agent.isStopped = false;
			}
			else
			{
				// Still within grace window: keep attempting ranged combat (hold position/attack)
				HandleRangedCombat();
			}
		}
		else if (agent.isOnNavMesh)
		{
			agent.isStopped = false;
		}
	}

	public bool CanSeePlayer()
	{
		if (player == null)
		{
			return false;
		}

		Vector3 origin = transform.position + Vector3.up;
		Vector3 target = player.position + Vector3.up;
		Vector3 direction = target - origin;

		if (direction.sqrMagnitude > sightRange * sightRange)
		{
			return false;
		}

		if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, sightRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			return hit.transform == player || hit.transform.IsChildOf(player);
		}

		return false;
	}

	public void ChasePlayer()
	{
		if (player == null || !agent.isOnNavMesh)
		{
			return;
		}

		agent.SetDestination(player.position);
	}

	public void AttackPlayer()
	{
		ShootPlayer();
	}

	public void ShootPlayer()
	{
		if (player == null || Time.time < nextAttackTime)
		{
			return;
		}

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		if (distanceToPlayer > attackRange)
		{
			return;
		}

		GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		projectile.name = "EnemyProjectile";
		projectile.transform.position = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.5f + transform.forward * 0.8f;
		projectile.transform.localScale = Vector3.one * 0.2f;

		Collider projectileCollider = projectile.GetComponent<Collider>();
		projectileCollider.isTrigger = true;

		Rigidbody projectileRigidbody = projectile.AddComponent<Rigidbody>();
		projectileRigidbody.useGravity = false;
		projectileRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
		enemyProjectile.Initialize(attackDamage, projectileLifetime);

		Vector3 direction = (player.position + Vector3.up) - projectile.transform.position;
		projectileRigidbody.linearVelocity = direction.normalized * projectileSpeed;

		foreach (Collider enemyCollider in GetComponentsInChildren<Collider>())
		{
			Physics.IgnoreCollision(projectileCollider, enemyCollider);
		}

		nextAttackTime = Time.time + attackCooldown;
	}

	private void HandleRangedCombat()
	{
		if (player == null || !agent.isOnNavMesh)
		{
			return;
		}

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		if (distanceToPlayer > attackRange)
		{
			agent.isStopped = false;
			ChasePlayer();
		}
		else
		{
			agent.isStopped = true;
			AttackPlayer();
		}
	}

	public void TakeDamage(int damage)
	{
		health -= damage;
		if (health <= 0)
		{
			Destroy(gameObject);
		}
	}

	public void Patrol()
	{
		if (isEngaged)
		{
			ChasePlayer();
			return;
		}

		if (!agent.isOnNavMesh || patrolPoints == null || patrolPoints.Length == 0)
		{
			return;
		}

		if (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance))
		{
			return;
		}

		Transform targetPoint = patrolPoints[currentPatrolPointIndex];
		if (targetPoint == null)
		{
			currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
			return;
		}

		if (NavMesh.SamplePosition(targetPoint.position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
		{
			agent.SetDestination(hit.position);
		}
		else
		{
			agent.SetDestination(targetPoint.position);
		}

		currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
	}

	private IEnumerator PatrolRoutine()
	{
		while (true)
		{
			if (!canSeePlayer && agent.isOnNavMesh && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
			{
				Patrol();
			}

			yield return new WaitForSeconds(patrolInterval);
		}
	}

}
