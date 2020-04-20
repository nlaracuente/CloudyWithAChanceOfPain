using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    [SerializeField] GameObject highlighter;

    // Update is called once per frame
    void Update()
    {
        if (LevelController.Instance.IsGameOver)
        {
            highlighter.SetActive(false);
            return;
        }

        UpdatePosition();
    }

    void UpdatePosition()
    {
        var go = Cloud.Instance.GetClickableObjectUnderMouse();
        if (go == null)
            highlighter.SetActive(false);
        else
        {
            var xPos = go.transform.position;
            transform.position = new Vector3(xPos.x, transform.position.y, xPos.z);
            highlighter.SetActive(true);
        }
    }
}
