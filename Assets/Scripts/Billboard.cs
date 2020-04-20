using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Face towards the camera
    void LateUpdate()
    {
        transform.LookAt(transform.position + LevelController.Instance.MainCamera.transform.forward);
    }
}
