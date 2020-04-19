using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour, ITileState
{
    private Material GrassMaterial { get; set; }
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
            GrassMaterial = GetComponent<MeshRenderer>().material;
            GrassMaterial.color = Color.green;
            StartingColor = GrassMaterial.color;
            CurrentColor = GrassMaterial.color;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        FieldTile = GetComponent<FieldTile>();
        Debug.Log($"FieldTile:  { FieldTile}");
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
        Debug.Log("Grass has been hit by Lightning!");
        GrassMaterial.color = Color.red;
        CurrentColor = Color.red;
        FieldTile.ChangeState("Fire");
        isDefultEnableState = IsEnabled;

    }

    public void RainedOn()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        Debug.Log("Grass has been hit by Water!");
        GrassMaterial.color = Color.blue;
        CurrentColor = Color.blue;
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
        Debug.Log("Grass has mouse pointer on it!");
        GrassMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        Debug.Log("Grass has NO mouse pointer");
        GrassMaterial.color = CurrentColor;
    }

    /// <summary>
    /// Setup starting values
    /// </summary>
    private void Setup()
    {
        if (!GrassMaterial)
        {
            GrassMaterial = GetComponent<MeshRenderer>().material;
            GrassMaterial.color = Color.green;
            StartingColor = GrassMaterial.color;
            CurrentColor = GrassMaterial.color;
        }
    }
}
