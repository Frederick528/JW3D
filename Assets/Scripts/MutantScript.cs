using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MutantScript : MonoBehaviour
{
    public float mutantMaxHp;
    float mutantHp;

    public Transform player;
    public Transform enemy;
    public GameObject hpBarObj;
    public Slider hpBarSlider;

    Camera cam; //�� �ᵵ �� ��
    Animator anim;
    NavMeshAgent agent;

    float distance;

    enum State
    {
        Idle,
        Walk,
        Attack
    }
    //���� ó��
    State state;

    private void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
        mutantHp = mutantMaxHp;
        agent = GetComponent<NavMeshAgent>();
        state = State.Idle;
    }

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
        agent.destination = player.transform.position;
        distance = Vector3.Distance(transform.position, player.transform.position);
        HpBar();

        if (mutantHp <= 0)
        {
            Destroy(gameObject);
        }

        if (state == State.Idle)
        {
            UpdateIdle();
        }
        else if (state == State.Walk)
        {
            UpdateWalk();
        }
        else if (state == State.Attack)
        {
            UpdateAttack();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            mutantHp -= 1f;
        }
    }

    void HpBar()
    {
        if (distance > 10f)
        {
            hpBarObj.SetActive(false);
        }
        else
        {
            hpBarObj.SetActive(true);
            hpBarObj.transform.position = enemy.transform.position + Vector3.up * 2.8f;
            hpBarObj.transform.LookAt(player);
            //hpBarObj.transform.position = cam.WorldToScreenPoint(enemy.position + new Vector3(0, 3f, 0));
        }
        hpBarSlider.value = mutantHp / mutantMaxHp;
    }


    private void UpdateAttack()
    {
        agent.speed = 0;
        if (distance > 2 && distance <= 10)
        {
            state = State.Walk;
            anim.SetTrigger("Walk");
        }
        else if (distance > 10)
        {
            state = State.Idle;
            anim.SetTrigger("Idle");
        }
    }

    private void UpdateWalk()
    {

        if (distance <= 2)
        {
            state = State.Attack;
            anim.SetTrigger("Attack");
        }
        else if (distance > 10)
        {
            state = State.Idle;
            anim.SetTrigger("Idle");
        }

        agent.speed = 1f;
        //agent.destination = player.transform.position;

    }

    private void UpdateIdle()
    {
        agent.speed = 0;
        if (player != null && distance <= 10)
        {
            state = State.Walk;
            anim.SetTrigger("Walk");
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
