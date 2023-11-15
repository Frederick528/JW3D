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

    Camera cam; //안 써도 될 듯
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
    //상태 처리
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
    //    anim.ResetTrigger("Attack");    // 공격 스킬은 중간에 끊을 수 없기 때문에 한번 리셋시켜줘야 함.
    //    AttackState();
    //}

    //private void UpdateComboAttack()
    //{
    //    anim.ResetTrigger("ComboAttack");    // 공격 스킬은 중간에 끊을 수 없기 때문에 한번 리셋시켜줘야 함.
    //    AttackState();
    //}

    //private void UpdateJumpAttack()
    //{
    //    anim.ResetTrigger("JumpAttack");    // 공격 스킬은 중간에 끊을 수 없기 때문에 한번 리셋시켜줘야 함.
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
