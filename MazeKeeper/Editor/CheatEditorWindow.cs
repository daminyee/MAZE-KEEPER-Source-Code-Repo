using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Component;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using UnityEditor;
using UnityEngine;


public class CheatEditorWindow : EditorWindow
{
    [MenuItem("Tools/CheatTool")]
    public static void ShowCheatTool()
    {
        GetWindow<CheatEditorWindow>("CheatTool");
    }


    string addMoneyInputStr     = string.Empty;
    string currentStageInputStr = string.Empty;


    void SwitchInvincible()
    {
        CompEnemy.IsInvincible = !CompEnemy.IsInvincible;
    }


    void KillAllEnemies()
    {
        // if (PauseManager.Instance != null)
        // {
        //     GameManager.Instance.KillAllEnemies();
        // }
    }


    void AddGold(int addGold)
    {
        if (PauseManager.Instance != null)
        {
            PlayerDataManager.Instance.AddGold(addGold);
        }
    }


    void AddGem(int addGem)
    {
        if (PauseManager.Instance != null)
        {
            PlayerDataManager.Instance.AddGem(addGem);
        }
    }


    void DeleteAllPlayerPrefsKeys()
    {
        PlayerPrefs.DeleteAll();
    }


    void OnGUI()
    {
        GUILayout.Label("재화 기능", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUILayout.Label("추가할 돈 : ");

        var addMoney = 0;

        addMoneyInputStr = GUILayout.TextField(addMoneyInputStr, GUILayout.Width(250));
        if (addMoneyInputStr != string.Empty && int.TryParse(addMoneyInputStr, out addMoney) == false)
        {
            var invalidStartIndex = 0;

            for (int i = 0; i < addMoneyInputStr.Length; i++)
            {
                var character = addMoneyInputStr[i];
                if (character is < '0' or > '9')
                {
                    invalidStartIndex = i;
                    break;
                }
            }
            addMoneyInputStr = addMoneyInputStr.Substring(0, invalidStartIndex);
        }

        if (GUILayout.Button("골드 추가"))
        {
            AddGold(addMoney);
        }
        if (GUILayout.Button("젬 추가"))
        {
            AddGem(addMoney);
        }

        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.Label("스테이지 변경", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("스테이지 : ");

        var currentStage = 0;

        currentStageInputStr = GUILayout.TextField(currentStageInputStr, GUILayout.Width(250));
        if (currentStageInputStr != string.Empty && int.TryParse(currentStageInputStr, out currentStage) == false)
        {
            var invalidStartIndex = 0;

            for (int i = 0; i < currentStageInputStr.Length; i++)
            {
                var character = currentStageInputStr[i];
                if (character is < '0' or > '9')
                {
                    invalidStartIndex = i;
                    break;
                }
            }
            currentStageInputStr = currentStageInputStr.Substring(0, invalidStartIndex);
        }

        if (GUILayout.Button("변경"))
        {
            PlayerDataManager.Instance.PlayerData.CurrentStageIndex = currentStage;
            SceneLoadManager.Instance.ChangeScene(SceneName.CharacterSelectScene);
        }

        GUILayout.EndHorizontal();

        //

        GUILayout.Label("");
        GUILayout.Label("타워 레벨변경", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("모든 타워레벨 0"))
        {
            for (int i = 0; i < (int)CellObjectType.Length - 1; i++)
            {
                PlayerDataManager.Instance.PlayerData.CellObjectLevelList[i]       = 0;
                PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[i] = true;
            }
        }

        if (GUILayout.Button("모든 타워레벨 1"))
        {
            for (int i = 0; i < (int)CellObjectType.Length - 1; i++)
            {
                PlayerDataManager.Instance.PlayerData.CellObjectLevelList[i]       = 1;
                PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[i] = true;
            }
        }

        if (GUILayout.Button("모든 타워레벨 2"))
        {
            for (int i = 0; i < (int)CellObjectType.Length - 1; i++)
            {
                PlayerDataManager.Instance.PlayerData.CellObjectLevelList[i]       = 2;
                PlayerDataManager.Instance.PlayerData.CellObjectUnlockCheckList[i] = true;
            }
        }

        GUILayout.EndHorizontal();

        //

        GUILayout.Label("");
        GUILayout.Label("기타 기능", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("현재 PlayerData 저장", GUILayout.Height(40), GUILayout.Width(150)))
        {
            PlayerDataManager.Instance.Save();
        }
        if (GUILayout.Button("PlayerPrefs 초기화", GUILayout.Height(40), GUILayout.Width(150)))
        {
            DeleteAllPlayerPrefsKeys();
        }

        GUILayout.EndHorizontal();
    }
}