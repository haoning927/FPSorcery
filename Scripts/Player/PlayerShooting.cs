using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;

    private float shootCoolDownTime = 0f; //距离上次开枪，过了多久
    private int autoShootCount = 0; //当前一共连开多少枪

    private int fireballCount = 0;
    private float fireballCooldownTime = 8f;
    private float nextFireTime = 0f;

    [SerializeField]
    private LayerMask mask;

    private Camera cam;
    private PlayerController playerController;

    public GameObject projectile;

    enum HitEffectMaterial
    {
        Metal,
        Stone,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        shootCoolDownTime += Time.deltaTime;

        if (!IsLocalPlayer) return; //如果不是本地玩家就不要操作了

        currentWeapon = weaponManager.GetCurrentWeapon();

        if (Input.GetKeyDown(KeyCode.K))
        {
            ShootServerRpc(transform.name, 10);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnFireBallServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponManager.Reload(currentWeapon);
            return;
        }

        if (currentWeapon.shootRate <= 0) //单发
        {
            if (Input.GetButtonDown("Fire1") && shootCoolDownTime >= currentWeapon.shootCoolDownTime)
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime = 0f; //重置冷却时间
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate);
            }
            else if (Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q))
            {
                CancelInvoke("Shoot");
            }
        }
    }

    public void StopShoooting()
    {
        CancelInvoke("Shoot");
    }

    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material) //击中点的特效
    {
        GameObject hitEffectPrefab;
        if (material == HitEffectMaterial.Metal)
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().metalHitEffectPrefab;
        }
        else
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().stoneHitEffectPrefab;
        }

        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal)); //特效施放方向与子弹射来方向相反
        ParticleSystem particleSystem = hitEffectObject.GetComponent<ParticleSystem>();
        particleSystem.Emit(1); //瞬间触发，否则击中特效不同步
        particleSystem.Play();
        Destroy(hitEffectObject, 2f);
    }

    [ClientRpc]
    private void OnHitClientRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        OnHit(pos, normal, material);
    }

    [ServerRpc]
    private void OnHitServerRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        if (!IsHost)
        {
            OnHit(pos, normal, material);
        }
        OnHitClientRpc(pos, normal, material);
    }

    private void OnShoot(float recoilForce) //每次射击相关的逻辑，包括特效、声音等
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
        weaponManager.GetCurrentAudioSource().Play();

        if (IsLocalPlayer) //控制玩家移动是在client端，所以只有本地玩家才会添加后坐力
        {
            playerController.AddRecoilForce(recoilForce);
        }
    }

    [ClientRpc]
    private void OnShootClientRpc(float recoilForce)
    {
        OnShoot(recoilForce);
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoilForce)
    {
        if (!IsHost)
        {
            OnShoot(recoilForce);
        }
        OnShootClientRpc(recoilForce);
    }

    private void Shoot()
    {
        if (currentWeapon.bullets <= 0 || currentWeapon.isReloading) return;

        currentWeapon.bullets--;

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload(currentWeapon);
        }

        autoShootCount++;
        float recoilForce = currentWeapon.recoilForce;

        if (autoShootCount <= 3)
        {
            recoilForce *= 0.2f;
        }

        OnShootServerRpc(recoilForce);

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.range, mask)) //Detect whether there's a object got hitted
        {
            //ShootServerRpc(hit.collider.name);
            if (hit.collider.tag == PLAYER_TAG) //Detect whether there's a player got hitted
            {
                ShootServerRpc(hit.collider.name, currentWeapon.damage);
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Metal);
            }
            else
            {
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Stone);
            }
        }
    }

    private void OnFireBall()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        Vector3 firePosition = cam.transform.position + cam.transform.forward * 1.5f;
        Quaternion fireRotation = cam.transform.rotation;

        GameObject fireball = Instantiate(projectile, firePosition, fireRotation);
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        rb.linearVelocity = fireRotation * Vector3.forward * 10;

        fireballCount++;

        // 每发射3个火球后，进入冷却
        if (fireballCount >= 3)
        {
            nextFireTime = Time.time + fireballCooldownTime;
            fireballCount = 0;

            // 调用 UI 冷却
            if (PlayerUI.Singleton != null)
            {
                PlayerUI.Singleton.StartFireballCooldownUI(fireballCooldownTime);
            }
        }
    }

    [ClientRpc]
    private void OnFireBallClientRpc()
    {
        OnFireBall();
    }

    [ServerRpc]
    private void OnFireBallServerRpc()
    {
        if (!IsHost)
        {
            OnFireBall();
        }
        OnFireBallClientRpc();
    }

    [ServerRpc]
    private void ShootServerRpc(string hittedName, int damage)
    {
        //GameManager.UpdateInfo(transform.name + " hit " + hittedName);
        Player player = GameManager.Singleton.GetPlayer(hittedName);
        player.TakeDamage(damage);
    }
}

