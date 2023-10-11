using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtr : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpPower = 6f;
    float addSpeed = 1f;
    bool readyToAttack;
    bool isGround;
    bool ableToRun = true;
    bool ableToAttack = true;
    Animator anim;
    Rigidbody charRigid;

    public Camera cam;

    void Start()
    {
        anim = GetComponent<Animator>();
        charRigid = GetComponent<Rigidbody>();
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
        else
            addSpeed = 1f;

        Jump();

        charRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        if (!readyToAttack || !isGround)
            transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0);
        else if (readyToAttack)
        {
            transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y + 90, 0);
        }

        Attck();


    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {

            StartCoroutine(JumpDelay());
            anim.SetBool("Grounded", true);
            anim.SetBool("Jump", false);
        }

    }

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isGround = true;
        if (Input.GetMouseButton(1))
        {
            readyToAttack = true;
        }
    }

    void Attck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool("Bow", true);
            ableToRun = false;
            readyToAttack = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("Bow", false);
            ableToRun = true;
            readyToAttack = false;
        }
        if (ableToAttack == true && Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("BowAttack");
            ableToAttack = false;
            StartCoroutine(AbleToBowAttack());
        }
    }

    void Jump()
    {
        if (isGround && Input.GetButtonDown("Jump"))
        {
            readyToAttack = false;
            anim.SetBool("Jump", true);
            anim.SetBool("Grounded", false);
            charRigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
            isGround = false;
        }
    }
    IEnumerator AbleToBowAttack()
    {
        yield return new WaitForSeconds(1.2f);
        ableToAttack = true;

    }
}
