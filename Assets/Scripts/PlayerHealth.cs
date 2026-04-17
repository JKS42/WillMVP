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
        healthBar.value = health;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("EnemyBullet"))
        {
            health -= 5;
            SetHealth(health);
            if(health <= 0)
            {
                Destroy(this.gameObject);
                UI.SetActive(true);
                reload.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
        }
    }   
}
