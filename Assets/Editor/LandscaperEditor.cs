using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Landscaper))]
[CanEditMultipleObjects]
public class LandscaperEditor : Editor
{
    SerializedProperty isPortrait;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        isPortrait = serializedObject.FindProperty("_isPortrait");
        Debug.LogFormat("OnEnable {0}", isPortrait);
    }
    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        //Debug.Log(serializedObject);
        serializedObject.Update();
        if(!isPortrait.boolValue)
        EditorGUILayout.LabelField("Landscape mode");
        else
            EditorGUILayout.LabelField("Portrait mode");

    }
}