using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FieldTile : MonoBehaviour
{
    [SerializeField]
    private Fire FireComponent;
    [SerializeField]
    private Water WaterComponent;
    [SerializeField]
    private Grass GrassComponent;

    public ITileState State
    {
        get
        {
            return gameObject.GetComponents<ITileState>().Where(c => c.IsEnabled).FirstOrDefault();
        }
        set
        {

        }
    }

    private void Awake()
    {
        FireComponent = GetComponent<Fire>();
        WaterComponent = GetComponent<Water>();
        GrassComponent = GetComponent<Grass>();
        var temp = State;
    }

    public void OnMouseExitEvent()
    {
        //throw new System.NotImplementedException();
    }

    public void OnMouseOverEvent()
    {
        //throw new System.NotImplementedException();
    }

    public void RainedOn()
    {
        State?.RainedOn();
    }

    public void StruckedByLightning()
    {
        State?.StruckedByLightning();
    }

    public void ChangeState(string newState)
    {
        if (State != null)
        {
            State.IsEnabled = false;
        }

        //Add code to take the new state and use that to enable the right one
        switch(newState)
        {
            case "Fire":
                FireComponent.IsEnabled = true;
                Debug.Log("FireComponent.enabled:  " + FireComponent.IsEnabled);
                break;
            case "Grass":
                GrassComponent.IsEnabled = true;
                Debug.Log("GrassComponent.enabled:  " + GrassComponent.IsEnabled);
                break;
            case "Water":
                WaterComponent.IsEnabled = true;
                Debug.Log("WaterComponent.enabled:  " + WaterComponent.IsEnabled);
                break;
            default:
                Debug.Log("FieldTitle.ChagneState():  invalid newState Value in switch.  newState.GetType().ToString() = " + newState.GetType().ToString());
                break;
        }
    }


}
