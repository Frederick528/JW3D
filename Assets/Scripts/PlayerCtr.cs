using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtr : MonoBehaviour
{
    public GameObject activateArrow;
    public Transform[] firePos;

    public Slider playerHp;

    public float moveSpeed = 10f;
    public float jumpPower = 6f;
    float addSpeed = 1f;
    bool readyToAttack = false;
    bool isGround;
    bool ableToRun = true;
    //bool ableToAttack = false;
    Animator anim;
    Rigidbody charRigid;

    private Coroutine attackCor;

    public Camera cam;

    public float maxHp = 10;
    float corHp;

    void Start()
    {
        corHp = maxHp;
        anim = GetComponent<Animator>();
        charRigid = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 _moveHoizontal = transform.right * hAxis;
        Vector3 _moveVertial = transform.forward * vAxis;

        Vector3 _velocity = (_moveHoizontal + _moveVertial).normalized * moveSpeed * addSpeed;
        anim.SetFloat("Horizontal", hAxis);
        anim.SetFloat("Vertical", vAxis);
        anim.SetFloat("Speed", _velocity.magnitude);

        if (ableToRun == true && vAxis == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            readyToAttack = false;
            addSpeed = 4f;
        }
        else if (readyToAttack)
            addSpeed = 0.75f;
        else
            addSpeed = 1f;

        Jump();

        charRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0);

        Attack();

        playerHp.value = corHp / maxHp;

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {

            StartCoroutine(JumpDelay());
            anim.SetBool("Grounded", true);
            anim.SetBool("Jump", false);
        }

        //if (collision.transform.CompareTag("EnemyAttack"))
        //{
        //    print("Sdsdds");
        //    corHp -= 1;
        //}

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            corHp -= 1;
        }
    }

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isGround = true;
        if (Input.GetMouseButton(1))
        {
            attackCor = StartCoroutine(AbleToBowAttack(0.4f, 0.7f));
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //ableToAttack = true;
            anim.SetBool("Bow", true);
            ableToRun = false;
            attackCor = StartCoroutine(AbleToBowAttack(0.4f, 0.7f));
            //readyToAttack = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("Bow", false);
            activateArrow.SetActive(false);
            ableToRun = true;
            StopCoroutine(attackCor);
            readyToAttack = false;
        }
        if (/*ableToAttack*/readyToAttack == true && Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("BowAttack");
            activateArrow.SetActive(false);
            PoolManager.instance.Pool.Get();
            //ableToAttack = false;
            readyToAttack = false;

            attackCor = StartCoroutine(AbleToBowAttack(0.8f, 0.7f));
        }
    }

    void Jump()
    {
        if (isGround && Input.GetButtonDown("Jump"))
        {
            readyToAttack = false;
            activateArrow.SetActive(false);
            anim.SetBool("Jump", true);
            anim.SetBool("Grounded", false);
            charRigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
            isGround = false;
        }
    }
    IEnumerator AbleToBowAttack(float activate, float attack)
    {
        yield return new WaitForSeconds(activate);
        activateArrow.SetActive(true);
        yield return new WaitForSeconds(attack);
        //ableToAttack = true;
        readyToAttack = true;

    }
}
