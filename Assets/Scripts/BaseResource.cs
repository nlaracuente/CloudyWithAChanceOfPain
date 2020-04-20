using System.Collections.Generic;
using UnityEngine;

abstract public class BaseResource : MonoBehaviour, IConsumable, IResourceTargetable
{
    [SerializeField]
    Transform accessPoints;

    protected Queue<Transform> availableAccessPoints;
    abstract public bool IsConsumable { get; }
    abstract public Resource ResourceType { get; }
    
    abstract public void Consume();

    abstract public void RainedOn();

    abstract public void StruckedByLightning();

    protected virtual void Start()
    {
        availableAccessPoints = new Queue<Transform>();
        for (var i = 0; i < accessPoints.childCount; i++)
        {
            var child = accessPoints.GetChild(i);
            availableAccessPoints.Enqueue(child);
        }
    }

    public bool IsAvailable
    {
        get { return availableAccessPoints.Count > 0; }
    }

    /// <summary>
    /// Defaults to self to be safe
    /// </summary>
    /// <returns></returns>
    public Transform GetAccessPoint()
    {
        return IsAvailable ? availableAccessPoints.Dequeue() : transform;
    }

    public void SetAccessPoint(Transform accessPoint)
    {
        if (!availableAccessPoints.Contains(accessPoint))
            availableAccessPoints.Enqueue(accessPoint);
    }

    abstract public void SetAsNotConsumable();
}
