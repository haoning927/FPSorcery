using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Singleton;

    private Player player = null;

    [SerializeField]
    private TextMeshProUGUI bulletsText;
    [SerializeField]
    private GameObject bulletsObject;

    private WeaponManager weaponManager;

    [SerializeField]
    private Transform healthBarFill;
    [SerializeField]
    private GameObject healthBarObject;

    [Header("Wall Cooldown UI")]
    [SerializeField]
    private GameObject wallPlacerObject;
    [SerializeField]
    private Image wallCooldownOverlay;
    [SerializeField]
    private TextMeshProUGUI wallCooldownText;

    private float wallCooldownTime = 0f;
    private float wallCooldownRemaining = 0f;
    private bool isWallCooling = false;

    [Header("Fireball Cooldown UI")]
    [SerializeField]
    private GameObject FireballObject;
    [SerializeField] 
    private Image fireballCooldownOverlay;
    [SerializeField] 
    private TextMeshProUGUI fireballCooldownText;

    private float fireballCooldownTime = 0f;
    private float fireballCooldownRemaining = 0f;
    private bool isFireballCooling = false;


    private void Awake()
    {
        Singleton = this;

        if (wallCooldownOverlay != null)
            wallCooldownOverlay.fillAmount = 0f;

        if (wallCooldownText != null)
            wallCooldownText.text = "";

        if (fireballCooldownOverlay != null)
            fireballCooldownOverlay.fillAmount = 0f;

        if (fireballCooldownText != null)
            fireballCooldownText.text = "";
    }

    public void SetPlayer(Player localPlayer)
    {
        player = localPlayer;
        weaponManager = player.GetComponent<WeaponManager>();
        bulletsObject.SetActive(true);
        healthBarObject.SetActive(true);
        wallPlacerObject.SetActive(true);
        FireballObject.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        var currentWeapon = weaponManager.GetCurrentWeapon();
        
        if (currentWeapon == null) return;

        if (currentWeapon.isReloading)
        {
            bulletsText.text = "Reloading...";
        }
        else
        {
            bulletsText.text = "Bullets: " + currentWeapon.bullets + "/" + currentWeapon.maxBullets;
        }

        healthBarFill.localScale = new Vector3(player.GetHealth() / 100f, 1f, 1f);

        if (isWallCooling)
        {
            wallCooldownRemaining -= Time.deltaTime;
            wallCooldownOverlay.fillAmount = wallCooldownRemaining / wallCooldownTime;

            if (wallCooldownRemaining <= 0)
            {
                wallCooldownOverlay.fillAmount = 0;
                wallCooldownText.text = "";
                isWallCooling = false;
            }
            else
            {
                wallCooldownText.text = Mathf.CeilToInt(wallCooldownRemaining).ToString();
            }
        }

        if (isFireballCooling)
        {
            fireballCooldownRemaining -= Time.deltaTime;
            fireballCooldownOverlay.fillAmount = fireballCooldownRemaining / fireballCooldownTime;

            if (fireballCooldownRemaining <= 0f)
            {
                fireballCooldownOverlay.fillAmount = 0f;
                fireballCooldownText.text = "";
                isFireballCooling = false;
            }
            else
            {
                fireballCooldownText.text = Mathf.CeilToInt(fireballCooldownRemaining).ToString();
            }
        }

    }

    public void StartWallCooldownUI(float cooldown)
    {
        wallCooldownTime = cooldown;
        wallCooldownRemaining = cooldown;
        isWallCooling = true;
        wallCooldownOverlay.fillAmount = 1f;
        wallCooldownText.text = Mathf.CeilToInt(wallCooldownRemaining).ToString();
    }

    public void StartFireballCooldownUI(float cooldown)
    {
        fireballCooldownTime = cooldown;
        fireballCooldownRemaining = cooldown;
        isFireballCooling = true;
        fireballCooldownOverlay.fillAmount = 1f;
        fireballCooldownText.text = Mathf.CeilToInt(cooldown).ToString();
    }
}
