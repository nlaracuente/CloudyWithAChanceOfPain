using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, ITileState
{
    private Material WaterMaterial { get; set; }
    private Color StartingColor { get; set; }
    private Color CurrentColor { get; set; }
    private FieldTile FieldTile { get; set; }

    [SerializeField]
    private bool isEnabled;
    public bool IsEnabled { get; set; } = false;

    void Awake()
    {
        WaterMaterial = GetComponent<MeshRenderer>().material;
        StartingColor = WaterMaterial.color;
        CurrentColor = WaterMaterial.color;
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
        Debug.Log("Water has been hit by Lightning!");
        WaterMaterial.color = Color.green;
        CurrentColor = Color.green;
        FieldTile.ChangeState("Grass");
    }

    public void RainedOn()
    {
        if (!IsEnabled)
            return;
        //If Water has been hit by water then do nothing, unless it has been hit by Lightning first, then take it back to water
        Debug.Log("Water has been hit by Water!");
        WaterMaterial.color = StartingColor;
        CurrentColor = StartingColor;
        FieldTile.ChangeState("Water");
    }

    public void OnMouseOverEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Water has mouse pointer on it!");
        WaterMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
            return;
        Debug.Log("Water has NO mouse pointer");
        WaterMaterial.color = CurrentColor;
    }
}
