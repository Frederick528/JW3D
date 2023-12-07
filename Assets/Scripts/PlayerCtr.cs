using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerCtr : MonoBehaviour
{
    public GameObject activateArrow;            // 보여주기용 화살(공격 시, 화살통에서 화살이 나오는 것처럼 보이도록)
    public GameObject[] firePos;                // 화살이 나가는 위치

    public Slider playerHp;                     // 플레이어 체력
    public Slider playerStamina;                // 플레이어 스테미나

    public Image aim;                           // 공격 시, 에임이 화면에 나타남.

    float moveSpeed = 2f;                       // 플레이어 이동속도
    float jumpPower = 4f;                       // 플레이어 점프 파워
    float addSpeed = 1f;                        // 플레이어가 달리기 또는 회피를 할 때, 추가적으로 늘어나는 이동속도값
    bool readyToAttack = false;                 // (true일 경우)화살을 날릴 수 있는 상태(=공격 가능 상태)
    bool isGround;                              // (true일 경우)땅 위에 있는 상태
    bool ableToDive = true;                     // (true일 경우)회피가 가능한 상태
    bool ableToArcheryPosture = true;           // (true일 경우)공격 자세를 취할 수 있는 상태(공격 가능 상태와는 다름. 공격 가능 상태는 화살까지 준비되었을 때를 의미함.)

    [HideInInspector]
    public bool skill1 = false;                 // (true일 경우)플레이어 스킬 시전한 상태
    [HideInInspector]
    public int arrowIndex = 0;                  // 화살이 나갈 위치(0: 플레이어, 1,2: 플레이어가 스킬을 쓸 때 나오는 박스)

    public GameObject cameraLook;               // 카메라가 바라보는 위치
    
    Animator anim;
    Rigidbody charRigid;
    CapsuleCollider capsuleCollider;

    public MultiAimConstraint leftArmAim;       // 왼쪽 팔이 바라보는 방향(animation rigging을 통해 각 부위별로 바라보는 방향이 다르도록 조정함)
    public MultiAimConstraint leftBowAim;       // 왼쪽에 있는 활이 바라보는 방향(animation rigging을 통해 각 부위별로 바라보는 방향이 다르도록 조정함)
    public MultiAimConstraint rightArmAim;      // 오른쪽 팔이 바라보는 방향(animation rigging을 통해 각 부위별로 바라보는 방향이 다르도록 조정함)

    public CinemachineFollowZoom zoom;          // 카메라 zoom 기능

    private Coroutine attackCor;                // 공격 코루틴(StopCoroutine을 쓰기 위해 따로 지정함.)

    public Camera cam;

    public float maxHp = 10;                    // 플레이어 최대 체력
    float corHp;                                // 플레이어 현재 체력

    bool lockStamina = false;                   // (true일 경우)스테미나 회복 불가 상태(달리기 또는 회피 사용시 일정 시간동안 회복 불가)
    bool exhausted = false;                     // (true일 경우)탈진에 걸려 스테미나 사용이 불가능함.(스테미나값이 0에 가까워질 경우, 일정 시간동안은 스테미나를 사용하지 못 하고 회복만 가능하도록 설정)

    float hAxis;
    float vAxis;

    Vector3 _velocity;                          // 플레이어의 이동속도

    void Start()
    {
        corHp = maxHp;
        anim = GetComponent<Animator>();
        charRigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()                          // 플레이어 이동
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
        if (collision.transform.CompareTag("Ground"))   // 점프 후 착지 확인
        {

            StartCoroutine(JumpDelay());
            anim.SetBool("Grounded", true);
            anim.SetBool("Jump", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))        // 적 공격 확인
        {
            corHp -= 1;
        }
    }

    /// <summary>
    /// 우클릭다운: 활을 들면서, 화살을 장전함.
    /// <para>
    /// 우클릭업: 활을 내림.
    /// </para>
    /// 좌클릭다운: 공격 가능 상태일 경우, 화살을 날림(PoolManager 이용)
    /// </summary>
    void Attack()
    {
        if (Input.GetMouseButtonDown(1) && ableToArcheryPosture && isGround)
        {
            anim.SetTrigger("CanAttack");   // 애니메이션 레이어 변경
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
    /// 점프키를 누를 경우, 점프 진행
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
    /// 공격 자세를 취하고 있을 경우(활을 들 경우),
    /// <para>
    /// 줌인, 에임 활성화, 시점 변경, 왼쪽팔 애니메이션 리깅 weight를 1로 변경, 상체 공격 애니메이션 레이어 weight를 1로 변경
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
    /// 공격 자세를 취한 상태에서 달리기, 회피, 점프 등을 실행할 경우,
    /// <para>
    /// 줌아웃, 에임 비활성화, 시점 변경, 보여주기용 화살 비활성화, 공격 가능 상태 비활성화, 양팔 애니메이션 리깅 weight를 0으로 변경, 상체 공격 애니메이션 레이어 weight를 0으로 변경
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
    /// Q를 누를 경우, 스킬1 실행
    /// <para>
    /// 플레이어 양옆에 화살을 발사하는 박스 소환
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
    /// 플레이어 스테미나 관리
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
    /// 달리기(LeftShift) 및 이동속도 관리
    /// </summary>
    void RunningAndMovementSpeed()
    {
        //if (exhausted)          // 탈진 애니메이션 추가해야 함.
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
    /// 플레이어 회피(LeftControl)
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
    /// 플레이어의 이동, 속도, 방향, 속도 등 관리
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
    /// 땅에 착지 후, 바로 점프하는 것을 막는 코루틴
    /// </summary>
    /// <returns>0.2초 후에 isGround가 켜짐.</returns>
    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.2f);
        isGround = true;
    }

    /// <summary>
    /// 공격 후, 팔의 위치, 애니메이션 리깅 weight값, 공격 가능 상태 등을 조정함.
    /// </summary>
    /// <param name="activate">보여주기용 화살이 다시 활성화 되는데 걸리는 시간(화살 당기는 데 걸리는 0.2초 고려할 것)</param>
    /// <returns>(0.6초 + 파라미터값) 후에 다시 공격 가능</returns>
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
    /// 회피 사용 시, 플레이어의 레이어를 변경하여 플레이어가 적의 공격에 맞지 않도록 변경
    /// </summary>
    /// <returns>1.2초 후에 다시 회피 사용 가능</returns>
    IEnumerator AvoidDive()
    {
        gameObject.layer = 8;
        yield return new WaitForSeconds(1.2f);
        gameObject.layer = 3;
        ableToDive = true;
    }

    /// <summary>
    /// 스테미나를 회복하지 못 하도록 막음
    /// </summary>
    /// <returns>2초 후에 다시 회복 가능</returns>
    IEnumerator StaminaLock()
    {
        lockStamina = true;
        yield return new WaitForSeconds(2f);
        lockStamina = false;
    }

    /// <summary>
    /// 공격 자세를 바로 취하는 것을 막는 코루틴
    /// </summary>
    /// <param name="timing">공격 자세를 다시 취할 수 있는 데 걸리는 시간값</param>
    /// <returns>파라미터 값 후에 다시 공격 자세 가능</returns>
    IEnumerator ArcheryPosture(float timing)
    {
        ableToArcheryPosture = false;
        yield return new WaitForSeconds(timing);
        ableToArcheryPosture = true;
    }

    /// <summary>
    /// 카메라 줌인
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
    /// 카메라 줌아웃
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
