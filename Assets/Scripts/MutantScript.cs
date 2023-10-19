using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantScript : MonoBehaviour
{
    public float mutantHp = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mutantHp <= 0)
        {
            Destroy(gameObject);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Arrow"))
    //    {
    //        mutantHp -= 1f;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Arrow"))
        {
            mutantHp -= 1f;
        }
    }
}
