using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_C : MonoBehaviour
{
    public Camera followCamera;

    public float speed;     //  Inspector â���� �����Ҽ� �ֵ��� public
    public GameObject[] weapons;

    public GameObject[] grenades;
    public int hasGrenades;
    public int maxHasGrenades;

    public bool[] hasWeapons; //���⸦ ������ �ִ� ���� ���� bool��

    public int ammo;
    public int coin;
    public int heart;

    public int maxAmmo;
    public int maxCoin;
    public int maxHeart;



    float hAxis;
    float vAxis;


    bool wDown;
    bool jDown;     //  ������ �۶��� �ٰ� ������������ ���� 2���� bool ���� ����
    bool iDown;     // ���� ȹ�� eŰ�� ����

    bool fDown;
    bool rDown;

    float fireDelay;
    bool isFireReady = true;
    bool isReload;

    bool isBorder;


    bool isJump;
    bool isDodge;   //  ȸ��
    bool isSwap;    //  ���⺯�� vool��

    bool sDown1;    //���� ���� 123 Ű ����
    bool sDown2;
    bool sDown3;


    Rigidbody rigid;    // �����ϴµ� ���, ���� ���� �ֱ� ����(AddForce)
    Vector3 moveVec;    //  �������� ���� Vector3 ����
    Vector3 dodgeVec;   //  ȸ�Ǹ� ���� Vector3 ����

    Animator anim;

    GameObject nearObject;      // ������ Collider�� ������ ������Ʈ
    Weapon equipWeapon;     //  ���� ����

    int equipWeaponIndex = -1;



    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();  //�ִϸ��̼� ���� = �� ��ũ��Ʈ�� ���� ������Ʈ�� ����������Ʈ�� �ִϸ����� ��Ī
        rigid = GetComponent<Rigidbody>();   // �����ϱ� ����
    }

    //�Լ��� ����
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



    // ������
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //  X���� ������
        vAxis = Input.GetAxisRaw("Vertical");   //  Z���� ������

        wDown = Input.GetButton("Walk");    // ��ư ������ �߿��� Walk�Ѵ� - Unity���� Edit - ProjectSetting - InputManager - 18�� 19�� ���� - Walk �̸����� left shift��
                                            // left shift ������ ����(�ȴ����� ������ ����)
        jDown = Input.GetButtonDown("Jump");    // ��ư ������ ���� ����
        iDown = Input.GetButtonDown("Interaction");

        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");

        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;  // ���ο� ��ǥ(new Vector3)�� Y�� ����(0)�ϰ� �̵�. normalized=�밢������ ���� �ӵ� ����

        if (isDodge)                     //  ���� isDodge(ȸ��)�Ѵٸ�
        { moveVec = dodgeVec; }         //  �����̴� ���̶� ȸ���ϴ� ���̶� ���� - Dodge �ϴ� �� �ٸ� �������� ���� �ʱ�����

        if (isSwap || !isFireReady || isReload)
        { moveVec = Vector3.zero; }

        if (!isBorder)
        { transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime; }    // ??? ��Ƴ� �̰�


        anim.SetBool("isRun", moveVec != Vector3.zero);     //  �ִϸ��̼� isRun, �������� 0�� �ƴҶ� �� �����̰� ������ isRun�ִϸ��̼� ����
        anim.SetBool("isWalk", wDown);                      //  �ִϸ��̼� isWalk, wDownŰ ������ isWalk�ִϸ��̼� ����

    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);     // �����̷��� �� (�Էµ� ����)�� LookAt(�ٶ󺻴�)

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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)      //�����Ҷ� ??? 
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);     //  rigid�� AddForce(���� �ش�) Vector3�߿� up(��) ��������*15����, ForceMode.�� Inpulse���

            anim.SetBool("isJump", true);       //?? �����Ѵ�
            anim.SetTrigger("doJump");          //?? Ʈ����
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
    { rigid.angularVelocity = Vector3.zero; }   //angularVelocity ����ȸ���ӵ�

    void StoptoWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 3, LayerMask.GetMask("Wall"));

    }

    void FixedUpdate()
    { 
        FreezRotation();
        StoptoWall();
    
    }        //�÷��̾� ȸ���� FixedUpdate�� ������


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
                    grenades[hasGrenades].SetActive(true); //�߿�
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

