using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TK.Audio
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AudioManager manager = (AudioManager)target;

            GUILayout.Space(15);
            if (GUILayout.Button("Auto-populate SFX Data"))
            {
                AutoPopulateSFXData(manager);
            }
        }

        private void AutoPopulateSFXData(AudioManager manager)
        {
            string folderPath = "Assets/#TK/Sounds/SFXData";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/#TK/Sounds"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/#TK"))
                    {
                        AssetDatabase.CreateFolder("Assets", "#TK");
                    }
                    AssetDatabase.CreateFolder("Assets/#TK", "Sounds");
                }
                AssetDatabase.CreateFolder("Assets/#TK/Sounds", "SFXData");
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty sfxDataListProp = serializedManager.FindProperty("sfxDataList");

            Dictionary<SFXId, SFXData> currentMap = new Dictionary<SFXId, SFXData>();
            List<SFXData> allExistingData = new List<SFXData>();

            for (int i = 0; i < sfxDataListProp.arraySize; i++)
            {
                SerializedProperty elementProp = sfxDataListProp.GetArrayElementAtIndex(i);
                SFXData data = elementProp.objectReferenceValue as SFXData;
                if (data != null)
                {
                    if (!currentMap.ContainsKey(data.Id))
                    {
                        currentMap.Add(data.Id, data);
                    }
                    allExistingData.Add(data);
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:SFXData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SFXData data = AssetDatabase.LoadAssetAtPath<SFXData>(path);
                if (data != null && !currentMap.ContainsKey(data.Id))
                {
                    currentMap.Add(data.Id, data);
                    if (!allExistingData.Contains(data))
                    {
                        allExistingData.Add(data);
                    }
                }
            }

            System.Array values = System.Enum.GetValues(typeof(SFXId));
            foreach (SFXId id in values)
            {
                if (!currentMap.ContainsKey(id))
                {
                    string assetPath = $"{folderPath}/{id}SFXData.asset";
                    SFXData existingAsset = AssetDatabase.LoadAssetAtPath<SFXData>(assetPath);

                    if (existingAsset != null)
                    {
                        if (existingAsset.Id != id)
                        {
                            existingAsset.SetId(id);
                            EditorUtility.SetDirty(existingAsset);
                        }
                        currentMap.Add(id, existingAsset);
                        if (!allExistingData.Contains(existingAsset))
                        {
                            allExistingData.Add(existingAsset);
                        }
                    }
                    else
                    {
                        SFXData newAsset = ScriptableObject.CreateInstance<SFXData>();
                        newAsset.SetId(id);
                        AssetDatabase.CreateAsset(newAsset, assetPath);
                        currentMap.Add(id, newAsset);
                        allExistingData.Add(newAsset);
                        Debug.Log($"Created new SFXData asset at {assetPath} for {id}");
                    }
                }
            }

            sfxDataListProp.ClearArray();
            sfxDataListProp.arraySize = allExistingData.Count;
            for (int i = 0; i < allExistingData.Count; i++)
            {
                sfxDataListProp.GetArrayElementAtIndex(i).objectReferenceValue = allExistingData[i];
            }

            serializedManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("SFX Data Auto-populate completed!");
        }
    }
}
