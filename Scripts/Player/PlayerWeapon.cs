using System;
using UnityEngine;

[Serializable]
public class PlayerWeapon
{
    public string name = "M16";
    public int damage = 10;
    public float range = 100f;

    public float shootRate = 10f; //每秒可以打多少发子弹 <=0则为单发模式 >0则为连发模式
    public float shootCoolDownTime = 0.75f; //单发模式的冷却时间
    public float recoilForce = 2f; //后坐力

    public int maxBullets = 30;
    public int bullets = 30;
    public float reloadTime = 2f;

    [HideInInspector]
    public bool isReloading = false;

    public GameObject graphics;
}
