using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage;
    float speed = 1000f;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.up * speed);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        
    }
}
