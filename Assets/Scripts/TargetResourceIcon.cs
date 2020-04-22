using UnityEngine;
using UnityEngine.UI;

public class TargetResourceIcon : MonoBehaviour
{
    [SerializeField]
    GameObject iconContainer;

    [SerializeField] Image icon;
    [SerializeField] Sprite normalIcon;
    [SerializeField] Sprite dangerIcon;

    private void Awake()
    {
        iconContainer.SetActive(false);
    }

    public void ShowIcon(bool isInDanger = false)
    {
        // Don't change the high alert
        if (icon.sprite != dangerIcon)
            icon.sprite = isInDanger ? dangerIcon : normalIcon;

        iconContainer.SetActive(true);
    }

    public void HideIcon()
    {
        iconContainer.SetActive(false);

        // Reset high alertt
        icon.sprite = normalIcon;
    }
}
