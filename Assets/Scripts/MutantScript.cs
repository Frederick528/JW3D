using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutantScript : MonoBehaviour
{
    public float mutantMaxHp, mutantHp = 5f;

    public Transform enemy;
    public Slider hpbar;


    //�������� �Ծ��� �� ������ ó��
    //public void OnDamage(float damage)
    //{
    //    ////�ǰ� �ִϸ��̼� ���
    //    //enemyAnimator.SetTrigger("Hit");

    //    ////LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
    //    //base.OnDamage(damage);

    //    //ü�� ����
    //    enemyHpBarSlider.value = mutantHp;
    //}

    //�� ��ġ + offset�� HpBarPrefab �����ϱ�

    // Update is called once per frame
    void Update()
    {
        transform.position = enemy.position + new Vector3(0.0001f, 0, 0);
        hpbar.value = mutantHp / mutantMaxHp;
        if (mutantHp <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            mutantHp -= 1f;
        }
    }

    //private void OnTrigg erEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Arrow"))
    //    {
    //        mutantHp -= 1f;
    //    }
    //}
}
