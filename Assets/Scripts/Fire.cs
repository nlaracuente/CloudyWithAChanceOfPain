using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, ITileState
{
    private Material FireMaterial { get; set; }
    private Color StartingColor { get; set; }
    private Color CurrentColor { get; set; }
    private FieldTile FieldTile { get; set; }

    [SerializeField]
    private bool isEnabled;
    public bool IsEnabled { get; set; } = false;

    void Awake()
    {
        IsEnabled = isEnabled;
        FireMaterial = GetComponent<MeshRenderer>().material;
        StartingColor = FireMaterial.color;
        CurrentColor = FireMaterial.color; 
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
        //If Fire is stuck by Lightning it should do nothing for now, unless it was hit by water first, then take it back to fire
        Debug.Log("Fire has been hit by Lightning!");
        FireMaterial.color = StartingColor;
        CurrentColor = StartingColor;
        FieldTile.ChangeState("Fire");
    }

    public void RainedOn()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Fire has been hit by Water!");
        FireMaterial.color = Color.blue;
        CurrentColor = Color.blue;
        FieldTile.ChangeState("Water");
    }

    public void OnMouseOverEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Fire has mouse pointer on it!");
        FireMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Fire has NO mouse pointer");
        FireMaterial.color = CurrentColor;
    }
}
