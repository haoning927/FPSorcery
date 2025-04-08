using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon primaryWeapon;
    [SerializeField]
    private PlayerWeapon secondaryWeapon;
    [SerializeField]
    private GameObject weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;
    private AudioSource currentAudioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    public void EquipWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;

        if (weaponHolder.transform.childCount > 0)
        {
            Destroy(weaponHolder.transform.GetChild(0).gameObject);
        }

        GameObject weaponObject = Instantiate(currentWeapon.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);
        weaponObject.transform.SetParent(weaponHolder.transform);

        //SetLayerMaskForAllChildren(weaponObject.transform, LayerMask.NameToLayer("Weapon"));

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();
        currentAudioSource = weaponObject.GetComponent<AudioSource>();

        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f;
        }
    }

    //private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask) //自己和子组件的图层是完全独立的，所以要递归去改
    //{
    //    transform.gameObject.layer = layerMask;

    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        SetLayerMaskForAllChildren(transform.GetChild(i), layerMask);
    //    }
    //}

    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public AudioSource GetCurrentAudioSource()
    {
        return currentAudioSource;
    }

    private void ToggleWeapon()
    {
        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        } else
        {
            EquipWeapon(primaryWeapon);
        }
    }

    [ClientRpc]
    private void ToggleWeaponClientRpc()
    {
        ToggleWeapon();
    }

    [ServerRpc]
    private void ToggleWeaponServerRpc()
    {
        if (!IsHost)
        {
            ToggleWeapon(); //如果是Host就会切换两边武器，因为host = server + client. 如果是在host调试就把这一行comment掉
        }
        ToggleWeaponClientRpc();
    }


    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeaponServerRpc();
            }
        }
    }

    public void Reload(PlayerWeapon playerWeapon)
    {
        if (playerWeapon.isReloading) return;

        playerWeapon.isReloading = true;

        StartCoroutine(ReloadCoroutine(playerWeapon));
    }

    private IEnumerator ReloadCoroutine(PlayerWeapon playerWeapon)
    {
        yield return new WaitForSeconds(playerWeapon.reloadTime);

        playerWeapon.bullets = playerWeapon.maxBullets;
        playerWeapon.isReloading = false;
    }
}

/* TODO: 
 * 有个Bug：第一个玩家切换成手枪收，
 * 新的玩家加入时看到的还是长枪 */
