using System;
using System.Collections.Generic;
using System.IO;
using Core.Data.Event;
using UnityEditor;
using UnityEngine;

public class EventEditorWindow : EditorWindow
{
    // ── 상수 ─────────────────────────────────────────────────────────────
    private const string DB_PATH       = "Assets/Data/EventDatabase.asset";
    private const string SAVE_ROOT     = "Assets/Data/Events";
    private const float  LIST_WIDTH    = 200f;
    private static readonly string[] TAB_LABELS = { "Event", "Dialog", "Combat", "Reward" };

    // ── 상태 ─────────────────────────────────────────────────────────────
    private EventDatabaseSO _db;
    private int             _tabIndex;
    private int             _selectedIdx = -1;
    private Editor          _cachedEditor;
    private Vector2         _listScroll;
    private Vector2         _inspectorScroll;

    // ── 메뉴 진입점 ───────────────────────────────────────────────────────
    [MenuItem("Window/FTL/Event Editor")]
    public static void Open() => GetWindow<EventEditorWindow>("Event Editor");

    // ── 초기화 ────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        TryLoadDatabase();
    }

    private void TryLoadDatabase()
    {
        _db = AssetDatabase.LoadAssetAtPath<EventDatabaseSO>(DB_PATH);
        if (_db == null)
        {
            // FindAssets로 폴더 무관하게 재탐색
            var guids = AssetDatabase.FindAssets("t:EventDatabaseSO");
            if (guids.Length > 0)
                _db = AssetDatabase.LoadAssetAtPath<EventDatabaseSO>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

    // ── GUI 메인 ──────────────────────────────────────────────────────────
    private void OnGUI()
    {
        if (_db == null)
        {
            DrawNoDatabaseUI();
            return;
        }

        DrawToolbar();

        EditorGUILayout.BeginHorizontal();
        {
            DrawListPanel();
            DrawSeparator();
            DrawInspectorPanel();
        }
        EditorGUILayout.EndHorizontal();
    }

    // ── DB 없음 화면 ──────────────────────────────────────────────────────
    private void DrawNoDatabaseUI()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("EventDatabase.asset을 찾을 수 없습니다.", MessageType.Warning);
        if (GUILayout.Button("데이터베이스 생성하기", GUILayout.Height(30)))
            CreateDatabase();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
    }

    private void CreateDatabase()
    {
        EnsureDirectory(Path.GetDirectoryName(DB_PATH));
        var db = CreateInstance<EventDatabaseSO>();
        AssetDatabase.CreateAsset(db, DB_PATH);
        AssetDatabase.SaveAssets();
        _db = db;
    }

    // ── 상단 툴바 ─────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        int prev = _tabIndex;
        _tabIndex = GUILayout.Toolbar(_tabIndex, TAB_LABELS);
        if (_tabIndex != prev)
        {
            _selectedIdx  = -1;
            _cachedEditor = null;
        }
    }

    // ── 좌측 리스트 패널 ──────────────────────────────────────────────────
    private void DrawListPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(LIST_WIDTH));
        {
            // 버튼 높이(한 줄 + 여백)만큼 빼서 스크롤 뷰가 버튼을 덮지 않게 함
            float buttonAreaHeight = EditorGUIUtility.singleLineHeight + 6f;
            float scrollHeight     = position.height - buttonAreaHeight - 30f; // 30 = toolbar + 여백
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(Mathf.Max(scrollHeight, 40f)));
            DrawList();
            EditorGUILayout.EndScrollView();

            DrawListButtons();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawList()
    {
        var list = GetCurrentList();
        for (int i = 0; i < list.Count; i++)
        {
            var so = list[i];
            string label = so != null ? so.name : "(null)";

            bool selected = i == _selectedIdx;
            var  style    = selected
                ? new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.cyan } }
                : EditorStyles.label;

            if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true)))
            {
                _selectedIdx  = i;
                _cachedEditor = null;   // 선택 변경 시 캐시 초기화
            }
        }
    }

    private void DrawListButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add"))
            AddNew();
        GUI.enabled = _selectedIdx >= 0 && _selectedIdx < GetCurrentList().Count;
        if (GUILayout.Button("- Delete"))
            DeleteSelected();
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    // ── 구분선 ────────────────────────────────────────────────────────────
    private void DrawSeparator()
    {
        var rect = GUILayoutUtility.GetRect(1f, position.height, GUILayout.Width(1f));
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
    }

    // ── 우측 인스펙터 패널 ────────────────────────────────────────────────
    private void DrawInspectorPanel()
    {
        EditorGUILayout.BeginVertical();
        {
            var list = GetCurrentList();
            if (_selectedIdx < 0 || _selectedIdx >= list.Count)
            {
                EditorGUILayout.HelpBox("좌측 목록에서 항목을 선택하세요.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var selected = list[_selectedIdx];
            if (selected == null)
            {
                EditorGUILayout.HelpBox("(삭제된 Asset)", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            Editor.CreateCachedEditor(selected, null, ref _cachedEditor);

            _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);
            EditorGUI.BeginChangeCheck();
            _cachedEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                TryRenameByTitle(selected);
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    // ── Add / Delete ──────────────────────────────────────────────────────
    private void AddNew()
    {
        ScriptableObject newSO = _tabIndex switch
        {
            0 => CreateInstance<EventSO>(),
            1 => CreateInstance<DialogSubEventSO>(),
            2 => CreateInstance<CombatSubEventSO>(),
            3 => CreateInstance<RewardSubEventSO>(),
            _ => null
        };
        if (newSO == null) return;

        string folder   = GetSaveFolderForTab(_tabIndex);
        string typeName = TAB_LABELS[_tabIndex];
        string fileName = $"New{typeName}_{DateTime.Now:yyyyMMdd_HHmmss}.asset";
        string path     = Path.Combine(folder, fileName).Replace('\\', '/');

        EnsureDirectory(folder);
        AssetDatabase.CreateAsset(newSO, path);

        AddToDatabase(newSO);
        EditorUtility.SetDirty(_db);
        AssetDatabase.SaveAssets();

        _selectedIdx  = GetCurrentList().Count - 1;
        _cachedEditor = null;
        Repaint();
    }

    private void DeleteSelected()
    {
        var list = GetCurrentList();
        if (_selectedIdx < 0 || _selectedIdx >= list.Count) return;

        var so = list[_selectedIdx];
        RemoveFromDatabase(so);
        EditorUtility.SetDirty(_db);

        if (so != null)
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(so));

        AssetDatabase.SaveAssets();
        _selectedIdx  = Mathf.Clamp(_selectedIdx - 1, -1, list.Count - 1);
        _cachedEditor = null;
        Repaint();
    }

    // ── 헬퍼: DB 목록 접근 ───────────────────────────────────────────────
    private IList<ScriptableObject> GetCurrentList()
    {
        // IList<T>를 IList<ScriptableObject>로 노출
        return _tabIndex switch
        {
            0 => WrapList(_db.Events),
            1 => WrapList(_db.DialogEvents),
            2 => WrapList(_db.CombatEvents),
            3 => WrapList(_db.RewardEvents),
            _ => new List<ScriptableObject>()
        };
    }

    private static IList<ScriptableObject> WrapList<T>(List<T> source)
        where T : ScriptableObject
    {
        var result = new List<ScriptableObject>(source.Count);
        foreach (var item in source) result.Add(item);
        return result;
    }

    private void AddToDatabase(ScriptableObject so)
    {
        switch (_tabIndex)
        {
            case 0: _db.Add((EventSO)so);          break;
            case 1: _db.Add((DialogSubEventSO)so); break;
            case 2: _db.Add((CombatSubEventSO)so); break;
            case 3: _db.Add((RewardSubEventSO)so); break;
        }
    }

    private void RemoveFromDatabase(ScriptableObject so)
    {
        switch (_tabIndex)
        {
            case 0: _db.Remove((EventSO)so);          break;
            case 1: _db.Remove((DialogSubEventSO)so); break;
            case 2: _db.Remove((CombatSubEventSO)so); break;
            case 3: _db.Remove((RewardSubEventSO)so); break;
        }
    }

    private static string GetSaveFolderForTab(int tab) => tab switch
    {
        0 => $"{SAVE_ROOT}/Events",
        1 => $"{SAVE_ROOT}/Dialog",
        2 => $"{SAVE_ROOT}/Combat",
        3 => $"{SAVE_ROOT}/Reward",
        _ => SAVE_ROOT
    };

    // ── Title 변경 시 Asset 자동 리네임 ──────────────────────────────────
    private void TryRenameByTitle(ScriptableObject so)
    {
        var sp = new SerializedObject(so).FindProperty("Title");
        if (sp == null || string.IsNullOrWhiteSpace(sp.stringValue)) return;

        string prefix = _tabIndex switch
        {
            0 => "Event",
            1 => "Dialog",
            2 => "Combat",
            3 => "Reward",
            _ => "Event"
        };

        string newName = $"{prefix}_{sp.stringValue}";
        string path    = AssetDatabase.GetAssetPath(so);
        if (string.IsNullOrEmpty(path)) return;

        // 이미 같은 이름이면 무시
        if (Path.GetFileNameWithoutExtension(path) == newName) return;

        AssetDatabase.RenameAsset(path, newName);
        AssetDatabase.SaveAssets();
        Repaint();
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }
}
