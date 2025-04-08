using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class WallPlacer : NetworkBehaviour
{
    public GameObject wallPrefab;          // 实体墙预制体
    public GameObject wallPreviewPrefab;   // 预览墙预制体
    private GameObject previewInstance;    // 当前预览墙实例

    public LayerMask groundLayer;          // 地板图层

    private bool canPlace = false;         // 标记是否可以放置
    private bool isPreviewActive = false;  // 标记预览墙是否激活

    private bool isOnCooldown = false;     // 标记冷却中
    public float cooldownTime = 10f;       // 冷却时间
    private float cooldownTimer = 0f;      // 冷却计时器

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("冷却结束，可以放置墙体！");
            }
        }

        if (isPreviewActive) // 只有激活预览时才处理预览和放置
        {
            HandleWallPreview();
            HandleWallPlacement();
            //HandleWallPlacementServerRpc();

            //Destroy(previewInstance);
            //isPreviewActive = false;
        }
        // 按下 C 键激活预览墙
        if (Input.GetKeyDown(KeyCode.C) && !isOnCooldown)
        {
            ActivatePreview();
        }
    }

    // 按 C 键后实例化预览墙
    private void ActivatePreview()
    {
        if (isPreviewActive) return;          // 防止重复生成

        previewInstance = Instantiate(wallPreviewPrefab);
        isPreviewActive = true;
        Debug.Log("预览墙已生成！");
    }

    // 显示预览墙
    private void HandleWallPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);   // 鼠标位置射线
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 placementPos = hit.point;                         // 射线命中点
            placementPos.y += wallPrefab.transform.localScale.y / 2;  // 调整墙体高度

            // 计算墙体面向摄像机的旋转角度
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;  // 忽略垂直分量
            Quaternion wallRotation = Quaternion.LookRotation(-camForward);  // 法线朝向摄像机

            // 设置预览墙位置和朝向
            previewInstance.transform.position = placementPos;
            previewInstance.transform.rotation = wallRotation;

            previewInstance.SetActive(true);
            canPlace = true;
        }
        else
        {
            previewInstance.SetActive(false);
            canPlace = false;
        }
    }

    // 按键生成实体墙
    private void HandleWallPlacement()
    {
        if (canPlace && Input.GetMouseButtonDown(1))  
        {
            //GameObject WallPrefab = Instantiate(wallPrefab, previewInstance.transform.position, previewInstance.transform.rotation);

            //Destroy(WallPrefab, 6f);
            WallServerRpc(previewInstance.transform.position, previewInstance.transform.rotation);
            StartCooldown();

            Destroy(previewInstance);
            isPreviewActive = false;
        } 
    }

    private void Wall(Vector3 pos, Quaternion rotation)
    {
        GameObject WallPrefab = Instantiate(wallPrefab, pos, rotation);

        Destroy(WallPrefab, 6f);
    }


    [ClientRpc]
    private void WallClientRpc(Vector3 pos, Quaternion rotation)
    {
        Wall(pos, rotation);
    }

    [ServerRpc]
    private void WallServerRpc(Vector3 pos, Quaternion rotation)
    {
        if (!IsHost)
        {
            Wall(pos, rotation);
        }
       WallClientRpc(pos, rotation);
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;

        // 调用 UI 显示冷却
        if (PlayerUI.Singleton != null)
        {
            PlayerUI.Singleton.StartWallCooldownUI(cooldownTime);
        }

        Debug.Log($"冷却启动：{cooldownTime}秒");
    }
}
