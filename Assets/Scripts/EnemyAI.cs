using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using System.Collections.Generic;



[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
	[Header("References")]
    public Transform player;                      
    public PlayerHealth playerStats;              
    public List<Transform> patrolPoints = new List<Transform>(); 

    [Header("Enemy Stats")]
    public float maxHealth = 100f;               
    public float currentHealth = 100f;           
    public int ammo = 3;                         
    public int maxAmmo = 3;                      

    [Header("Decision Ranges")]
    public float detectionRange = 10f;           
    public float attackRange = 2.2f;             
    public float lowHealthThreshold = 25f;       

    [Header("Movement")]
    public float patrolSpeed = 2f;               
    public float chaseSpeed = 3.5f;              
    public float fleeSpeed = 4f;                 
    public float patrolPointStopDistance = 0.2f; 

    [Header("Combat")]
    public float attackCooldown = 1f;            
    public float reloadTime = 2f;                
    public int attackDamage = 10;             

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

    private DecisionNode rootNode;               
    private int patrolIndex;                     
    private float attackTimer;                   
    private float reloadTimer;                   
    private bool isReloading;                    

    private void Awake()
    {
        
        currentHealth = maxHealth;

        
        
        if (player != null && playerStats == null)
        {
            playerStats = player.GetComponent<PlayerHealth>();
        }

        // Build the decision tree once when the enemy starts
        BuildDecisionTree();
    }

    private void Update()
    {
        
        HandleDebugInput();

       
        if (player == null)
            return;

        
        attackTimer -= Time.deltaTime;

        // Evaluate the decision tree and store the chosen action
        string decision = EvaluateTree(rootNode);
        currentDecision = decision;

        // Perform the action chosen by the decision tree
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

    
    /// Creates the full decision tree structure.
    private void BuildDecisionTree()
    {
        // Action nodes = final decisions / leaf nodes
        ActionNode patrolNode = new ActionNode("Patrol");
        ActionNode chaseNode = new ActionNode("Chase");
        ActionNode attackNode = new ActionNode("Attack");
        ActionNode reloadNode = new ActionNode("Reload");
        ActionNode fleeNode = new ActionNode("Flee");

        // Question: Is the player close enough to detect?
        QuestionNode playerNearbyNode = new QuestionNode(
            "Is player nearby?",
            () => DistanceToPlayer() <= detectionRange
        );

        // Question: Is enemy health low?
        QuestionNode lowHealthNode = new QuestionNode(
            "Is health low?",
            () => currentHealth <= lowHealthThreshold
        );

        // Question: Is the player close enough to attack?
        QuestionNode playerInAttackRangeNode = new QuestionNode(
            "Is player in attack range?",
            () => DistanceToPlayer() <= attackRange
        );

        // Question: Does the enemy still have ammo?
        QuestionNode hasAmmoNode = new QuestionNode(
            "Has ammo?",
            () => ammo > 0
        );


        // Connect the branches of the tree
        playerNearbyNode.falseNode = patrolNode;
        playerNearbyNode.trueNode = lowHealthNode;

        lowHealthNode.trueNode = fleeNode;
        lowHealthNode.falseNode = playerInAttackRangeNode;

        playerInAttackRangeNode.falseNode = chaseNode;
        playerInAttackRangeNode.trueNode = hasAmmoNode;

        hasAmmoNode.trueNode = attackNode;
        hasAmmoNode.falseNode = reloadNode;

        // Set the first question as the root of the tree
        rootNode = playerNearbyNode;
    }

    /// Starts at the root node and moves through the tree
    /// until an action node is found.
    private string EvaluateTree(DecisionNode currentNode)
    {
        while (currentNode != null)
        {
            // If we reached an action node, return that action
            if (currentNode is ActionNode actionNode)
                return actionNode.actionName;

            // If this is a question node, evaluate its condition
            if (currentNode is QuestionNode questionNode)
            {
                bool result = questionNode.condition.Invoke();

                // Move to the true branch or false branch depending on result
                currentNode = result ? questionNode.trueNode : questionNode.falseNode;
            }
        }

        // Safety fallback
        return "None";
    }

    /// Moves between patrol points in a loop.
    private void Patrol()
    {
        // Stop if there are no patrol points assigned
        if (patrolPoints == null || patrolPoints.Count == 0)
            return;

        // Get the current patrol target
        Transform target = patrolPoints[patrolIndex];

        // Keep target on the same Y level as the enemy
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);

        // Check distance to the patrol point
        float distance = Vector3.Distance(transform.position, targetPosition);

        // If close enough, switch to the next patrol point
        if (distance <= patrolPointStopDistance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
            target = patrolPoints[patrolIndex];
            targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        }

        // Move to the patrol point
        MoveTowards(targetPosition, patrolSpeed);
    }

    /// Moves directly toward the player.
    private void Chase()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    /// Faces the player and attacks if cooldown allows and ammo is available.
    private void Attack()
    {

        FaceTarget(player.position);


        if (attackTimer > 0f)
            return;


        if (ammo <= 0)
            return;


        ammo--;


        attackTimer = attackCooldown;

        Debug.Log($"Enemy ATTACKS. Ammo left: {ammo}");


        if (playerStats != null)
        {
            playerStats.TakeDamage(attackDamage);
        }
    }

    /// Handles reloading over time.

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
            ammo = maxAmmo;
            isReloading = false;
            Debug.Log("Enemy finished RELOADING.");
        }
    }


    /// Moves away from the player when health is low.
    private void Flee()
    {
        isReloading = false;

        Vector3 awayDirection = (transform.position - player.position).normalized;

        Vector3 fleeTarget = transform.position + awayDirection * 3f;

        MoveTowards(fleeTarget, fleeSpeed);
    }

    /// Smoothly moves the enemy toward a target position.
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        // Move step by step toward the target
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Work out the direction the enemy should face
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;

        // Only rotate if the direction is large enough
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }

        // Apply the new position
        transform.position = nextPosition;
    }

    /// Rotates the enemy to face a target without moving.
    private void FaceTarget(Vector3 targetPosition)
    {
        // Flatten the target so the enemy only rotates on the Y axis
        Vector3 flatTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 direction = (flatTarget - transform.position).normalized;

        // Rotate only if the direction is valid
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction;
        }
    }

    /// Returns the flat distance between enemy and player.
    /// Y is ignored so height differences do not matter.
    private float DistanceToPlayer()
    {
        if (player == null)
            return Mathf.Infinity;

        Vector3 a = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 b = new Vector3(player.position.x, 0f, player.position.z);

        return Vector3.Distance(a, b);
    }

    /// Debug controls for testing the tree quickly during play mode.
    private void HandleDebugInput()
    {
        // Damage enemy
        if (Input.GetKeyDown(damageEnemyKey))
        {
            currentHealth -= debugDamageAmount;
            currentHealth = Mathf.Max(currentHealth, 0f);
            Debug.Log($"Enemy damaged. HP: {currentHealth}");
        }

        // Heal enemy
        if (Input.GetKeyDown(healEnemyKey))
        {
            currentHealth += debugHealAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            Debug.Log($"Enemy healed. HP: {currentHealth}");
        }

        // Refill ammo
        if (Input.GetKeyDown(addAmmoKey))
        {
            ammo = maxAmmo;
            isReloading = false;
            Debug.Log($"Enemy ammo reset to {ammo}");
        }

        // Empty ammo
        if (Input.GetKeyDown(emptyAmmoKey))
        {
            ammo = 0;
            isReloading = false;
            Debug.Log("Enemy ammo emptied.");
        }
    }

    /// Draws debug gizmos in the Scene view.
    private void OnDrawGizmos()
    {
        // Blue circle = detection range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Red circle = attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Small sphere above enemy = current state color
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position + Vector3.up * 1.5f, 0.25f);
    }
}

/// Base class for all decision tree nodes.
public abstract class DecisionNode
{
}

/// A question node stores:
/// - a question
/// - a condition to test
/// - a true branch
/// - a false branch
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

