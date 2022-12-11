using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_C : MonoBehaviour
{
    public Camera followCamera;

    public float speed;     //  Inspector 창에서 조절할수 있도록 public
    public GameObject[] weapons;

    public GameObject[] grenades;
    public int hasGrenades;
    public int maxHasGrenades;

    public bool[] hasWeapons; //무기를 가지고 있는 지에 대한 bool값

    public int ammo;
    public int coin;
    public int heart;

    public int maxAmmo;
    public int maxCoin;
    public int maxHeart;



    float hAxis;
    float vAxis;


    bool wDown;
    bool jDown;     //  점프는 뛸때와 뛰고 내려왔을때를 위해 2가지 bool 변수 설정
    bool iDown;     // 무기 획득 e키로 설정

    bool fDown;
    bool rDown;

    float fireDelay;
    bool isFireReady = true;
    bool isReload;

    bool isBorder;


    bool isJump;
    bool isDodge;   //  회피
    bool isSwap;    //  무기변경 vool값

    bool sDown1;    //무기 스왑 123 키 설정
    bool sDown2;
    bool sDown3;


    Rigidbody rigid;    // 점프하는데 사용, 위로 힘을 주기 위함(AddForce)
    Vector3 moveVec;    //  움직임을 위한 Vector3 변수
    Vector3 dodgeVec;   //  회피를 위한 Vector3 변수

    Animator anim;

    GameObject nearObject;      // 아이템 Collider에 접근한 오브젝트
    Weapon equipWeapon;     //  무기 장착

    int equipWeaponIndex = -1;



    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();  //애니메이션 변수 = 이 스크립트를 가진 오브젝트중 하위오브젝트의 애니메이터 지칭
        rigid = GetComponent<Rigidbody>();   // 점프하기 위해
    }

    //함수로 정리
    void Update()
    {
        GetInput();

        Move();
        Turn();
        Jump();

        Attack();
        Reload();

        Dodge();

        Swap();

        Interaction();


    }



    // 움직임
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //  X축의 움직임
        vAxis = Input.GetAxisRaw("Vertical");   //  Z축의 움직임

        wDown = Input.GetButton("Walk");    // 버튼 누르는 중에는 Walk한다 - Unity에서 Edit - ProjectSetting - InputManager - 18을 19로 변경 - Walk 이름변경 left shift로
                                            // left shift 누를시 걸음(안누를땐 뜀으로 설정)
        jDown = Input.GetButtonDown("Jump");    // 버튼 눌리는 순간 점프
        iDown = Input.GetButtonDown("Interaction");

        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");

        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;  // 새로운 좌표(new Vector3)중 Y축 제외(0)하고 이동. normalized=대각선으로 가도 속도 동일

        if (isDodge)                     //  만약 isDodge(회피)한다면
        { moveVec = dodgeVec; }         //  움직이는 곳이랑 회피하는 곳이랑 같음 - Dodge 하는 중 다른 방향으로 가지 않기위함

        if (isSwap || !isFireReady || isReload)
        { moveVec = Vector3.zero; }

        if (!isBorder)
        { transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime; }    // ??? 어렵네 이건


        anim.SetBool("isRun", moveVec != Vector3.zero);     //  애니메이션 isRun, 움직임이 0이 아닐때 즉 움직이고 있을때 isRun애니메이션 실행
        anim.SetBool("isWalk", wDown);                      //  애니메이션 isWalk, wDown키 누를때 isWalk애니메이션 실행

    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);     // 움직이려는 곳 (입력된 방향)을 LookAt(바라본다)

        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVec = rayhit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }


    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)      //점프할때 ??? 
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);     //  rigid에 AddForce(힘을 준다) Vector3중에 up(위) 방향으로*15힘을, ForceMode.는 Inpulse즉시

            anim.SetBool("isJump", true);       //?? 점프한다
            anim.SetTrigger("doJump");          //?? 트리거
            isJump = true;                      // ??

        }
    }
    void Attack()
    {
        if (equipWeapon == null)
        { return; }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;

        }

    }

    void Reload()
    {
        if (equipWeapon == null)
        { return; }

        if (equipWeapon.type == Weapon.Type.Melee)
        { return; }

        if (ammo == 0)
        { return; }

        if (rDown && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2.5f);
        }

    }
    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }


    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;

            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);

        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {

        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
        { return; }
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
        { return; }
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
        { return; }


        int weaponIndex = -1;
        if (sDown1) { weaponIndex = 0; }
        if (sDown2) { weaponIndex = 1; }
        if (sDown3) { weaponIndex = 2; }

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null)
            { equipWeapon.gameObject.SetActive(false); }

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.5f);
        }

    }
    void SwapOut()
    {
        isSwap = false;
    }


    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }

        }

    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {

            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void FreezRotation()
    { rigid.angularVelocity = Vector3.zero; }   //angularVelocity 물리회전속도

    void StoptoWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 3, LayerMask.GetMask("Wall"));

    }

    void FixedUpdate()
    { 
        FreezRotation();
        StoptoWall();
    
    }        //플레이어 회전을 FixedUpdate로 고정함


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        { 
            Item item = other.GetComponent<Item>();
            switch(item.type)
            { 
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo>maxAmmo)
                    { ammo = maxAmmo; }
                    break;

                case Item.Type.Coin: 
                    coin += item.value;
                    if (coin > maxCoin)
                    { coin = maxCoin; }
                    break;

                case Item.Type.Heart: 
                    heart += item.value;
                    if (heart > maxHeart)
                    { heart = maxHeart; }
                    break;

                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true); //중요
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                    { hasGrenades = maxHasGrenades; }
                    break;

            }
           
            Destroy(other.gameObject);
        }
    }


    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        { nearObject = other.gameObject; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        { nearObject = null; }
    }


  


}

