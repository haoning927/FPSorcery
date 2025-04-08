using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 yRotation = Vector3.zero; //Rotate character
    private Vector3 xRotation = Vector3.zero; //Rotate camera
    private float recoilForce = 0f; //������

    private float cameraRotationTotal = 0f; //�ۼ�ת�˶��ٶ�
    [SerializeField]
    private float cameraRotationLimit = 85f; //�����ת����

    private Vector3 thrusterForce = Vector3.zero; //Up force

    private float eps = 0.01f; //����
    private Vector3 lastFramePosition = Vector3.zero; //��¼��һ֡��λ��
    private Animator animator;

    private float distToGround = 0f;

    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();

        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    public void Move(Vector3 _verlocity)
    {
        velocity = _verlocity;
    }

    public void Rotate(Vector3 _yRotation, Vector3 _xRoatation)
    {
        yRotation = _yRotation;
        xRotation = _xRoatation;
    }

    public void Thrust(Vector3 _thrusterForce)
    {
        thrusterForce = _thrusterForce;
    }

    public void AddRecoilForce(float newRecoilForce)
    {
        recoilForce += newRecoilForce;
    }

    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + Time.fixedDeltaTime * velocity);
        }


        // (���ɣ���Ҫ�� Configurable Joint
        //if (thrusterForce != Vector3.zero)
        //{
        //    rb.AddForce(thrusterForce); //apply Time.fixedDeltatime 0.02s
        //    thrusterForce = Vector3.zero;
        //}

        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce); //apply Time.fixedDeltatime 0.02s
            thrusterForce = Vector3.zero;
        }
    }

    private void PerfomRotation()
    {
        if (recoilForce < 0.1)
        {
            recoilForce = 0f;
        }

        if (yRotation != Vector3.zero || recoilForce > 0)
        {
            rb.transform.Rotate(yRotation + rb.transform.up * Random.Range(-2f * recoilForce, 2f * recoilForce));
        }

        if (xRotation != Vector3.zero || recoilForce > 0)
        {
            //cam.transform.Rotate(xRotation);
            cameraRotationTotal += xRotation.x - recoilForce;
            cameraRotationTotal = Mathf.Clamp(cameraRotationTotal, -cameraRotationLimit, cameraRotationLimit);
            cam.transform.localEulerAngles = new Vector3(cameraRotationTotal, 0f, 0f);
        }

        recoilForce *= 0.5f;
    }

    private void performAnimation()
    {
        Vector3 deltaPosition = transform.position - lastFramePosition;
        lastFramePosition = transform.position;

        float forward = Vector3.Dot(deltaPosition, transform.forward);
        float right = Vector3.Dot(deltaPosition, transform.right);

        int direction = 0; //��ֹ
        if (forward > eps)
        {
            direction = 1; //ǰ
        }
        else if (forward < -eps)
        {
            if (right > eps)
            {
                direction = 4; //�Һ�
            }
            else if (right < -eps)
            {
                direction = 6; //���
            }
            else
            {
                direction = 5; //��
            }
        }
        else if (right > eps)
        {
            direction = 3; //��
        }
        else if (right < -eps)
        {
            direction = 7; //��
        }

        if (!Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
        {
            direction = 8;
        }

            if (GetComponent<Player>().IsDead())
        {
            direction = -1;
        }

        animator.SetInteger("direction", direction);

    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            PerformMovement();
            PerfomRotation();
        }
        performAnimation();
    }
}
