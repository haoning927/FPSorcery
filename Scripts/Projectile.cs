using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionVFX;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        Destroy(gameObject);
        GameObject explosionEffectObject = Instantiate(explosionVFX, transform.position, transform.rotation);

        if (other.CompareTag("Player"))
        {
            FireBallServerRpc(other.name, 50);
        }

        Destroy(explosionEffectObject, 2f);
    }

    [ServerRpc]
    private void FireBallServerRpc(string hittedName, int damage)
    {
        //GameManager.UpdateInfo(transform.name + " hit " + hittedName);
        Player player = GameManager.Singleton.GetPlayer(hittedName);
        player.TakeDamage(damage);
    }
}
