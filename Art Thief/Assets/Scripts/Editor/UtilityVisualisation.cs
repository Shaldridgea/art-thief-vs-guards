using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UtilityBehaviour))]
public class UtilityVisualisation : Editor
{
    public void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
            return;

        var t = target as UtilityBehaviour;

        var callback = t.GetDebugDrawCallback();
        if (callback != null)
            callback();
    }
}
