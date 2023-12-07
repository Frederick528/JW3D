using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerCtr : MonoBehaviour
{
    public GameObject activateArrow;            // �����ֱ�� ȭ��(���� ��, ȭ���뿡�� ȭ���� ������ ��ó�� ���̵���)
    public GameObject[] firePos;                // ȭ���� ������ ��ġ

    public Slider playerHp;                     // �÷��̾� ü��
    public Slider playerStamina;                // �÷��̾� ���׹̳�

    public Image aim;                           // ���� ��, ������ ȭ�鿡 ��Ÿ��.

    float moveSpeed = 2f;                       // �÷��̾� �̵��ӵ�
    float jumpPower = 4f;                       // �÷��̾� ���� �Ŀ�
    float addSpeed = 1f;                        // �÷��̾ �޸��� �Ǵ� ȸ�Ǹ� �� ��, �߰������� �þ�� �̵��ӵ���
    bool readyToAttack = false;                 // (true�� ���)ȭ���� ���� �� �ִ� ����(=���� ���� ����)
    bool isGround;                              // (true�� ���)�� ���� �ִ� ����
    bool ableToDive = true;                     // (true�� ���)ȸ�ǰ� ������ ����
    bool ableToArcheryPosture = true;           // (true�� ���)���� �ڼ��� ���� �� �ִ� ����(���� ���� ���¿ʹ� �ٸ�. ���� ���� ���´� ȭ����� �غ�Ǿ��� ���� �ǹ���.)

    [HideInInspector]
    public bool skill1 = false;                 // (true�� ���)�÷��̾� ��ų ������ ����
    [HideInInspector]
    public int arrowIndex = 0;                  // ȭ���� ���� ��ġ(0: �÷��̾�, 1,2: �÷��̾ ��ų�� �� �� ������ �ڽ�)

    public GameObject cameraLook;               // ī�޶� �ٶ󺸴� ��ġ
    
    Animator anim;
    Rigidbody charRigid;
    CapsuleCollider capsuleCollider;

    public MultiAimConstraint leftArmAim;       // ���� ���� �ٶ󺸴� ����(animation rigging�� ���� �� �������� �ٶ󺸴� ������ �ٸ����� ������)
    public MultiAimConstraint leftBowAim;       // ���ʿ� �ִ� Ȱ�� �ٶ󺸴� ����(animation rigging�� ���� �� �������� �ٶ󺸴� ������ �ٸ����� ������)
    public MultiAimConstraint rightArmAim;      // ������ ���� �ٶ󺸴� ����(animation rigging�� ���� �� �������� �ٶ󺸴� ������ �ٸ����� ������)

    public CinemachineFollowZoom zoom;          // ī�޶� zoom ���

    private Coroutine attackCor;                // ���� �ڷ�ƾ(StopCoroutine�� ���� ���� ���� ������.)

    public Camera cam;

    public float maxHp = 10;                    // �÷��̾� �ִ� ü��
    float corHp;                                // �÷��̾� ���� ü��

    bool lockStamina = false;                   // (true�� ���)���׹̳� ȸ�� �Ұ� ����(�޸��� �Ǵ� ȸ�� ���� ���� �ð����� ȸ�� �Ұ�)
    bool exhausted = false;                     // (true�� ���)Ż���� �ɷ� ���׹̳� ����� �Ұ�����.(���׹̳����� 0�� ������� ���, ���� �ð������� ���׹̳��� ������� �� �ϰ� ȸ���� �����ϵ��� ����)

    float hAxis;
    float vAxis;

    Vector3 _velocity;                          // �÷��̾��� �̵��ӵ�

    void Start()
    {
        corHp = maxHp;
        anim = GetComponent<Animator>();
        charRigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()                          // �÷��̾� �̵�
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
        StaminaManagement();

        RunningAndMovementSpeed();

        Skill1();

        Avoid();

        PlayerContent();

        Jump();

        Attack();

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Ground"))   // ���� �� ���� Ȯ��
        {

            StartCoroutine(JumpDelay());
            anim.SetBool("Grounded", true);
            anim.SetBool("Jump", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))        // �� ���� Ȯ��
        {
            corHp -= 1;
        }
    }

    /// <summary>
    /// ��Ŭ���ٿ�: Ȱ�� ��鼭, ȭ���� ������.
    /// <para>
    /// ��Ŭ����: Ȱ�� ����.
    /// </para>
    /// ��Ŭ���ٿ�: ���� ���� ������ ���, ȭ���� ����(PoolManager �̿�)
    /// </summary>
    void Attack()
    {
        if (Input.GetMouseButtonDown(1) && ableToArcheryPosture && isGround)
        {
            anim.SetTrigger("CanAttack");   // �ִϸ��̼� ���̾� ����
            anim.SetBool("Bow", true);
            OnBow();
            attackCor = StartCoroutine(AbleToBowAttack(0.15f));
        }
        if (Input.GetMouseButtonUp(1) && anim.GetBool("Bow"))
        {
            StartCoroutine(ArcheryPosture(0.4f));
            StopCoroutine(attackCor);
            OffBow();
        }
        if (readyToAttack == true && Input.GetMouseButtonDown(0))
        {
            leftBowAim.weight = 0;
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
            readyToAttack = false;

            attackCor = StartCoroutine(AbleToBowAttack(0.6f));
        }
    }

    /// <summary>
    /// ����Ű�� ���� ���, ���� ����
    /// </summary>
    void Jump()
    {
        if (isGround && Input.GetButtonDown("Jump") && ableToDive)
        {
            readyToAttack = false;
            OffBow();
            anim.SetBool("Jump", true);
            anim.SetBool("Grounded", false);
            charRigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
            StartCoroutine(JumpPositionFixing());
            isGround = false;
        }
    }

    /// <summary>
    /// ���� �ڼ��� ���ϰ� ���� ���(Ȱ�� �� ���),
    /// <para>
    /// ����, ���� Ȱ��ȭ, ���� ����, ������ �ִϸ��̼� ���� weight�� 1�� ����, ��ü ���� �ִϸ��̼� ���̾� weight�� 1�� ����
    /// </para>
    /// </summary>
    void OnBow()
    {
        StartCoroutine(ZoomIn());
        aim.enabled = true;
        cameraLook.transform.localPosition = new Vector3(0.4f, 1.5f, 0);
        leftArmAim.weight = 1;
        anim.SetLayerWeight(1, 1);
    }

    /// <summary>
    /// ���� �ڼ��� ���� ���¿��� �޸���, ȸ��, ���� ���� ������ ���,
    /// <para>
    /// �ܾƿ�, ���� ��Ȱ��ȭ, ���� ����, �����ֱ�� ȭ�� ��Ȱ��ȭ, ���� ���� ���� ��Ȱ��ȭ, ���� �ִϸ��̼� ���� weight�� 0���� ����, ��ü ���� �ִϸ��̼� ���̾� weight�� 0���� ����
    /// </para>
    /// </summary>
    void OffBow()
    {
        if (!anim.GetBool("Bow"))
            return;
        anim.SetBool("Bow", false);
        StartCoroutine(ZoomOut());
        aim.enabled = false;
        cameraLook.transform.localPosition = new Vector3(1f, 1.5f, 0);
        activateArrow.SetActive(false);
        readyToAttack = false;
        leftArmAim.weight = 0;
        leftBowAim.weight = 0;
        rightArmAim.weight = 0;
        anim.SetLayerWeight(1, 0);
    }

    /// <summary>
    /// Q�� ���� ���, ��ų1 ����
    /// <para>
    /// �÷��̾� �翷�� ȭ���� �߻��ϴ� �ڽ� ��ȯ
    /// </para>
    /// </summary>
    void Skill1()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            skill1 = true;
            for (int arrowFirePos = 1; arrowFirePos < firePos.Length; arrowFirePos++)
            {
                firePos[arrowFirePos].SetActive(true);
            }
        }
    }

    /// <summary>
    /// �÷��̾� ���׹̳� ����
    /// </summary>
    void StaminaManagement()
    {
        if (!lockStamina)
        {
            playerStamina.value += 0.003f;
        }
        if (playerStamina.value <= 0.01f)
            exhausted = true;
        else if (exhausted && playerStamina.value > 0.1f)
            exhausted = false;
    }

    /// <summary>
    /// �޸���(LeftShift) �� �̵��ӵ� ����
    /// </summary>
    void RunningAndMovementSpeed()
    {
        //if (exhausted)          // Ż�� �ִϸ��̼� �߰��ؾ� ��.
        //    addSpeed = 0.3f;
        if (!exhausted && playerStamina.value > 0.01f && vAxis == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            StopCoroutine("StaminaLock");
            OffBow();
            readyToAttack = false;
            addSpeed = 4f;
            playerStamina.value -= 0.0005f;
            StartCoroutine("StaminaLock");
        }
        else if (!ableToDive)
            addSpeed = 4f;
        else if (readyToAttack)
            addSpeed = 0.75f;
        else
            addSpeed = 1f;
    }

    /// <summary>
    /// �÷��̾� ȸ��(LeftControl)
    /// </summary>
    void Avoid()
    {
        if (!exhausted && playerStamina.value > 0.1f && Input.GetKeyDown(KeyCode.LeftControl) && _velocity.magnitude != 0 && ableToDive == true && isGround == true)
        {
            StartCoroutine(ArcheryPosture(1.25f));
            StopCoroutine("StaminaLock");
            ableToDive = false;
            OffBow();
            anim.SetTrigger("Dive");
            StartCoroutine(AvoidDive());
            playerStamina.value -= 0.1f;
            StartCoroutine("StaminaLock");
        }
    }

    /// <summary>
    /// �÷��̾��� �̵�, �ӵ�, ����, �ӵ� �� ����
    /// </summary>
    void PlayerContent()
    {
        anim.SetFloat("Horizontal", hAxis);
        anim.SetFloat("Vertical", vAxis);
        anim.SetFloat("Speed", _velocity.magnitude * 5);

        transform.rotation = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0);

        playerHp.value = corHp / maxHp;
    }



    IEnumerator JumpPositionFixing()
    {
        for (int i = 0; i < 7; i++)
        {
            capsuleCollider.center = new Vector3(0, capsuleCollider.center.y + 0.1f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 7; i++)
        {
            capsuleCollider.center = new Vector3(0, capsuleCollider.center.y - 0.1f, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// ���� ���� ��, �ٷ� �����ϴ� ���� ���� �ڷ�ƾ
    /// </summary>
    /// <returns>0.2�� �Ŀ� isGround�� ����.</returns>
    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.2f);
        isGround = true;
    }

    /// <summary>
    /// ���� ��, ���� ��ġ, �ִϸ��̼� ���� weight��, ���� ���� ���� ���� ������.
    /// </summary>
    /// <param name="activate">�����ֱ�� ȭ���� �ٽ� Ȱ��ȭ �Ǵµ� �ɸ��� �ð�(ȭ�� ���� �� �ɸ��� 0.2�� ����� ��)</param>
    /// <returns>(0.6�� + �Ķ���Ͱ�) �Ŀ� �ٽ� ���� ����</returns>
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
            if (i == 10)
                leftBowAim.weight = 1;
            rightArmAim.data.offset = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
        readyToAttack = true;

    }


    /// <summary>
    /// ȸ�� ��� ��, �÷��̾��� ���̾ �����Ͽ� �÷��̾ ���� ���ݿ� ���� �ʵ��� ����
    /// </summary>
    /// <returns>1.2�� �Ŀ� �ٽ� ȸ�� ��� ����</returns>
    IEnumerator AvoidDive()
    {
        gameObject.layer = 8;
        yield return new WaitForSeconds(1.2f);
        gameObject.layer = 3;
        ableToDive = true;
    }

    /// <summary>
    /// ���׹̳��� ȸ������ �� �ϵ��� ����
    /// </summary>
    /// <returns>2�� �Ŀ� �ٽ� ȸ�� ����</returns>
    IEnumerator StaminaLock()
    {
        lockStamina = true;
        yield return new WaitForSeconds(2f);
        lockStamina = false;
    }

    /// <summary>
    /// ���� �ڼ��� �ٷ� ���ϴ� ���� ���� �ڷ�ƾ
    /// </summary>
    /// <param name="timing">���� �ڼ��� �ٽ� ���� �� �ִ� �� �ɸ��� �ð���</param>
    /// <returns>�Ķ���� �� �Ŀ� �ٽ� ���� �ڼ� ����</returns>
    IEnumerator ArcheryPosture(float timing)
    {
        ableToArcheryPosture = false;
        yield return new WaitForSeconds(timing);
        ableToArcheryPosture = true;
    }

    /// <summary>
    /// ī�޶� ����
    /// </summary>
    /// <returns></returns>
    IEnumerator ZoomIn()
    {
        for (int zoomInIdx = 0; zoomInIdx < 40; zoomInIdx++)
        {
            zoom.m_MaxFOV -= 0.5f;
            zoom.m_MinFOV -= 0.5f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    /// <summary>
    /// ī�޶� �ܾƿ�
    /// </summary>
    /// <returns></returns>
    IEnumerator ZoomOut()
    {
        for (int zoomOutIdx = 0; zoomOutIdx < 20; zoomOutIdx++)
        {
            zoom.m_MaxFOV += 1;
            zoom.m_MinFOV += 1;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
