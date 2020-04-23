using UnityEngine;
using UnityEngine.UI;

public class MedalController : MonoBehaviour
{
    [SerializeField] GameObject bronzeMedal;
    [SerializeField] GameObject silverMedal;
    [SerializeField] GameObject goldMedal;
    [SerializeField] Text totalText;

    void Start()
    {
        var total = GameManager.Instance.TotalSheeps;
        totalText.text = total.ToString();

        bronzeMedal.SetActive(total < 4);
        silverMedal.SetActive(total > 3 && total < 8);
        goldMedal.SetActive(total >= 7);
    }
}
