using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public int health = 100;
    public GameObject projectile;
    public GameObject UI;
    public GameObject reload;
    public Slider healthBar;
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        SetHealth(health);

        if (health <= 0)
        {
            Destroy(this.gameObject);
            UI.SetActive(true);
            reload.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("EnemyBullet"))
        {
            TakeDamage(5);
        }
    }   
}
