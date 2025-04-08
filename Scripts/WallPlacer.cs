using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class WallPlacer : NetworkBehaviour
{
    public GameObject wallPrefab;          // ʵ��ǽԤ����
    public GameObject wallPreviewPrefab;   // Ԥ��ǽԤ����
    private GameObject previewInstance;    // ��ǰԤ��ǽʵ��

    public LayerMask groundLayer;          // �ذ�ͼ��

    private bool canPlace = false;         // ����Ƿ���Է���
    private bool isPreviewActive = false;  // ���Ԥ��ǽ�Ƿ񼤻�

    private bool isOnCooldown = false;     // �����ȴ��
    public float cooldownTime = 10f;       // ��ȴʱ��
    private float cooldownTimer = 0f;      // ��ȴ��ʱ��

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("��ȴ���������Է���ǽ�壡");
            }
        }

        if (isPreviewActive) // ֻ�м���Ԥ��ʱ�Ŵ���Ԥ���ͷ���
        {
            HandleWallPreview();
            HandleWallPlacement();
            //HandleWallPlacementServerRpc();

            //Destroy(previewInstance);
            //isPreviewActive = false;
        }
        // ���� C ������Ԥ��ǽ
        if (Input.GetKeyDown(KeyCode.C) && !isOnCooldown)
        {
            ActivatePreview();
        }
    }

    // �� C ����ʵ����Ԥ��ǽ
    private void ActivatePreview()
    {
        if (isPreviewActive) return;          // ��ֹ�ظ�����

        previewInstance = Instantiate(wallPreviewPrefab);
        isPreviewActive = true;
        Debug.Log("Ԥ��ǽ�����ɣ�");
    }

    // ��ʾԤ��ǽ
    private void HandleWallPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);   // ���λ������
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 placementPos = hit.point;                         // �������е�
            placementPos.y += wallPrefab.transform.localScale.y / 2;  // ����ǽ��߶�

            // ����ǽ���������������ת�Ƕ�
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;  // ���Դ�ֱ����
            Quaternion wallRotation = Quaternion.LookRotation(-camForward);  // ���߳��������

            // ����Ԥ��ǽλ�úͳ���
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

    // ��������ʵ��ǽ
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

        // ���� UI ��ʾ��ȴ
        if (PlayerUI.Singleton != null)
        {
            PlayerUI.Singleton.StartWallCooldownUI(cooldownTime);
        }

        Debug.Log($"��ȴ������{cooldownTime}��");
    }
}
