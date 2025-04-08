using TMPro;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 6f;
    [SerializeField]
    private float thrusterForce = 20f;
    [SerializeField]
    private PlayerController controller;

    private float distToGround = 0f;

    private ConfigurableJoint joint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        distToGround = GetComponent<Collider>().bounds.extents.y;
        
        //���� Configurable Joint��ֻ���ڵ������յ�ʱ������ AddComponent()�����ӻ���
        joint = GetComponent<ConfigurableJoint>();
        //if (joint != null)
        //{
        //    Destroy(joint);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        float xMov = Input.GetAxisRaw("Horizontal");
        float yMov = Input.GetAxisRaw("Vertical");

        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move(velocity);

        float xMouse = Input.GetAxisRaw("Mouse X");
        float yMouse = Input.GetAxisRaw("Mouse Y");

        Vector3 yRotation = new Vector3(0f, xMouse, 0f) * lookSensitivity;
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;
        controller.Rotate(yRotation, xRotation);

        //Ҳ������õ��ɵ�target pointȥ����ͼƽ��ĵ��£���Ϊ����������ڷǵ�����Ҳ��һ����Ч����

        //// Raycast to detect the surface below the player
        //bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f);

        //if (isGrounded)
        //{
        //    // Adjust the joint's target position to the top of the object
        //    Vector3 localTargetPosition = transform.InverseTransformPoint(hit.collider.bounds.center);
        //    joint.targetPosition = new Vector3(0f, localTargetPosition.y -6f, 0f);
        //    Debug.Log(joint.targetPosition);
        //}
        //else
        //{
        //    // Default target position for regular ground
        //    joint.targetPosition = Vector3.zero;
        //}

        //(���ɲ���)ģ�ⵯ������
        Vector3 force = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //joint = GetComponent<ConfigurableJoint>();
            
            force = Vector3.up * 40f;
            joint.yDrive = new JointDrive
            {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f,
            };
        }
        else
        {
            joint.yDrive = new JointDrive
            {
                positionSpring = 20f,
                positionDamper = 0f,
                maximumForce = 40f,
            };
        }
        controller.Thrust(force);



        //������Ծ
        if (Input.GetButton("Jump"))
        {
            joint.yDrive = new JointDrive
            {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f,
            };
            if (Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f)) //�ж��Ƿ���ground
            {
                Vector3 jumpForce = Vector3.up * thrusterForce;
                controller.Thrust(jumpForce);
            }
        }

    }
}
