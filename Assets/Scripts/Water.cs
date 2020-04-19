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
    private bool isDefultEnableState;

    [HideInInspector]
    public bool IsEnabled { get; set; } = false;

    void Awake()
    {
        IsEnabled = isDefultEnableState;
        if (IsEnabled)
        {
            WaterMaterial = GetComponent<MeshRenderer>().material;
            WaterMaterial.color = Color.blue;
            StartingColor = WaterMaterial.color;
            CurrentColor = WaterMaterial.color;
        }
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
        {
            return;
        }

        Setup();
        Debug.Log("Water has been hit by Lightning!");
        WaterMaterial.color = Color.green;
        CurrentColor = Color.green;
        FieldTile.ChangeState("Grass");
        isDefultEnableState = IsEnabled;
    }

    public void RainedOn()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        //If Water has been hit by water then do nothing, unless it has been hit by Lightning first, then take it back to water
        Debug.Log("Water has been hit by Water!");
        WaterMaterial.color = StartingColor;
        CurrentColor = StartingColor;
        FieldTile.ChangeState("Water");
        isDefultEnableState = IsEnabled;
    }

    public void OnMouseOverEvent()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        Debug.Log("Water has mouse pointer on it!");
        WaterMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        Debug.Log("Water has NO mouse pointer");
        WaterMaterial.color = CurrentColor;
    }

    /// <summary>
    /// Setup starting values
    /// </summary>
    private void Setup()
    {
        if (!WaterMaterial)
        {
            WaterMaterial = GetComponent<MeshRenderer>().material;
            WaterMaterial.color = Color.blue;
            StartingColor = WaterMaterial.color;
            CurrentColor = WaterMaterial.color;
        }
    }
}
