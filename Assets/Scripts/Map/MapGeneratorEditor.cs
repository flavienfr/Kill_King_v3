using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
            mapGen.GenerateMap();

        if (GUILayout.Button("Generate"))
            mapGen.GenerateMap();
        if (GUILayout.Button("Clear"))
            mapGen.ClearMap();
        if (GUILayout.Button("Save"))
            mapGen.SaveMap();
    }
}
