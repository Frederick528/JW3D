using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutantScript : MonoBehaviour
{
    public float mutantMaxHp, mutantHp = 5f;

    public Transform enemy;
    public Slider hpbar;


    //데미지를 입었을 때 실행할 처리
    //public void OnDamage(float damage)
    //{
    //    ////피격 애니메이션 재생
    //    //enemyAnimator.SetTrigger("Hit");

    //    ////LivingEntity의 OnDamage()를 실행하여 데미지 적용
    //    //base.OnDamage(damage);

    //    //체력 갱신
    //    enemyHpBarSlider.value = mutantHp;
    //}

    //적 위치 + offset에 HpBarPrefab 생성하기

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
