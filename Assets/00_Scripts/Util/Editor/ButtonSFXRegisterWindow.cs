// Editor/ButtonSFXRegisterWindow.cs
// 사용법: Unity 메뉴 → Tools → Button SFX Register

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonSFXRegisterWindow : EditorWindow
{
    // ── 내부 데이터 ────────────────────────────────────────────────────
    private class ButtonEntry
    {
        public Button button;
        public bool include;
        public bool alreadyHooked;

        // 인라인 편집용 임시값
        public string editKey;
        public float editVolume;
        public bool editPooled;

        /// <summary>편집값이 핸들러 현재값과 다른지 매번 직접 비교합니다.</summary>
        public bool IsDirty()
        {
            if (!alreadyHooked) return false;
            var h = button.GetComponent<ButtonSFXHandler>();
            if (h == null) return false;
            return editKey != h.sfxKey
                || !Mathf.Approximately(editVolume, h.volumeScale)
                || editPooled != h.usePooled;
        }
    }

    private List<ButtonEntry> _entries = new();
    private Vector2 _scrollPos;

    // 신규 등록용 공통 SFX 설정
    private string _sfxKey = "UI_Click";
    private float _volumeScale = 1f;
    private bool _usePooled = true;

    // 필터
    private string _filterText = "";
    private bool _showOnlyNew = false;

    // ── 메뉴 진입점 ───────────────────────────────────────────────────
    [MenuItem("Tools/Button SFX Register")]
    public static void Open()
    {
        var win = GetWindow<ButtonSFXRegisterWindow>("Button SFX Register");
        win.minSize = new Vector2(560, 540);
        win.ScanScene();
    }

    // ── 씬 스캔 ───────────────────────────────────────────────────────
    private void ScanScene()
    {
        _entries.Clear();

        var allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var btn in allButtons)
        {
            if (!IsSceneObject(btn.gameObject)) continue;

            var handler = btn.GetComponent<ButtonSFXHandler>();
            bool hooked = handler != null;

            var entry = new ButtonEntry
            {
                button = btn,
                include = true,
                alreadyHooked = hooked,
                // 기존 핸들러 값을 편집 필드 초기값으로 사용
                editKey = hooked ? handler.sfxKey : _sfxKey,
                editVolume = hooked ? handler.volumeScale : _volumeScale,
                editPooled = hooked ? handler.usePooled : _usePooled,
            };

            _entries.Add(entry);
        }
    }

    private static bool IsSceneObject(GameObject go)
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
            return prefabStage.IsPartOfPrefabContents(go);
        return go.scene.IsValid() && go.scene == SceneManager.GetActiveScene();
    }

    // ── GUI ───────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawToolbar();
        DrawNewSFXSettings();
        EditorGUILayout.Space(4);
        DrawFilterBar();
        EditorGUILayout.Space(4);
        DrawButtonList();
        EditorGUILayout.Space(6);
        DrawFooter();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("씬 재스캔", EditorStyles.toolbarButton, GUILayout.Width(90)))
            ScanScene();
        if (GUILayout.Button("전체 선택", EditorStyles.toolbarButton, GUILayout.Width(70)))
            SetAllInclude(true);
        if (GUILayout.Button("전체 해제", EditorStyles.toolbarButton, GUILayout.Width(70)))
            SetAllInclude(false);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"총 {_entries.Count}개", EditorStyles.toolbarButton, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawNewSFXSettings()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("공통 설정값", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        _sfxKey = EditorGUILayout.TextField(
            new GUIContent("SFX Key", "미등록 버튼에 적용할 기본 키 / 일괄 덮어쓰기 시 사용"),
            _sfxKey);
        _volumeScale = EditorGUILayout.Slider(
            new GUIContent("Volume Scale"), _volumeScale, 0f, 1f);
        _usePooled = EditorGUILayout.Toggle(
            new GUIContent("풀링 사용 (PlaySFXPooled)"), _usePooled);

        EditorGUI.indentLevel--;

        EditorGUILayout.Space(2);

        // 일괄 덮어쓰기: 체크박스로 항목 단위 제어
        EditorGUILayout.BeginHorizontal();
        _bulkKey = EditorGUILayout.ToggleLeft("Key", _bulkKey, GUILayout.Width(52));
        _bulkVolume = EditorGUILayout.ToggleLeft("Volume", _bulkVolume, GUILayout.Width(68));
        _bulkPooled = EditorGUILayout.ToggleLeft("Pooled", _bulkPooled, GUILayout.Width(68));
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!_bulkKey && !_bulkVolume && !_bulkPooled);
        if (GUILayout.Button("선택 버튼에 일괄 적용", GUILayout.Width(140)))
            BulkApply();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    // 일괄 적용할 항목 선택 토글
    private bool _bulkKey = true;
    private bool _bulkVolume = false;
    private bool _bulkPooled = false;

    /// <summary>체크된 필드만 선택된 모든 항목의 editKey/editVolume/editPooled에 덮어씁니다.</summary>
    private void BulkApply()
    {
        foreach (var e in _entries)
        {
            if (!e.include) continue;
            if (_bulkKey) e.editKey = _sfxKey;
            if (_bulkVolume) e.editVolume = _volumeScale;
            if (_bulkPooled) e.editPooled = _usePooled;
        }
    }

    private void DrawFilterBar()
    {
        EditorGUILayout.BeginHorizontal();
        _filterText = EditorGUILayout.TextField("이름 필터", _filterText);
        _showOnlyNew = EditorGUILayout.ToggleLeft("미등록만", _showOnlyNew, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawButtonList()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        EditorGUILayout.LabelField("포함", GUILayout.Width(36));
        EditorGUILayout.LabelField("버튼 경로", GUILayout.Width(160));
        EditorGUILayout.LabelField("SFX Key", GUILayout.Width(100));
        EditorGUILayout.LabelField("Vol", GUILayout.Width(90));
        EditorGUILayout.LabelField("Pooled", GUILayout.Width(46));
        EditorGUILayout.LabelField("상태", GUILayout.Width(72));
        EditorGUILayout.EndHorizontal();

        DrawSeparator();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (entry.button == null) continue;

            string path = GetHierarchyPath(entry.button.transform);
            bool dirty = entry.IsDirty();

            if (!string.IsNullOrEmpty(_filterText) &&
                !path.ToLower().Contains(_filterText.ToLower())) continue;
            if (_showOnlyNew && entry.alreadyHooked) continue;

            Color oldBg = GUI.backgroundColor;
            if (dirty)
                GUI.backgroundColor = new Color(1f, 0.92f, 0.6f, 1f);
            else if (entry.alreadyHooked)
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);

            EditorGUILayout.BeginHorizontal();

            // 체크박스
            entry.include = EditorGUILayout.Toggle(entry.include, GUILayout.Width(20));

            // 경로 (클릭 → 하이라이트)
            if (GUILayout.Button(path, EditorStyles.label, GUILayout.Width(160)))
            {
                Selection.activeGameObject = entry.button.gameObject;
                EditorGUIUtility.PingObject(entry.button.gameObject);
            }

            // 인라인 편집 필드
            entry.editKey = EditorGUILayout.TextField(entry.editKey, GUILayout.Width(100));
            entry.editVolume = EditorGUILayout.Slider(entry.editVolume, 0f, 1f, GUILayout.Width(90));
            entry.editPooled = EditorGUILayout.Toggle(entry.editPooled, GUILayout.Width(20));

            // 상태 배지 / 즉시 적용 버튼
            if (entry.alreadyHooked)
            {
                if (dirty)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
                    if (GUILayout.Button("적용", GUILayout.Width(46)))
                    {
                        WriteToHandler(entry);
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    }
                    GUI.backgroundColor = oldBg;
                }
                else
                {
                    GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
                    s.normal.textColor = new Color(0.3f, 0.7f, 0.3f);
                    EditorGUILayout.LabelField("✔ 등록됨", s, GUILayout.Width(60));
                }
            }
            else
            {
                GUIStyle s = new GUIStyle(EditorStyles.miniLabel);
                s.normal.textColor = new Color(0.8f, 0.5f, 0.2f);
                EditorGUILayout.LabelField("미등록", s, GUILayout.Width(60));
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = oldBg;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawFooter()
    {
        DrawSeparator();

        int selectedNew = 0;
        int selectedDirty = 0;
        int skippedClean = 0;
        foreach (var e in _entries)
        {
            if (!e.include) continue;
            if (!e.alreadyHooked) selectedNew++;
            else if (e.IsDirty()) selectedDirty++;
            else skippedClean++;
        }

        EditorGUILayout.HelpBox(
            $"신규 등록 대상: {selectedNew}개  |  설정 변경 대상: {selectedDirty}개  |  변경 없음(스킵): {skippedClean}개",
            MessageType.Info);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("▶  선택된 버튼에 SFX 등록 / 설정 업데이트", GUILayout.Height(32)))
            RegisterOrUpdateSFX();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);
    }

    // ── 등록 / 업데이트 로직 ──────────────────────────────────────────
    private void RegisterOrUpdateSFX()
    {
        int registered = 0;
        int updated = 0;
        int skipped = 0;

        Undo.SetCurrentGroupName("Register/Update Button SFX");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (var entry in _entries)
        {
            if (!entry.include || entry.button == null) continue;

            if (!entry.alreadyHooked)
            {
                var handler = Undo.AddComponent<ButtonSFXHandler>(entry.button.gameObject);
                handler.sfxKey = entry.editKey;
                handler.volumeScale = entry.editVolume;
                handler.usePooled = entry.editPooled;
                EditorUtility.SetDirty(entry.button.gameObject);
                // 등록 완료 후 상태 갱신
                entry.alreadyHooked = true;
                registered++;
            }
            else if (entry.IsDirty())
            {
                WriteToHandler(entry);
                updated++;
            }
            else
            {
                skipped++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("완료",
            $"처리 완료!\n" +
            $"  - 새로 등록:      {registered}개\n" +
            $"  - 설정 업데이트:  {updated}개\n" +
            $"  - 변경 없음(스킵): {skipped}개\n\n" +
            $"씬을 저장하는 것을 잊지 마세요 (Ctrl+S).",
            "확인");

        ScanScene();
    }

    /// <summary>
    /// entry의 편집값을 ButtonSFXHandler에 기록합니다.
    /// Undo 그룹은 호출부에서 관리하므로 여기서는 RecordObject만 합니다.
    /// </summary>
    private static void WriteToHandler(ButtonEntry entry)
    {
        var handler = entry.button.GetComponent<ButtonSFXHandler>();
        if (handler == null) return;

        Undo.RecordObject(handler, "Update ButtonSFXHandler");
        handler.sfxKey = entry.editKey;
        handler.volumeScale = entry.editVolume;
        handler.usePooled = entry.editPooled;
        EditorUtility.SetDirty(handler);
    }

    // ── 유틸 ──────────────────────────────────────────────────────────
    private static string GetHierarchyPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetHierarchyPath(t.parent) + "/" + t.name;
    }

    private void SetAllInclude(bool value)
    {
        foreach (var e in _entries) e.include = value;
    }

    private static void DrawSeparator()
    {
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
    }
}
#endif