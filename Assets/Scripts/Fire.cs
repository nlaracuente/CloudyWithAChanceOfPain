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
    private bool isDefultEnableState;

    [HideInInspector]
    public bool IsEnabled { get; set; } = false;

    void Awake()
    {
        IsEnabled = isDefultEnableState;
        if (IsEnabled)
        {
            FireMaterial = GetComponent<MeshRenderer>().material;
            FireMaterial.color = Color.red;
            StartingColor = FireMaterial.color;
            CurrentColor = FireMaterial.color;
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
        //If Fire is stuck by Lightning it should do nothing for now, unless it was hit by water first, then take it back to fire
        Debug.Log("Fire has been hit by Lightning!");
        FireMaterial.color = StartingColor;
        CurrentColor = StartingColor;
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
        Debug.Log("Fire has been hit by Water!");
        FireMaterial.color = Color.blue;
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
        Debug.Log("Fire has mouse pointer on it!");
        FireMaterial.color = Color.yellow;
    }

    public void OnMouseExitEvent()
    {
        if (!IsEnabled)
        {
            return;
        }

        Setup();
        Debug.Log("Fire has NO mouse pointer");
        FireMaterial.color = CurrentColor;
    }

    /// <summary>
    /// Setup starting values
    /// </summary>
    private void Setup()
    {
        if (!FireMaterial)
        {
            FireMaterial = GetComponent<MeshRenderer>().material;
            FireMaterial.color = Color.red;
            StartingColor = FireMaterial.color;
            CurrentColor = FireMaterial.color;
        }
    }
}
