using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpaceProjectile))]
[CanEditMultipleObjects]
public class SpaceProjectileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // This forces Unity to draw the inspector using the simplest possible method,
        // bypassing the broken automatic layout logic that's causing your error.
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        serializedObject.ApplyModifiedProperties();
    }
}