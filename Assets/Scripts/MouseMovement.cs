using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMovement : MonoBehaviour
{
	[Header("Mouse Look")]
	[SerializeField] private float mouseSensitivity = 2f;
	[SerializeField] private float maxLookAngle = 90f;

	[Header("References")]
	[SerializeField] private Camera playerCamera;
	[SerializeField] private Transform playerBody;
	[SerializeField] private InputActionAsset inputActions;

	private InputActionMap playerActionMap;
	private InputAction lookAction;
	private float xRotation;

	private void Awake()
	{
		if (playerCamera == null)
			playerCamera = GetComponentInChildren<Camera>();

		// If this script is on the camera, yaw should rotate the parent/player body.
		if (playerBody == null)
		{
			if (playerCamera != null && playerCamera.transform == transform && transform.parent != null)
				playerBody = transform.parent;
			else
				playerBody = transform;
		}

		if (inputActions == null)
			inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");

		if (inputActions == null)
		{
			Debug.LogError("InputSystem_Actions asset not found in Resources.", this);
			enabled = false;
			return;
		}

		playerActionMap = inputActions.FindActionMap("Player");
		lookAction = playerActionMap != null ? playerActionMap.FindAction("Look") : null;

		if (playerActionMap == null || lookAction == null)
		{
			Debug.LogError("Could not find Player/Look input actions.", this);
			enabled = false;
			return;
		}

		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnEnable()
	{
		if (playerActionMap != null)
			playerActionMap.Enable();
	}

	private void OnDisable()
	{
		if (playerActionMap != null)
			playerActionMap.Disable();
	}

	private void Update()
	{
		HandleMouseLook();
	}

	private void HandleMouseLook()
	{
		if (playerCamera == null || playerBody == null || lookAction == null)
			return;

		Vector2 lookInput = lookAction.ReadValue<Vector2>();
		Look(lookInput);
	}

	// Simple look method you can call from other scripts
	// (e.g., if you later swap input systems or want AI to aim the player).
	public void Look(Vector2 lookInput)
	{
		if (playerCamera == null || playerBody == null)
			return;

		// Yaw rotates the body/root object.
		playerBody.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.deltaTime);

		// Pitch rotates only the camera.
		xRotation -= lookInput.y * mouseSensitivity * Time.deltaTime;
		xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
		playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
	}

	public void ToggleCursorLock()
	{
		Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
			? CursorLockMode.Confined
			: CursorLockMode.Locked;
	}
}
