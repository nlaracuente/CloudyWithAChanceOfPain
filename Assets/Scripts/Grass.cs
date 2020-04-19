using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour, ITileState
{
    private Material GrassMaterial { get; set; }
    private Color CurrentColor { get; set; }
    private FieldTile FieldTile { get; set; }

    [SerializeField]
    private bool isEnabled;
    public bool IsEnabled { get; set; } = false;

    void Awake()
    {
        GrassMaterial = GetComponent<MeshRenderer>().material;
        CurrentColor = GrassMaterial.color;
        FieldTile = gameObject.GetComponent<FieldTile>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FieldTile = gameObject.GetComponent<FieldTile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StruckedByLightning()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Grass has been hit by Lightning!");
        GrassMaterial.color = Color.red;
        CurrentColor = Color.red;
        FieldTile.ChangeState("Fire");
    }

    public void RainedOn()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Grass has been hit by Water!");
        GrassMaterial.color = Color.blue;
        CurrentColor = Color.blue;
        FieldTile.ChangeState("Water");
    }

    public void OnMouseOverEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Grass has mouse pointer on it!");
        GrassMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Grass has NO mouse pointer");
        GrassMaterial.color = CurrentColor;
    }
}
