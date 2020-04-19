using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TravisMain : MonoBehaviour
{
    private enum  MouseClick {
        LeftClick = 0,
        RightClick = 1
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Check for mouse input
        if (Input.GetMouseButtonDown((int)MouseClick.LeftClick))
        {
            FireLightning();
        }
        else if (Input.GetMouseButton((int)MouseClick.RightClick))
        { 
            DropRain();
        }

        //Take away the Reticle every from, we will figure out if it should be put on an object in LateUpdate()
        RemoveReticle();
    }
    //LateUpdate is called after Update but before frame is graphically updated to the player
    private void LateUpdate()
    {
        //Highlight the tile a player is hovering over
        DrawReticle();
    }

    /// <summary>
    ///  Fire Lighting on the current object we are clicking.
    ///  Using RayCasting for point of click
    ///  Using Interface to control behavior
    /// </summary>
    private void FireLightning()
    {
        //Use Ray Casting to check for hit
        //Create ray
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Create hit info
        RaycastHit hitInfo;

        //Did we hit anything?
        if(Physics.Raycast(rayOrigin, out hitInfo))
        {
            //Grab the IDousable Interface ofthe object we hit
            FieldTile obj = hitInfo.collider.GetComponent<FieldTile>();
            Debug.Log($"hitInfo.collider.name:  {hitInfo.collider.name}");
            Debug.Log($"obj:  {obj}");

            //We hit a "Attackable" object
            if(obj != null)
            {
                obj.StruckedByLightning();
            }
        }
    }

    /// <summary>
    ///  Drop Rain on the current object we are clicking.
    ///  Using RayCasting for point of click
    ///  Using Interface to control behavior
    /// </summary>
    private void DropRain()
    {
        //Use Ray Casting to check for hit
        //Create ray
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Create hit info
        RaycastHit hitInfo;

        //Did we hit anything?
        if (Physics.Raycast(rayOrigin, out hitInfo))
        {
            //Grab the IDousable Interface ofthe object we hit
            FieldTile obj = hitInfo.collider.GetComponent<FieldTile>();

            //We hit a "Dousable" object
            if (obj != null)
            {
                obj.RainedOn();
            }
        }
    }

    /// <summary>
    /// Method to let the player see what they are selecting in the game
    /// Method uses Raycating to calcuate
    /// </summary>
    private void DrawReticle()
    {
        //Use Ray Casting to check for hit
        //Create ray
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Create hit info
        RaycastHit hitInfo;

        // *You can use RaycastAll() to get ack more than one object*
        // https://docs.unity3d.com/ScriptReference/Physics.RaycastAll.html
        //Did we hit anything?
        if (Physics.Raycast(rayOrigin, out hitInfo))
        {
            //Grab the ISelectable Interface ofthe object we hit
            ITileState obj = hitInfo.collider.GetComponents<ITileState>().Where(c => c.IsEnabled).FirstOrDefault();

            //We hit a "Selectable" object
            if (obj != null)
            {
                obj.OnMouseOverEvent();
            }
        } 
    }

    /// <summary>
    /// Method to remove the reticle graphic from all selectable objects in the scene
    /// </summary>
    private void RemoveReticle()
    {
        MonoBehaviour[] gameObjects = (MonoBehaviour[])FindObjectsOfType(typeof(MonoBehaviour));
        foreach (var gameObject in gameObjects)
        {
            ITileState selectableObject = gameObject.GetComponents<ITileState>().Where(c => c.IsEnabled).FirstOrDefault();
            if (selectableObject != null)
            {
                //Remove the reticle from the object
                selectableObject.OnMouseExitEvent();
            }
        }
    }
}
