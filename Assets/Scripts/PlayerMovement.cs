using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;
    public Transform orientation;

    [Header("Input")]
    // Assign an InputAction (Value - Vector2) in the Inspector (InputActionReference)
    public InputActionReference moveAction;

    Vector2 moveInput;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    void Start()
    {
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (moveAction != null)
            moveInput = moveAction.action.ReadValue<Vector2>();

        if (rb == null) return;

        rb.linearDamping = grounded ? groundDrag : 0f;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (rb == null || orientation == null) return;

        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}