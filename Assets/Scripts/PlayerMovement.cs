using UnityEngine;

using UnityEngine.InputSystem;

  

public class Movement : MonoBehaviour

{

    [Header("Movement")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Raycast")]

    [SerializeField] private float raycastDistance = 100f;

    [SerializeField] private LayerMask raycastLayer = -1; // Default: all layers

    [SerializeField] private bool drawRaycastDebug = true;

    [Header("References")]

    [SerializeField] private CharacterController characterController;

    [SerializeField] private InputActionAsset inputActions;

    [SerializeField] private Camera playerCamera;

    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private Transform firePoint; // Optional: where bullets spawn from (e.g., gun barrel)

    private InputActionMap playerActionMap;

    private InputAction moveAction;

    private InputAction sprintAction;

    private InputAction jumpAction;

    private InputAction crouchAction;

    private InputAction attackAction;

    private Vector3 velocity;

    private float gravity = -9.81f;

    [SerializeField] private float jumpForce = 5f;

    [SerializeField] private float crouchHeight = 0.6f;

    private float normalHeight = 2f;

    private void Awake()

    {

        // Get character controller if not assigned

        if (characterController == null)

            characterController = GetComponent<CharacterController>();

        // Get camera if not assigned

        if (playerCamera == null)

            playerCamera = GetComponentInChildren<Camera>();

        // Setup input system

        if (inputActions == null)

            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");

        playerActionMap = inputActions.FindActionMap("Player");

        moveAction = playerActionMap.FindAction("Move");

        sprintAction = playerActionMap.FindAction("Sprint");

        jumpAction = playerActionMap.FindAction("Jump");

        crouchAction = playerActionMap.FindAction("Crouch");

        attackAction = playerActionMap.FindAction("Attack");

        // Store normal height

        normalHeight = characterController.height;

    }

    private void OnEnable()

    {

        playerActionMap.Enable();

        // Subscribe to input actions

        jumpAction.performed += OnJump;

        crouchAction.performed += OnCrouch;

        crouchAction.canceled += OnStopCrouch;

        attackAction.performed += OnAttack;

    }

    private void OnDisable()

    {

        playerActionMap.Disable();

        jumpAction.performed -= OnJump;

        crouchAction.performed -= OnCrouch;

        crouchAction.canceled -= OnStopCrouch;

        attackAction.performed -= OnAttack;

    }   
    

    private void Update()

    {
        
        HandleMovement();

        ApplyGravity();

        characterController.Move(velocity * Time.deltaTime);

        PerformRaycast();

    }

    private void HandleMovement()

    {

        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        bool isSprinting = sprintAction.IsPressed();

        // Convert 2D input to 3D movement

        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        // Apply sprint multiplier

        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Update horizontal velocity

        velocity.x = moveDirection.x * currentSpeed;

        velocity.z = moveDirection.z * currentSpeed;

    }

    private void ApplyGravity()

    {

        if (characterController.isGrounded && velocity.y < 0)

        {

            velocity.y = -2f; // Small negative value to keep grounded

        }

        else

        {

            velocity.y += gravity * Time.deltaTime;

        }

    }

    private void OnJump(InputAction.CallbackContext context)

    {

        if (characterController.isGrounded)

        {

            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        }

    }

    private void OnCrouch(InputAction.CallbackContext context)

    {

        characterController.height = crouchHeight;

    }

    private void OnStopCrouch(InputAction.CallbackContext context)

    {

        characterController.height = normalHeight;

    }

    private void OnAttack(InputAction.CallbackContext context)

    {

        PerformRaycast();
        GameObject tempPrefab = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        tempPrefab.GetComponent<Rigidbody>().linearVelocity = firePoint.forward * bulletSpeed; // Example bullet speed
    }

    private void PerformRaycast()

    {

        // Use camera forward for more intuitive aiming

        Vector3 rayOrigin = playerCamera.transform.position;

        Vector3 rayDirection = playerCamera.transform.forward;

        // Draw debug ray (visible in Scene view)

        if (drawRaycastDebug)

            Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red, 0.1f);

        // Perform the raycast

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, raycastDistance, raycastLayer))

        {

            Debug.Log($"Raycast hit: {hit.collider.gameObject.name} at distance {hit.distance:F2}");

            Debug.Log($"Hit point: {hit.point}, Normal: {hit.normal}");

            // Example: Draw a sphere at hit point

            Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.green, 0.1f);

        }

        else

        {

            Debug.Log("Raycast did not hit anything");

        }

    }

    // Optional: Public method to perform raycast from code (not just from input)

    public bool TryRaycast(out RaycastHit hit)

    {

        Vector3 rayOrigin = playerCamera.transform.position;

        Vector3 rayDirection = playerCamera.transform.forward;

        return Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, raycastLayer);

    }

}