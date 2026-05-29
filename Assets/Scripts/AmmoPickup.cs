using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] public int amount = 10;
    [SerializeField] private float pickupScale = 5f;

    private void Start()
    {
        transform.localScale = Vector3.one * pickupScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player") && other.GetComponentInParent<PlayerHealth>() == null)
        {
            return;
        }

        GunFire gunFire = other.GetComponentInParent<GunFire>();
        if (gunFire == null)
        {
            gunFire = FindFirstObjectByType<GunFire>();
        }

        if (gunFire == null)
        {
            return;
        }

        switch (GetActiveWeaponIndex(gunFire))
        {
            case 1:
                gunFire.shotgunAmmoReserve += amount;
                break;
            case 2:
                gunFire.rifleAmmoReserve += amount;
                break;
            default:
                gunFire.pistolAmmoReserve += amount;
                break;
        }

        Destroy(gameObject);
    }

    private int GetActiveWeaponIndex(GunFire gunFire)
    {
        if (gunFire.playerMovement != null)
        {
            return gunFire.playerMovement.currentWeaponIndex;
        }

        return 0;
    }
}
