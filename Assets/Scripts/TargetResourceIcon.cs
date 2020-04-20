using UnityEngine;

public class TargetResourceIcon : MonoBehaviour
{
    [SerializeField]
    GameObject iconContainer;

    private void Awake()
    {
        iconContainer.SetActive(false);
    }

    public void SetTarget(Transform target)
    {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
        iconContainer.SetActive(true);
    }

    public void DisableTarget()
    {
        iconContainer.SetActive(false);
    }
}
