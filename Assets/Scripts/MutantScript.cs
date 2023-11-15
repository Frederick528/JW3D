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
    //public Transform mutatntDirection;

    Camera cam; //�� �ᵵ �� ��
    Animator anim;
    NavMeshAgent agent;

    float distance;

    enum State
    {
        Idle,
        Walk,
        Attack,
        ComboAttack,
        JumpAttack
    }
    //���� ó��
    State state;

    bool nowAttack = false;
    Coroutine attackCor = null;

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
        if (player != null)
            distance = Vector3.Distance(transform.position, player.transform.position);
        HpBar();
        if (mutantHp <= 0)
        {
            Destroy(gameObject);
        }
        agent.destination = player.transform.position;

        if (attackCor == null)
        {
            MoveState();
        }
        if (nowAttack == true)
        {
            AttackState();
        }

        //if (state == State.Idle)
        //{
        //    UpdateIdle();
        //}
        //else if (state == State.Walk)
        //{
        //    UpdateWalk();
        //}
        //else if (state == State.Attack)
        //{
        //    UpdateAttack();
        //}
        //else if (state == State.ComboAttack)
        //{
        //    UpdateComboAttack();
        //}
        //else if (state == State.JumpAttack)
        //{
        //    UpdateJumpAttack();
        //}
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
            hpBarObj.transform.position = enemy.transform.position + Vector3.up * 2.7f;
            hpBarObj.transform.LookAt(player);
            //hpBarObj.transform.position = cam.WorldToScreenPoint(enemy.position + new Vector3(0, 3f, 0));
        }
        hpBarSlider.value = mutantHp / mutantMaxHp;
    }

    //private void UpdateAttack()
    //{
    //    anim.ResetTrigger("Attack");    // ���� ��ų�� �߰��� ���� �� ���� ������ �ѹ� ���½������ ��.
    //    AttackState();
    //}

    //private void UpdateComboAttack()
    //{
    //    anim.ResetTrigger("ComboAttack");    // ���� ��ų�� �߰��� ���� �� ���� ������ �ѹ� ���½������ ��.
    //    AttackState();
    //}

    //private void UpdateJumpAttack()
    //{
    //    anim.ResetTrigger("JumpAttack");    // ���� ��ų�� �߰��� ���� �� ���� ������ �ѹ� ���½������ ��.
    //    AttackState();
    //}

    //private void UpdateWalk()
    //{
    //    agent.speed = 1f;
    //    //agent.destination = player.transform.position;
    //    AttackState();
    //}

    //private void UpdateIdle()
    //{
    //    agent.speed = 0;
    //    AttackState();
    //}

    void MoveState()
    {
        if (distance <= 2.7f)
        {
            nowAttack = true;
        }
        else if (distance > 2.7f && distance <= 8)
        {
            state = State.Walk;
            agent.speed = 1f;
            anim.SetTrigger("Walk");
        }
        else if (distance > 8 && distance <= 10)
        {
            nowAttack = true;
        }
        else if (distance > 10)
        {
            state = State.Idle;
            agent.speed = 0;
            anim.SetTrigger("Idle");
        }
    }
    void AttackState()
    {
        nowAttack = false;
        if (distance <= 2.7f)
        {
            int attackState;
            attackState = Random.Range(0, 2);
            switch (attackState)
            {
                case 0:
                    state = State.Attack;
                    anim.SetTrigger("Attack");
                    attackCor = StartCoroutine(Attack());
                    break;
                case 1:
                    state = State.ComboAttack;
                    anim.SetTrigger("ComboAttack");
                    attackCor = StartCoroutine(ComboAttack());
                    break;
            }
        }
        else if (distance > 8 && distance <= 10)
        {
            state = State.JumpAttack;
            anim.SetTrigger("JumpAttack");
            attackCor = StartCoroutine(JumpAttack());
        }
    }

    IEnumerator Attack()
    {
        agent.speed = 0;
        yield return new WaitForSeconds(2.5f);
        attackCor = null;
    }

    IEnumerator ComboAttack()
    {
        agent.speed = 1f;
        yield return new WaitForSeconds(4f);
        attackCor = null;
    }

    IEnumerator JumpAttack()
    {
        agent.speed = 0;
        yield return new WaitForSeconds(0.5f);
        agent.speed = 5;
        yield return new WaitForSeconds(2f);
        agent.speed = 0;
        yield return new WaitForSeconds(0.5f);
        attackCor = null;
    }


    //private void OnTrigg erEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Arrow"))
    //    {
    //        mutantHp -= 1f;
    //    }
    //}
}
