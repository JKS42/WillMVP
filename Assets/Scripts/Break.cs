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
            Invoke("DestroyPieces", 5f); // Destroy the pieces after 5 seconds
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bullet"))
        {
            isBroken = true;
        }
    }
    void DestroyPieces()
    {
        Destroy(this.gameObject);
    }
}
