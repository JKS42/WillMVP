using UnityEngine;

public class Break : MonoBehaviour
{
    public Rigidbody rb;
    public bool isBroken;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isBroken)
        {
            rb.isKinematic = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            isBroken = true;
        }
    }
}
