using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SuspicionModule))]
public class SuspicionEditorVisualisation : Editor
{
    public void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
            return;

        var t = target as SuspicionModule;

        var data = t.GetSuspectData();
        var color = new Color(1, 0.8f, 0.4f, 1);
        Handles.color = color;
        GUI.color = color;

        foreach(var k in data)
            Handles.Label(k.Key.gameObject.transform.position, $"Visible: {k.Value.Visible}\nAware: {k.Value.Awareness}");
    }
}
