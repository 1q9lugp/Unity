using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class Act3NavMeshFix : MonoBehaviour
{
    void Awake()
    {
        var surface = gameObject.GetComponent<NavMeshSurface>();
        if (surface == null) surface = gameObject.AddComponent<NavMeshSurface>();
        
        surface.collectObjects = CollectObjects.All;
        // Use the correct enum namespace or underlying value
        surface.useGeometry = (NavMeshCollectGeometry)0; 
        surface.BuildNavMesh();
        
        Debug.Log("Act3 NavMesh rebaked at Y=" + transform.position.y);
    }
}