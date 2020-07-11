using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MessageEditor : EditorWindow
{
    public TutorialData  tutorialData;

    private string gameDataProjectFilePath = "/StreamingAssets/messagedata.json";

    [MenuItem("Window/Messages Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(MessageEditor));
    }
    void OnGUI()
    {
        if (tutorialData != null)
        {
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("tutorialData");
            EditorGUILayout.PropertyField(serializedProperty, true);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save data"))
            {
                SaveGameData();
            }
        }

        if (GUILayout.Button("Load data"))
        {
            LoadGameData();
        }
    }

    private void LoadGameData()
    {
        string filePath = Application.dataPath + gameDataProjectFilePath;

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
             tutorialData= JsonUtility.FromJson<TutorialData>(dataAsJson);
        }
        else
        {
          tutorialData = new TutorialData();
        }
    }

    private void SaveGameData()
    {

        string dataAsJson = JsonUtility.ToJson(tutorialData);

        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);

    }
}
