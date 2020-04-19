using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshController : MonoBehaviour
{
    [SerializeField]
    NavMeshSurface surface;

    void Awake()
    {
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
        }
        // surface.BuildNavMesh();
    }
}
