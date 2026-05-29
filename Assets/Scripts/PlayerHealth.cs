using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public int health = 100;
    public GameObject projectile;
    public GameObject UI;
    public GameObject playerDeath;
    public GameObject reload;
    public Slider healthBar;
    private bool isDead = false;

    public void setMaxHealth(int health)
    {
        healthBar.maxValue = health;
        healthBar.value = health;
    }
    public void SetHealth(int health)
    {
        if (healthBar != null)
        {
            healthBar.value = Mathf.Clamp(health, 0, healthBar.maxValue);
        }
    }

    private void Start()
    {
        setMaxHealth(health);
    }
    private void Update()
    {
        PlayerDeath();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        SetHealth(health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("EnemyBullet"))
        {
            TakeDamage(5);
        }
    } 
    private void PlayerDeath()
    {
        if(health <= 0 && !isDead)
        {
            Die();
        }
        else
        {
            isDead = false;
        }
            
    }  

    private void Die()
    {
        isDead = true;

        if (playerDeath != null)
        {
            playerDeath.SetActive(true);
        }

        if (UI != null)
        {
            UI.SetActive(false);
        }

        if (reload != null)
        {
            reload.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
