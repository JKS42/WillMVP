using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public GameObject npcPrefab; // Reference to the NPC prefab
    public Transform spawnPoint1; // Reference to the first spawn point
    public Transform spawnPoint2; // Reference to the second spawn point
    public float minSpawnTime = 2f; // Minimum time between spawns
    public float maxSpawnTime = 5f; // Maximum time between spawns
    
    private float spawnTimer = 0f; // Current timer
    private float nextSpawnTime; // Next spawn time (randomized)
    public int spawnCount = 0; // Counter for spawned NPCs
    
    void Start()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Update()
    {
        
    }
    
    public void SpawnTimer()
    {
        spawnTimer += Time.deltaTime; // Add elapsed time
        
        if (spawnTimer >= nextSpawnTime)
        {
            SpawnerNPC();
            spawnTimer = 0f; // Reset timer
            nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime); // Generate new random interval
        }
    }
    
    public void SpawnerNPC()
    {
        if(spawnCount > 0)
        {
            // Randomly choose between the two spawn points
            Transform selectedSpawnPoint = Random.value > 0.5f ? spawnPoint1 : spawnPoint2;
        
            Instantiate(npcPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
            Debug.Log("NPC Spawned at: " + selectedSpawnPoint.name);
            spawnCount--;
        }

        
    }
    void OnTriggerEnter(Collider other)
    {
        SpawnerNPC();
        SpawnTimer();
    }
}
