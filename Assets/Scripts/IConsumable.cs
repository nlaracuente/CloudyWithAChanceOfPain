using System.Collections;
using UnityEngine;

/// <summary>
/// A consumable is a resource the sheep can consume
/// </summary>
public interface IConsumable: IAttackable, IDousable
{
    bool IsConsumable { get; }
    void Consume();
    void SetAsNotConsumable();
    GameObject gameObject { get; }
    Resource ResourceType { get; }
}

public enum Resource
{
    Food,
    Water,
    Rest,
}
