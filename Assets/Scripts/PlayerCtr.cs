using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerCtr : MonoBehaviour
{
    public GameObject activateArrow;
    public GameObject[] firePos;

    public Slider playerHp;
    public Slider playerStamina;

    float moveSpeed = 2f;
    float jumpPower = 4f;
    float addSpeed = 1f;
    bool readyToAttack = false;
    bool isGround;
    bool ableToDive = true;
    //bool ableToAttack = false;

    [HideInInspector]
    public bool skill1 = false;
    [HideInInspector]
    public int arrowIndex = 0;

    public GameObject cameraLook;
    
    Animator anim;
    Rigidbody charRigid;

    public MultiAimConstraint leftArmAim;
    public MultiAimConstraint rightArmAim;

    public CinemachineFollowZoom zoom;

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
        if (!exhausted && playerStamina.value > 0.01f && vAxis == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            StopCoroutine("StaminaLock");
            anim.SetBool("Bow", false);
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

        if (anim.GetBool("Bow"))
        {
            cameraLook.transform.localPosition = new Vector3(0.6f,1.5f,0);
            zoom.enabled = true;
            leftArmAim.weight = 1;
            anim.SetLayerWeight(1, 1);
        }
        else
        {
            cameraLook.transform.localPosition = new Vector3(1f, 1.5f, 0);
            zoom.enabled= false;
            leftArmAim.weight = 0;
            rightArmAim.weight = 0;
            activateArrow.SetActive(false);
            readyToAttack = false;
            anim.SetLayerWeight(1, 0);
        }

        if (!exhausted && playerStamina.value > 0.1f && Input.GetKeyDown(KeyCode.LeftControl) && _velocity.magnitude != 0 && ableToDive == true && isGround == true)
        {
            StopCoroutine("StaminaLock");
            ableToDive = false;
            anim.SetBool("Bow", false);
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
        yield return new WaitForSeconds(0.2f);
        isGround = true;
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //ableToAttack = true;
            anim.SetTrigger("CanAttack");
            anim.SetBool("Bow", true);
            attackCor = StartCoroutine(AbleToBowAttack(0.15f));
            //readyToAttack = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("Bow", false);
            StopCoroutine(attackCor);
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

            attackCor = StartCoroutine(AbleToBowAttack(0.6f));
        }
    }

    void Jump()
    {
        if (isGround && Input.GetButtonDown("Jump") && ableToDive)
        {
            readyToAttack = false;
            anim.SetBool("Bow", false);
            anim.SetBool("Jump", true);
            anim.SetBool("Grounded", false);
            charRigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
            isGround = false;
        }
    }

    IEnumerator AbleToBowAttack(float activate)
    {
        yield return new WaitForSeconds(0.2f);
        rightArmAim.weight = 0;
        rightArmAim.data.offset = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(activate);
        activateArrow.SetActive(true);
        rightArmAim.weight = 1;
        yield return new WaitForSeconds(0.4f);
        for (int i = 0; i < 20; i++)
        {
            rightArmAim.data.offset = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
        //ableToAttack = true;
        readyToAttack = true;

    }

    IEnumerator AvoidDive()
    {
        gameObject.layer = 8;
        yield return new WaitForSeconds(1.25f);
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
