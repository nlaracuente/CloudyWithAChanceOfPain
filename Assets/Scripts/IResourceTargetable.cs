using UnityEngine;

public interface IResourceTargetable
{
    bool IsAvailable { get; }
    Transform GetAccessPoint(bool isInDanger = false);
    void SetAccessPoint(Transform accessPoint);
}
