using System;
using UnityEngine;

[Serializable]
public class PlayerWeapon
{
    public string name = "M16";
    public int damage = 10;
    public float range = 100f;

    public float shootRate = 10f; //ÿ����Դ���ٷ��ӵ� <=0��Ϊ����ģʽ >0��Ϊ����ģʽ
    public float shootCoolDownTime = 0.75f; //����ģʽ����ȴʱ��
    public float recoilForce = 2f; //������

    public int maxBullets = 30;
    public int bullets = 30;
    public float reloadTime = 2f;

    [HideInInspector]
    public bool isReloading = false;

    public GameObject graphics;
}
