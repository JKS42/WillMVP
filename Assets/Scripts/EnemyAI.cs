using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerHealth playerHealth;
    public List<Transform> waypoints = new List<Transform>();

    [Header("Enemy")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public int currentAmmo = 15;
    public int maxAmmo = 15;

    [Header("Decision Ranges")]
    public float detectionRange = 10f;           
    public float attackRange = 2.2f;             
    public float lowHealthThreshold = 25f;       

    [Header("Movement")]
    public float patrolSpeed = 2f;               
    public float chaseSpeed = 3.5f;              
    public float fleeSpeed = 4f;                 
    public float patrolPointStopDistance = 0.2f;

    [Header("Debug Controls")]
    public KeyCode damageEnemyKey = KeyCode.H;   
    public KeyCode healEnemyKey = KeyCode.J;     
    public KeyCode addAmmoKey = KeyCode.R;       
    public KeyCode emptyAmmoKey = KeyCode.T;     
    public float debugDamageAmount = 20f;        
    public float debugHealAmount = 20f;     

    [Header("State Debug")]
    public string currentDecision;               
    public Color gizmoColor = Color.white;   

    [Header("Combat")]
    public float attackCooldown = 1f;            
    public float reloadTime = 2f;                
    public int attackDamage = 10;
    private DecisionNode rootNode;               
    private int patrolIndex;                     
    private float attackTimer;                   
    private float reloadTimer;                   
    private bool isReloading;                    

    private void Awake()
    {
        currentHealth = maxHealth;

        if (player != null && playerHealth == null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Initialize the decision tree.
        BuildDecisionTree();
    }

    private void Update()
    {
        HandleDebugInput();

        if (player == null)
            return;

        attackTimer -= Time.deltaTime;

        // Evaluate the current behavior.
        string decision = EvaluateTree(rootNode);
        currentDecision = decision;

        // Execute the chosen action.
        switch (decision)
        {
            case "Patrol":
                gizmoColor = Color.green;
                Patrol();
                break;

            case "Chase":
                gizmoColor = Color.yellow;
                Chase();
                break;

            case "Attack":
                gizmoColor = Color.red;
                Attack();
                break;

            case "Reload":
                gizmoColor = Color.cyan;
                Reload();
                break;

            case "Flee":
                gizmoColor = Color.magenta;
                Flee();
                break;
        }
    }

    
    // Builds the decision tree for enemy behavior.
    private void BuildDecisionTree()
    {
        ActionNode patrolNode = new ActionNode("Patrol");
        ActionNode chaseNode = new ActionNode("Chase");
        ActionNode attackNode = new ActionNode("Attack");
        ActionNode reloadNode = new ActionNode("Reload");
        ActionNode fleeNode = new ActionNode("Flee");

        QuestionNode playerNearbyNode = new QuestionNode(
            "Is player nearby?",
            () => DistanceToPlayer() <= detectionRange
        );

        QuestionNode lowHealthNode = new QuestionNode(
            "Is health low?",
            () => currentHealth <= lowHealthThreshold
        );

        QuestionNode playerInAttackRangeNode = new QuestionNode(
            "Is player in attack range?",
            () => DistanceToPlayer() <= attackRange
        );

        QuestionNode hasAmmoNode = new QuestionNode(
            "Has ammo?",
            () => currentAmmo > 0
        );

        playerNearbyNode.falseNode = patrolNode;
        playerNearbyNode.trueNode = lowHealthNode;

        lowHealthNode.trueNode = fleeNode;
        lowHealthNode.falseNode = playerInAttackRangeNode;

        playerInAttackRangeNode.falseNode = chaseNode;
        playerInAttackRangeNode.trueNode = hasAmmoNode;

        hasAmmoNode.trueNode = attackNode;
        hasAmmoNode.falseNode = reloadNode;

        rootNode = playerNearbyNode;
    }

    // Walks the decision tree to choose the next action.
    private string EvaluateTree(DecisionNode currentNode)
    {
        while (currentNode != null)
        {
            if (currentNode is ActionNode actionNode)
                return actionNode.actionName;

            if (currentNode is QuestionNode questionNode)
            {
                bool result = questionNode.condition.Invoke();
                currentNode = result ? questionNode.trueNode : questionNode.falseNode;
            }
        }

        return "None";
    }

    // Patrols between assigned waypoints.
    private void Patrol()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;

        Transform target = waypoints[patrolIndex];
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= patrolPointStopDistance)
        {
            patrolIndex = (patrolIndex + 1) % waypoints.Count;
            target = waypoints[patrolIndex];
            targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        }

        MoveTowards(targetPosition, patrolSpeed);
    }

    // Chases the player.
    private void Chase()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    // Faces the player and attacks when the cooldown is ready.
    private void Attack()
    {
        FaceTarget(player.position);

        if (attackTimer > 0f)
            return;

        if (currentAmmo <= 0)
            return;

        currentAmmo--;
        attackTimer = attackCooldown;

        Debug.Log($"Enemy ATTACKS. Ammo left: {currentAmmo}");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // Reloads the enemy weapon over time.
    private void Reload()
    {
        FaceTarget(player.position);

        if (!isReloading)
        {
            isReloading = true;
            reloadTimer = reloadTime;
            Debug.Log("Enemy started RELOADING.");
        }

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log("Enemy finished RELOADING.");
        }
    }

    // Flees from the player when health is low.
    private void Flee()
    {
        isReloading = false;

        Vector3 awayDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + awayDirection * 3f;

        MoveTowards(fleeTarget, fleeSpeed);
    }

    // Moves the enemy smoothly toward a target position.
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }

        transform.position = nextPosition;
    }

    // Rotates the enemy to face a target without moving.
    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 flatTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 direction = (flatTarget - transform.position).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction;
        }
    }

    // Returns the flat distance between the enemy and the player.
    private float DistanceToPlayer()
    {
        if (player == null)
            return Mathf.Infinity;

        Vector3 a = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 b = new Vector3(player.position.x, 0f, player.position.z);

        return Vector3.Distance(a, b);
    }

    // Handles debug controls for testing the enemy behavior.
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(damageEnemyKey))
        {
            currentHealth -= debugDamageAmount;
            currentHealth = Mathf.Max(currentHealth, 0f);
            Debug.Log($"Enemy damaged. HP: {currentHealth}");
        }

        if (Input.GetKeyDown(healEnemyKey))
        {
            currentHealth += debugHealAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            Debug.Log($"Enemy healed. HP: {currentHealth}");
        }

        if (Input.GetKeyDown(addAmmoKey))
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log($"Enemy ammo reset to {currentAmmo}");
        }

        if (Input.GetKeyDown(emptyAmmoKey))
        {
            currentAmmo = 0;
            isReloading = false;
            Debug.Log("Enemy ammo emptied.");
        }
    }

    // Draws debug gizmos for the enemy's detection and attack ranges.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position + Vector3.up * 1.5f, 0.25f);
    }
}


public abstract class DecisionNode
{
}

public class QuestionNode : DecisionNode
{
    public string questionText;
    public Func<bool> condition;
    public DecisionNode trueNode;
    public DecisionNode falseNode;

    public QuestionNode(string questionText, Func<bool> condition)
    {
        this.questionText = questionText;
        this.condition = condition;
    }
}


/// An action node is a final answer in the tree.
public class ActionNode : DecisionNode
{
    public string actionName;

    public ActionNode(string actionName)
    {
        this.actionName = actionName;
    }
}
    

