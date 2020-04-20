using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestResource : MonoBehaviour, IConsumable
{
    public bool IsConsumable { get { return true; } }
    public Resource ResourceType { get { return Resource.Rest; } }
    public void Consume() { }
    public void RainedOn() { }
    public void StruckedByLightning() { }
}
