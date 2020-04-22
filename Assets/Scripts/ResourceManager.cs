using System.Linq;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    BaseResource[] Resources { get; set; }

    private void Start()
    {
        Resources = FindObjectsOfType<BaseResource>();
    }

    public BaseResource GetAvailableResource(Resource type, BaseResource curResource)
    {
        var resources = Resources.Where(r => r.ResourceType == type && r.IsAvailable).ToList();

        // Default to the most current
        if (resources.Count == 0)
            return curResource;

        // Avoid getting the same current one
        BaseResource resource = curResource;

        var tries = 0;
        while (resource == curResource && tries < resources.Count)
        {
            var i = Random.Range(0, resources.Count);
            resource = resources[i];
            tries++;
        }

        return resource;
    }

}
