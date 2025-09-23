using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;


public class ResaveAssets
{
    // 프로젝트 창에서 우클릭 시 메뉴 아이템을 추가합니다.
    [MenuItem("Assets/Resave Selected Assets")]
    private static void ResaveSelectedAssets()
    {
        // 변경된 애셋을 추적할 카운터
        int savedCount = 0;

        // 선택된 모든 오브젝트의 GUID를 가져옵니다.
        string[] guids = Selection.assetGUIDs;

        foreach (string guid in guids)
        {
            // GUID로부터 애셋 경로를 얻습니다.
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 경로가 폴더인지 확인합니다.
            if (Directory.Exists(assetPath))
            {
                // 폴더인 경우, 하위의 모든 ScriptableObject와 Prefab을 찾습니다.
                string[] subGuids = AssetDatabase.FindAssets("t:ScriptableObject t:Prefab", new string[] { assetPath });

                foreach (string subGuid in subGuids)
                {
                    string subAssetPath = AssetDatabase.GUIDToAssetPath(subGuid);
                    Object asset        = AssetDatabase.LoadAssetAtPath<Object>(subAssetPath);

                    if (asset != null)
                    {
                        // 애셋을 'dirty'로 표시하여 변경되었음을 알립니다.
                        EditorUtility.SetDirty(asset);
                        savedCount++;
                        Debug.Log($"Resaved: {subAssetPath}");
                    }
                }
            }
            else // 폴더가 아닌 단일 파일인 경우
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                // 애셋이 ScriptableObject 또는 Prefab인지 확인합니다.
                if (asset is ScriptableObject || PrefabUtility.IsPartOfPrefabAsset(asset))
                {
                    EditorUtility.SetDirty(asset);
                    savedCount++;
                    Debug.Log($"Resaved: {assetPath}");
                }
            }
        }

        if (savedCount > 0)
        {
            // 변경된 모든 애셋을 디스크에 저장합니다.
            AssetDatabase.SaveAssets();
            Debug.Log($"<b><color=green>Successfully re-saved {savedCount} assets.</color></b>");
        }
        else
        {
            Debug.Log("No ScriptableObjects or Prefabs were found in the selection.");
        }
    }


    // 메뉴 아이템의 유효성 검사
    // 선택된 애셋이 하나 이상 있을 때만 메뉴를 활성화합니다.
    [MenuItem("Assets/Resave Selected Assets", true)]
    private static bool ResaveSelectedAssetsValidation()
    {
        return Selection.assetGUIDs.Length > 0;
    }
}