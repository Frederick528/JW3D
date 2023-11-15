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
    //BoxCollider col;

    // StarQuaternion arrowRott is called before the first frame update
    private void OnEnable()
    {
        if (GameManager.instance.playerCtr.skill1 == false)
        {
            transform.position = GameManager.instance.playerCtr.firePos[0].transform.position;
            transform.rotation = GameManager.instance.playerCtr.firePos[0].transform.rotation;
            rigid = GetComponent<Rigidbody>();
            rigid.AddForce(transform.up * speed, ForceMode.Impulse);
        }
        else if (GameManager.instance.playerCtr.skill1 == true)
        {
            for (int index = 0; index < GameManager.instance.playerCtr.firePos.Length; index++)
            {
                transform.position = GameManager.instance.playerCtr.firePos[GameManager.instance.playerCtr.arrowIndex].transform.position;
                transform.rotation = GameManager.instance.playerCtr.firePos[GameManager.instance.playerCtr.arrowIndex].transform.rotation;
                rigid = GetComponent<Rigidbody>();
                rigid.AddForce(transform.up * speed, ForceMode.Impulse);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //col.enabled = false;
        StartCoroutine(ArrowRelease());
    }

    IEnumerator ArrowRelease()
    {
        yield return new WaitForSeconds(10f);
        Pool.Release(this.gameObject);
    }
}
