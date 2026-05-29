using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class GunFire : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject[] weapons; // Array to hold weapon GameObjects
    public GameObject[] weaponUI;

    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private float shotgunSpreadAngle = 18f;
    [SerializeField] private int shotgunPelletCount = 8;
    [SerializeField] private float shotgunMuzzleOffset = 1.0f;
    [SerializeField] private float rifleBulletSpeed = 45f;
    [SerializeField] private float rifleFireInterval = 0.1f;
    [SerializeField] private float bulletScale = 0.1f;
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
    public PlayerMovement playerMovement;
    public GameObject reloadImage;

    private int activeWeaponIndex;
    private float nextRifleFireTime;

    void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable();
        }

        if (reloadAction != null)
        {
            reloadAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.Disable();
        }

        if (reloadAction != null)
        {
            reloadAction.action.Disable();
        }
    }

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
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

        activeWeaponIndex = GetActiveWeaponIndex();
    }

    void Update()
    {
        if (playerMovement != null)
        {
            activeWeaponIndex = playerMovement.currentWeaponIndex;
        }
        else
        {
            activeWeaponIndex = GetActiveWeaponIndex();
        }

        if (PistolammoText != null)
        {
            PistolammoText.enabled = true;
        }

        if (ShotgunammoText != null)
        {
            ShotgunammoText.enabled = true;
        }

        if (RifleammoText != null)
        {
            RifleammoText.enabled = true;
        }

        if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
        {
            ReloadActiveWeaponAmmo(activeWeaponIndex);
            if (reloadImage != null)
            {
                reloadImage.SetActive(true);
               
            }
            
        }

        if (reloadImage != null)
        {
            reloadImage.SetActive(false);
                // Show reload image when current weapon ammo is 0
                if (reloadImage != null)
                {
                    if (GetCurrentAmmo(activeWeaponIndex) <= 0)
                    {
                        reloadImage.SetActive(true);
                    }
                }

                // Hide reload image when reload action is pressed
                if (reloadAction != null && reloadAction.action.WasPressedThisFrame())
                {
                    ReloadActiveWeaponAmmo(activeWeaponIndex);
                    if (reloadImage != null)
                    {
                        reloadImage.SetActive(false);
                    }
                }
        }

        if (activeWeaponIndex == 1)
        {
            if (fireAction != null && fireAction.action.triggered && GetCurrentAmmo(activeWeaponIndex) > 0)
            {
                ShotgunShoot(activeWeaponIndex);
            }
        }
        else if (activeWeaponIndex == 2)
        {
            if (fireAction != null && fireAction.action.IsPressed() && Time.time >= nextRifleFireTime && GetCurrentAmmo(activeWeaponIndex) > 0)
            {
                RifleShoot(activeWeaponIndex);
                nextRifleFireTime = Time.time + rifleFireInterval;
            }
        }
        else if (fireAction != null && fireAction.action.triggered && GetCurrentAmmo(activeWeaponIndex) > 0)
        {
            ShootBullet(activeWeaponIndex);
        }

        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    public void ShootBullet(int weaponIndex)
    {
        if (playerCamera == null)
        {
            return;
        }

        if (GetCurrentAmmo(weaponIndex) <= 0)
        {
            return;
        }

        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "PlayerBullet";
        bullet.transform.position = firePoint != null ? firePoint.position : playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        bullet.transform.localScale = Vector3.one * bulletScale;

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

        SetCurrentAmmo(weaponIndex, GetCurrentAmmo(weaponIndex) - 1);

        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    private void ReloadActiveWeaponAmmo(int weaponIndex)
    {
        ReloadAmmo(weaponIndex);

        if (weaponIndex == 2)
        {
            nextRifleFireTime = Time.time;
        }
    }

    private void UpdatePistolAmmoText()
    {
        if (PistolammoText != null)
        {
            PistolammoText.text = $"ammo: {currentPistolAmmo} / {pistolAmmoReserve}";
        }
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

    private void ReloadAmmo(int weaponIndex)
    {
        int currentAmmo = GetCurrentAmmo(weaponIndex);
        int ammoCap = GetAmmoCap(weaponIndex);
        int ammoReserve = GetAmmoReserve(weaponIndex);

        if (currentAmmo >= ammoCap || ammoReserve <= 0)
        {
            return;
        }

        int ammoNeeded = ammoCap - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, ammoReserve);

        SetCurrentAmmo(weaponIndex, currentAmmo + ammoToLoad);
        SetAmmoReserve(weaponIndex, ammoReserve - ammoToLoad);

        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    private int GetActiveWeaponIndex()
    {
        if (weapons != null)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].activeSelf)
                {
                    return i;
                }
            }
        }

        return 0;
    }

    private int GetCurrentAmmo(int weaponIndex)
    {
        return weaponIndex switch
        {
            1 => currentShotgunAmmo,
            2 => currentRifleAmmo,
            _ => currentPistolAmmo
        };
    }

    private void SetCurrentAmmo(int weaponIndex, int value)
    {
        switch (weaponIndex)
        {
            case 1:
                currentShotgunAmmo = value;
                break;
            case 2:
                currentRifleAmmo = value;
                break;
            default:
                currentPistolAmmo = value;
                break;
        }
    }

    private int GetAmmoCap(int weaponIndex)
    {
        return weaponIndex switch
        {
            1 => shotgunAmmoCap,
            2 => rifleAmmoCap,
            _ => pistolAmmoCap
        };
    }

    private int GetAmmoReserve(int weaponIndex)
    {
        return weaponIndex switch
        {
            1 => shotgunAmmoReserve,
            2 => rifleAmmoReserve,
            _ => pistolAmmoReserve
        };
    }

    private void SetAmmoReserve(int weaponIndex, int value)
    {
        switch (weaponIndex)
        {
            case 1:
                shotgunAmmoReserve = value;
                break;
            case 2:
                rifleAmmoReserve = value;
                break;
            default:
                pistolAmmoReserve = value;
                break;
        }
    }

    private void ShotgunShoot(int weaponIndex)
    {
        if (playerCamera == null)
        {
            return;
        }

        int ammo = GetCurrentAmmo(weaponIndex);
        if (ammo <= 0)
        {
            return;
        }

        Vector3 shotOrigin = firePoint != null
            ? firePoint.position + firePoint.forward * shotgunMuzzleOffset
            : playerCamera.transform.position + playerCamera.transform.forward * 0.7f;

        Vector3 shotForward = firePoint != null ? firePoint.forward : playerCamera.transform.forward;
        int pellets = Mathf.Max(1, shotgunPelletCount);

        for (int pelletIndex = 0; pelletIndex < pellets; pelletIndex++)
        {
            Vector3 spreadDirection = GetShotgunPelletDirection(shotForward);
            SpawnBullet(shotOrigin, spreadDirection, bulletSpeed);
        }

        SetCurrentAmmo(weaponIndex, ammo - 1);
        UpdateShotgunAmmoText();
    }

    private void RifleShoot(int weaponIndex)
    {
        if (playerCamera == null || GetCurrentAmmo(weaponIndex) <= 0)
        {
            return;
        }

        Vector3 origin = firePoint != null ? firePoint.position : playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        SpawnBullet(origin, playerCamera.transform.forward, rifleBulletSpeed);

        SetCurrentAmmo(weaponIndex, GetCurrentAmmo(weaponIndex) - 1);
        UpdatePistolAmmoText();
        UpdateShotgunAmmoText();
        UpdateRifleAmmoText();
    }

    private void SpawnBullet(Vector3 origin, Vector3 direction, float speed)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "PlayerBullet";
        bullet.transform.position = origin;
        bullet.transform.localScale = Vector3.one * bulletScale;

        Collider bulletCollider = bullet.GetComponent<Collider>();
        Rigidbody bulletRigidbody = bullet.AddComponent<Rigidbody>();
        bulletRigidbody.useGravity = false;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        bulletRigidbody.linearVelocity = direction.normalized * speed;

        bullet.AddComponent<Bullet>();
        Destroy(bullet, bulletLifetime);

        foreach (Collider ownCollider in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(bulletCollider, ownCollider);
        }

        if (playerMovement != null)
        {
            foreach (Collider weaponCollider in playerMovement.GetComponentsInChildren<Collider>())
            {
                if (weaponCollider != null)
                {
                    Physics.IgnoreCollision(bulletCollider, weaponCollider);
                }
            }
        }
    }

    private Vector3 GetShotgunPelletDirection(Vector3 forward)
    {
        Vector2 spread = Random.insideUnitCircle * shotgunSpreadAngle;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        if (right == Vector3.zero)
        {
            right = Vector3.right;
        }

        Vector3 up = Vector3.Cross(forward, right).normalized;
        Vector3 pelletDirection = forward + right * spread.x * 0.01f + up * spread.y * 0.01f;
        return pelletDirection.normalized;
    }

    
}
