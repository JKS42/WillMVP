using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;



public class EnemyAI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float MaxHealth;
    private bool isDead = false;
    [Header("Refereneces")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform FirePoint;
    [SerializeField] GameObject EnemyBulletPrefab;
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask terrainMask;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 10f;
    private Vector3 currentPartolPoint;
    private bool hasPatrolPoint;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCoolDown;
    [SerializeField] private float forwardShotForce = 10f;
    [SerializeField] private float verticalShotForce = 5f;

    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engangementRange = 10f;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private void Awake()
    {
        if(player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if(playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        if(agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
       DectectPlayer();
       UpdateBehavourState(); 
    }
    private void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engangementRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
    private void DectectPlayer()
    {
        

        
        isPlayerVisible = Physics.CheckSphere(transform.position, visionRange, playerMask);
        isPlayerInRange = Physics.CheckSphere(transform.position, engangementRange, playerMask);
    }
    private void FireProjectile()
    {
        if(EnemyBulletPrefab == null || FirePoint == null)return;
        Rigidbody projectilerb = Instantiate(EnemyBulletPrefab, FirePoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        projectilerb.AddForce(transform.forward * forwardShotForce, ForceMode.Impulse);
        projectilerb.AddForce(transform.up * verticalShotForce, ForceMode.Impulse);

        Destroy(projectilerb.gameObject, 3f);
    }
    private void FindPatrolPoint()
    {
        float randX = UnityEngine.Random.Range(-patrolRange, patrolRange);
        float randz = UnityEngine.Random.Range(-patrolRange, patrolRange);

        Vector3 potentialPoint = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randz);

        if(Physics.Raycast(potentialPoint, -transform.up,2f, terrainMask))
        {
            currentPartolPoint = potentialPoint;
            hasPatrolPoint = true;
        }
        
    }
    private IEnumerator AttackCoolDownRoutine()
    {
        isOnAttackCoolDown  = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCoolDown = false;
    }
    
    private void PerformPatrol()
    {
        if(!hasPatrolPoint)
        FindPatrolPoint();

        if(hasPatrolPoint)
        agent.SetDestination(currentPartolPoint);

        if(Vector3.Distance(transform.position, currentPartolPoint) < 1f)
        hasPatrolPoint = false;
    }
    private void PerformChase()
    {
        if(player != null)
        {
            agent.SetDestination(player.position);
        }
    }

   private void PreformAttack()
{
    agent.SetDestination(transform.position);

    if (player != null)
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    if (!isOnAttackCoolDown)
    {
        FireProjectile();
        StartCoroutine(AttackCoolDownRoutine());
    }
}

    private void UpdateBehavourState()
    {
        if(!isPlayerVisible && !isPlayerInRange)
        {
            PerformPatrol();
        }
        else if(isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if(isPlayerVisible && isPlayerInRange)
        {
            PreformAttack();
        }
    }
    private void EnemyDeath()
    {
        isDead = true;
        Collider col3D = GetComponent<Collider>();
        if(col3D != null) col3D.enabled = false;

        Destroy(this.gameObject);
    }
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            EnemyDeath();
        }
    }
}
