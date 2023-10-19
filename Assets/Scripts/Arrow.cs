using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage;
    public GameObject arrow;
    float speed = 1000f;
    // StarQuaternion arrowRott is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.up * speed);
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    Destroy(gameObject);

    //    // ������ ȭ�� ����
    //    GameObject newArrow = Instantiate(arrow, transform.position, transform.rotation);
    //    newArrow.transform.SetParent(collision.transform, true);

    //    //10�� �� �Ҹ�
    //    Destroy(newArrow, 10);
    //}

    private void OnTriggerEnter(Collider other)
    {

        // ������ ȭ�� ����
        GameObject newArrow = Instantiate(arrow, transform.position, transform.rotation);
        newArrow.transform.SetParent(other.transform, true);

        Destroy(gameObject);
        //10�� �� �Ҹ�
        Destroy(newArrow, 10);
    }
}
