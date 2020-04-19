using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    Sheep sheep;

    /// <summary>
    /// The layer mask where the floor we care for collision is at
    /// </summary>
    [SerializeField]
    LayerMask clickableLayer;

    bool IsGameOver { get; set; }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    IConsumable GetConsumableStriked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        IConsumable consumable = default;
        if(Physics.Raycast (ray, out hit, Mathf.Infinity, clickableLayer))
        {
            consumable = hit.collider.GetComponent<IConsumable>();
        }

        return consumable;
    }

    void OnSceneLoaded()
    {
        IsGameOver = false;
        sheep = FindObjectOfType<Sheep>();
    }
}
