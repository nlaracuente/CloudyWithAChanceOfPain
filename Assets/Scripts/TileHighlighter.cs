using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    [SerializeField] LayerMask clickableLayer;
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
        Ray ray = LevelController.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            highlighter.SetActive(false);

        if (hit.transform == null)
            return;

        var xPos = hit.transform.position;
        transform.position = new Vector3(xPos.x, transform.position.y, xPos.z);
        highlighter.SetActive(true);
    }
}
