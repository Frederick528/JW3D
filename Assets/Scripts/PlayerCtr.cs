using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtr : MonoBehaviour
{
    public GameObject activateArrow;
    public GameObject[] firePos;

    public Slider playerHp;
    public Slider playerStamina;

    public float moveSpeed = 2f;
    public float jumpPower = 4f;
    float addSpeed = 1f;
    bool readyToAttack = false;
    bool isGround;
    bool ableToRun = true;
    bool ableToDive = true;
    //bool ableToAttack = false;
    
    public bool skill1 = false;
    public int arrowIndex = 0;
    
    Animator anim;
    Rigidbody charRigid;

    private Coroutine attackCor;

    public Camera cam;

    public float maxHp = 10;
    float corHp;

    bool lockStamina = false;
    bool exhausted = false;

    float hAxis;
    float vAxis;

    Vector3 _velocity;

    void Start()
    {
        corHp = maxHp;
        anim = GetComponent<Animator>();
        charRigid = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");

        Vector3 _moveHoizontal = transform.right * hAxis;
        Vector3 _moveVertial = transform.forward * vAxis;

        _velocity = (_moveHoizontal + _moveVertial).normalized * moveSpeed * addSpeed;

        charRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    private void Update()
    {
        if (!lockStamina)
        {
            playerStamina.value += 0.003f;
        }
        if (playerStamina.value <= 0.01f)
            exhausted = true;
        else if (exhausted && playerStamina.value > 0.1f)
            exhausted = false;
        if (!exhausted && playerStamina.value > 0.01f && ableToRun == true && vAxis == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            StopCoroutine("StaminaLock");
            readyToAttack = false;
            addSpeed = 4f;
            playerStamina.value -= 0.001f;
            StartCoroutine("StaminaLock");
        }
        else if (readyToAttack)
            addSpeed = 0.75f;
        else
            addSpeed = 1f;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            skill1 = true;
            for (int arrowFirePos = 1; arrowFirePos < firePos.Length; arrowFirePos++)
            {
                firePos[arrowFirePos].SetActive(true);
            }
        }

        if (!exhausted && playerStamina.value > 0.1f && Input.GetKeyDown(KeyCode.LeftControl) && _velocity.magnitude != 0 && ableToDive == true && isGround == true)
        {
            StopCoroutine("StaminaLock");
            ableToDive = false;
            anim.SetTrigger("Dive");
            StartCoroutine(AvoidDive());
            playerStamina.value -= 0.1f;
            StartCoroutine("StaminaLock");
        }
        anim.SetFloat("Horizontal", hAxis);
        anim.SetFloat("Vertical", vAxis);
        anim.SetFloat("Speed", _velocity.magnitude * 5);

        transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0);

        Jump();

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
            if (skill1 == true)
            {
                for (int arrowFirePos = 0; arrowFirePos < firePos.Length; arrowFirePos++)
                {
                    arrowIndex = arrowFirePos;
                    PoolManager.instance.Pool.Get();
                }
            }
            else
            {
                PoolManager.instance.Pool.Get();
            }
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

    IEnumerator AvoidDive()
    {
        gameObject.layer = 8;
        yield return new WaitForSeconds(1f);
        gameObject.layer = 3;
        ableToDive = true;
    }

    IEnumerator StaminaLock()
    {
        lockStamina = true;
        yield return new WaitForSeconds(2f);
        lockStamina = false;
    }
}
