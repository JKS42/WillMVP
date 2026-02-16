using UnityEngine;
using UnityEngine.InputSystem;

public class Mousemovement : MonoBehaviour

{

    [Header("Mouse Look")]

    [SerializeField] private float mouseSensitivity = 2f;

    [SerializeField] private float maxLookAngle = 90f;

    [Header("References")]

    [SerializeField] private Camera playerCamera;

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerActionMap;

    private InputAction lookAction;

    private float xRotation = 0f;

    private void Awake()

    {

        // Get camera if not assigned

        if (playerCamera == null)

            playerCamera = GetComponentInChildren<Camera>();

        // Setup input system

        if (inputActions == null)

            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");

        playerActionMap = inputActions.FindActionMap("Player");

        lookAction = playerActionMap.FindAction("Look");

        // Lock and hide cursor

        Cursor.lockState = CursorLockMode.Locked;

    }

    private void OnEnable()

    {

        playerActionMap.Enable();

    }

    private void OnDisable()

    {

        playerActionMap.Disable();

    }

    private void Update()

    {

        HandleMouseLook();

    }

    private void HandleMouseLook()

    {

        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Rotate player body left/right (yaw)

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // Rotate camera up/down (pitch)

        xRotation -= lookInput.y * mouseSensitivity;

        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    }

    // Toggle cursor lock (optional, for UI/menus)

    public void ToggleCursorLock()

    {

        if (Cursor.lockState == CursorLockMode.Locked)

            Cursor.lockState = CursorLockMode.Confined;

        else

            Cursor.lockState = CursorLockMode.Locked;

    }

}
