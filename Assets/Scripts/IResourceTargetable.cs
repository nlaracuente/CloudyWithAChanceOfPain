using UnityEngine;

public interface IResourceTargetable
{
    bool IsAvailable { get; }
    Transform GetAccessPoint();
    void SetAccessPoint(Transform accessPoint);
}
