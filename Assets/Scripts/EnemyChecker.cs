using UnityEngine;
using System.Collections.Generic;

public class EnemyChecker : MonoBehaviour
{
    private readonly HashSet<Collider> enemiesInTrigger = new HashSet<Collider>();

    public int EnemyCount => enemiesInTrigger.Count;

    private void Update()
    {
        RemoveDestroyedEnemies();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInTrigger.Add(other))
            {
                Debug.Log($"Enemies in trigger: {EnemyCount}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && enemiesInTrigger.Remove(other))
        {
            Debug.Log($"Enemies in trigger: {EnemyCount}");
        }
    }

    private void RemoveDestroyedEnemies()
    {
        if (enemiesInTrigger.Count == 0)
        {
            return;
        }

        List<Collider> removedEnemies = null;

        foreach (Collider enemy in enemiesInTrigger)
        {
            if (enemy == null)
            {
                removedEnemies ??= new List<Collider>();
                removedEnemies.Add(enemy);
            }
        }

        if (removedEnemies == null)
        {
            return;
        }

        foreach (Collider enemy in removedEnemies)
        {
            enemiesInTrigger.Remove(enemy);
        }

        Debug.Log($"Enemies in trigger: {EnemyCount}");
    }

}
