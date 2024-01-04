using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Unity.AI.Navigation;

public class BakeAllNavMeshes : MonoBehaviour {
    [MenuItem("Tools/AI/Bake All NavMeshes")]
    static void BakeAll() {
        NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();
        foreach (var surface in surfaces) {
            surface.BuildNavMesh();
        }
        Debug.Log("Bakeo de todos los NavMeshSurface completado.");
    }

    [MenuItem("Tools/AI/Clear All NavMeshes")]
    static void ClearAll() {
        // Limpia todos los NavMeshes
        NavMesh.RemoveAllNavMeshData();

        Debug.Log("Limpieza de todos los NavMeshes completada.");
    }
}
