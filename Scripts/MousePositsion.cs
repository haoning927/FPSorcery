using TMPro;
using UnityEngine;

public class MousePositsion : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;

        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit raycastHit))
        {
            //transform.position = raycastHit.point;
            transform.position = targetPosition;
        }
    }
}
