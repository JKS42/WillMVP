using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public Transform CameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = CameraPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
