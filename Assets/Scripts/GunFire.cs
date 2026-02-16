using UnityEngine;

public class GunFire : MonoBehaviour
{
    public Camera playerCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = playerCamera.transform.rotation; // Rotate player to match camera direction (optional, for FPS style)
    }
    
}
