using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;
    
    private Camera sceneCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    
        if (!IsLocalPlayer)
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Remote Player"));
            DisableComponents();
        }
        else
        {
            PlayerUI.Singleton.SetPlayer(GetComponent<Player>());
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Player"));

            sceneCamera = Camera.main;
            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }

        //SetPlayerName();
        string name = "Player " + GetComponent<NetworkObject>().NetworkObjectId.ToString();
        Player player = GetComponent<Player>();
        player.Setup();

        GameManager.Singleton.RegisterPlayer(name, player);

    }

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask) //自己和子组件的图层是完全独立的，所以要递归去改
    {
        transform.gameObject.layer = layerMask;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerMaskForAllChildren(transform.GetChild(i), layerMask);
        }
    }

    //private void SetPlayerName()
    //{
    //    transform.name = "Player" + GetComponent<NetworkObject>().NetworkObjectId;
    //}

    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }

        GameManager.Singleton.UnregisterPlayer(transform.name);
    }

    //private void OnDisable()
    //{
    //    if (sceneCamera != null)
    //    {
    //        sceneCamera.gameObject.SetActive(true);
    //    }
    //}
}
