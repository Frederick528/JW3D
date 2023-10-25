using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Arrow : MonoBehaviour
{
    public IObjectPool<GameObject> Pool { get; set; }
    public float damage;
    float speed = 20f;
    Rigidbody rigid;

    // StarQuaternion arrowRott is called before the first frame update
    private void OnEnable()
    {
        transform.position = GameManager.instance.playerCtr.firePos[0].position;
        transform.rotation = GameManager.instance.playerCtr.firePos[0].rotation;
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(transform.up * speed, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(ArrowRelease());
    }

    IEnumerator ArrowRelease()
    {
        yield return new WaitForSeconds(10f);
        Pool.Release(this.gameObject);
    }
}
