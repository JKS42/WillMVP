using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GunFire : MonoBehaviour
{
    public Camera playerCamera;

    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletLifetime = 3f;
    public int currentPistolAmmo;
    public int currentShotgunAmmo;
    public int currentRifleAmmo;
    public int pistolAmmoCap = 15;
    public int shotgunAmmoCap = 8;
    public int rifleAmmoCap = 30;
    public int pistolAmmoReserve;
    public int shotgunAmmoReserve;
    public int rifleAmmoReserve;
    public TextMeshProUGUI PistolammoText;
    public TextMeshProUGUI ShotgunammoText;
    public TextMeshProUGUI RifleammoText;

    public InputActionReference fireAction;
    public InputActionReference reloadAction;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (currentPistolAmmo <= 0)
        {
            currentPistolAmmo = pistolAmmoCap;
        }

        if (currentShotgunAmmo <= 0)
        {
            currentShotgunAmmo = shotgunAmmoCap;
        }

        if (currentRifleAmmo <= 0)
        {
            currentRifleAmmo = rifleAmmoCap;
        }

        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    void Update()
    {
        if (PistolammoText != null)
        {
            PistolammoText.enabled = true;
        }

        if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
        {
            ReloadPistolAmmo();
            ReloadShotgunAmmo();
            ReloadRifleAmmo();
        }

        if (fireAction != null && fireAction.action.triggered && currentPistolAmmo > 0)
        {
            ShootBullet();
        }

        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    public void ShootBullet()
    {
        if (playerCamera == null)
        {
            return;
        }

        if (currentPistolAmmo <= 0)
        {
            if (fireAction != null)
            {
                fireAction.action.Disable();
            }

            return;
        }

        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "PlayerBullet";
        bullet.transform.position = firePoint != null ? firePoint.position : playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        bullet.transform.localScale = Vector3.one * 0.2f;

        Collider bulletCollider = bullet.GetComponent<Collider>();
        Rigidbody bulletRigidbody = bullet.AddComponent<Rigidbody>();
        bulletRigidbody.useGravity = false;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        bulletRigidbody.linearVelocity = playerCamera.transform.forward * bulletSpeed;

        Bullet bulletBehavior = bullet.AddComponent<Bullet>();
        Destroy(bullet, bulletLifetime);

        foreach (Collider ownCollider in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(bulletCollider, ownCollider);
        }

        currentPistolAmmo--;

        if (currentPistolAmmo <= 0 && fireAction != null)
        {
            fireAction.action.Disable();
        }

        UpdatePistolAmmoText();
    }

    private void ReloadPistolAmmo()
    {
        ReloadAmmo(ref currentPistolAmmo, pistolAmmoCap, ref pistolAmmoReserve);
    }

    private void UpdatePistolAmmoText()
    {
        if (PistolammoText != null)
        {
            PistolammoText.text = $"ammo: {currentPistolAmmo} / {pistolAmmoReserve}";
        }
    }

    private void ReloadShotgunAmmo()
    {
        ReloadAmmo(ref currentShotgunAmmo, shotgunAmmoCap, ref shotgunAmmoReserve);
    }

    private void ReloadRifleAmmo()
    {
        ReloadAmmo(ref currentRifleAmmo, rifleAmmoCap, ref rifleAmmoReserve);
    }

    private void UpdateShotgunAmmoText()
    {
        if (ShotgunammoText != null)
        {
            ShotgunammoText.text = $"ammo: {currentShotgunAmmo} / {shotgunAmmoReserve}";
        }
    }

    private void UpdateRifleAmmoText()
    {
        if (RifleammoText != null)
        {
            RifleammoText.text = $"ammo: {currentRifleAmmo} / {rifleAmmoReserve}";
        }
    }

    private void ReloadAmmo(ref int currentAmmo, int ammoCap, ref int ammoReserve)
    {
        if (currentAmmo >= ammoCap || ammoReserve <= 0)
        {
            return;
        }

        int ammoNeeded = ammoCap - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, ammoReserve);

        currentAmmo += ammoToLoad;
        ammoReserve -= ammoToLoad;

        if (fireAction != null)
        {
            fireAction.action.Enable();
        }
    }
    private void PistolAmmo()
    {
        if (currentPistolAmmo <= 0 && fireAction != null)
        {
            fireAction.action.Disable();
        }
    }
    private void ShotgunAmmo()
    {
        if (currentShotgunAmmo <= 0 && fireAction != null)
        {
            fireAction.action.Disable();
        }
    }
    private void RifleAmmo()
    {
        if (currentRifleAmmo <= 0 && fireAction != null)
        {
            fireAction.action.Disable();
        }
    }
}
