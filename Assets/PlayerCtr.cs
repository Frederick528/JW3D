using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtr : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpPower = 6f;
    float addSpeed = 1f;
    float mouseSpeed = 2f;
    bool isGround;
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
        float mXAxis = Input.GetAxis("Mouse X");
        float mYAxis = Input.GetAxis("Mouse Y");

        Vector3 _moveHoizontal = transform.right * hAxis;
        Vector3 _moveVertial = transform.forward * vAxis;

        Vector3 _velocity = (_moveHoizontal + _moveVertial).normalized * moveSpeed * addSpeed;

        anim.SetFloat("Horizontal", hAxis);
        anim.SetFloat("Vertical", vAxis);
        anim.SetFloat("Speed", _velocity.magnitude);


        if (vAxis == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            addSpeed = 4f;
        }
        else
        {
            addSpeed = 1f;
        }

        if (isGround && Input.GetButtonDown("Jump"))
        {
            anim.SetBool("Jump", true);
            anim.SetBool("Grounded", false);
            charRigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
            isGround = false;
        }

        charRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0);

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
    }

    void Attck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool("Bow", true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            anim.SetBool("Bow", false);
        }
    }
}
