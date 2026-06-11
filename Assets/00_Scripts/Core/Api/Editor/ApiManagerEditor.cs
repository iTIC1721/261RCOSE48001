using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// 개발/테스트용 유저 초기화 도구.
/// Inspector 상단에 "Dev Tools" 섹션으로 표시됩니다.
/// 빌드에는 포함되지 않습니다.
/// </summary>
[CustomEditor(typeof(ApiManager))]
public class ApiManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기존 Inspector 필드 유지
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("─── Dev Tools ───", EditorStyles.boldLabel);

        // 현재 user_id 표시
        string userId = PlayerPrefs.GetString("user_id", "(없음)");
        EditorGUILayout.LabelField("현재 user_id");
        EditorGUILayout.SelectableLabel(userId, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        EditorGUILayout.Space(4);

        // 유저 초기화 버튼
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("user_id 초기화 (새 계정으로 시작)"))
        {
            if (EditorUtility.DisplayDialog(
                "user_id 초기화",
                $"현재 user_id({userId})를 삭제합니다.\n다음 실행 시 새 계정이 발급됩니다.\n계속하시겠습니까?",
                "초기화", "취소"))
            {
                PlayerPrefs.DeleteKey("user_id");
                PlayerPrefs.Save();
                Debug.Log("[DevTools] user_id 삭제 완료. 다음 Play 시 새 계정이 발급됩니다.");
            }
        }
        GUI.backgroundColor = Color.white;
    }
}
#endif