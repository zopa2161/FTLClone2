using System;
using System.IO;
using Core.Data.Event;
using UnityEditor;
using UnityEngine;

public class EnemyShipEditorWindow : EditorWindow
{
    // ── 상수 ─────────────────────────────────────────────────────────────
    private const string DB_PATH    = "Assets/Data/EnemyShipDatabase.asset";
    private const string SAVE_ROOT  = "Assets/Data/EnemyShips";
    private const float  LIST_WIDTH = 200f;
    private static readonly string[] TAB_LABELS = { "EnemyShip" };

    // ── 상태 ─────────────────────────────────────────────────────────────
    private EnemyShipDatabaseSO _db;
    private int                 _tabIndex;
    private int                 _selectedIdx = -1;
    private Editor              _cachedEditor;
    private Vector2             _listScroll;
    private Vector2             _inspectorScroll;

    // ── 메뉴 진입점 ───────────────────────────────────────────────────────
    [MenuItem("Window/FTL/EnemyShip Editor")]
    public static void Open() => GetWindow<EnemyShipEditorWindow>("EnemyShip Editor");

    // ── 초기화 ────────────────────────────────────────────────────────────
    private void OnEnable() => TryLoadDatabase();

    private void TryLoadDatabase()
    {
        _db = AssetDatabase.LoadAssetAtPath<EnemyShipDatabaseSO>(DB_PATH);
        if (_db == null)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyShipDatabaseSO");
            if (guids.Length > 0)
                _db = AssetDatabase.LoadAssetAtPath<EnemyShipDatabaseSO>(
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
        EditorGUILayout.HelpBox("EnemyShipDatabase.asset을 찾을 수 없습니다.", MessageType.Warning);
        if (GUILayout.Button("데이터베이스 생성하기", GUILayout.Height(30)))
            CreateDatabase();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
    }

    private void CreateDatabase()
    {
        EnsureDirectory(Path.GetDirectoryName(DB_PATH));
        var db = CreateInstance<EnemyShipDatabaseSO>();
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
            float buttonAreaHeight = EditorGUIUtility.singleLineHeight + 6f;
            float scrollHeight     = position.height - buttonAreaHeight - 30f;
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(Mathf.Max(scrollHeight, 40f)));
            DrawList();
            EditorGUILayout.EndScrollView();

            DrawListButtons();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawList()
    {
        var list = _db.Ships;
        for (int i = 0; i < list.Count; i++)
        {
            var so    = list[i];
            string label  = so != null ? so.name : "(null)";
            bool   selected = i == _selectedIdx;
            var    style    = selected
                ? new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.cyan } }
                : EditorStyles.label;

            if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true)))
            {
                _selectedIdx  = i;
                _cachedEditor = null;
            }
        }
    }

    private void DrawListButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add"))
            AddNew();
        GUI.enabled = _selectedIdx >= 0 && _selectedIdx < _db.Ships.Count;
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
            var list = _db.Ships;
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
        var newSO    = CreateInstance<EnemyShipSO>();
        string fileName = $"NewEnemyShip_{DateTime.Now:yyyyMMdd_HHmmss}.asset";
        string path     = Path.Combine(SAVE_ROOT, fileName).Replace('\\', '/');

        EnsureDirectory(SAVE_ROOT);
        AssetDatabase.CreateAsset(newSO, path);

        _db.Add(newSO);
        EditorUtility.SetDirty(_db);
        AssetDatabase.SaveAssets();

        _selectedIdx  = _db.Ships.Count - 1;
        _cachedEditor = null;
        Repaint();
    }

    private void DeleteSelected()
    {
        var list = _db.Ships;
        if (_selectedIdx < 0 || _selectedIdx >= list.Count) return;

        var so = list[_selectedIdx];
        _db.Remove(so);
        EditorUtility.SetDirty(_db);

        if (so != null)
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(so));

        AssetDatabase.SaveAssets();
        _selectedIdx  = Mathf.Clamp(_selectedIdx - 1, -1, list.Count - 1);
        _cachedEditor = null;
        Repaint();
    }

    // ── Title 변경 시 Asset 자동 리네임 ──────────────────────────────────
    private void TryRenameByTitle(EnemyShipSO so)
    {
        var sp = new SerializedObject(so).FindProperty("Title");
        if (sp == null || string.IsNullOrWhiteSpace(sp.stringValue)) return;

        string newName = $"EnemyShip_{sp.stringValue}";
        string path    = AssetDatabase.GetAssetPath(so);
        if (string.IsNullOrEmpty(path)) return;
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
