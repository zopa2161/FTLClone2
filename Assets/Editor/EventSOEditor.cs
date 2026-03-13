using System.Collections.Generic;
using Core.Data.Event;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventSO))]
public class EventSOEditor : Editor
{
    private static readonly Color ColorDialog = new Color(0.4f, 0.7f, 1f);
    private static readonly Color ColorCombat = new Color(1f, 0.4f, 0.4f);
    private static readonly Color ColorReward = new Color(0.4f, 1f, 0.5f);
    private static readonly Color ColorEnd    = new Color(0.7f, 0.7f, 0.7f);

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var ev = (EventSO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("── 이벤트 체인 미리보기 ──", EditorStyles.boldLabel);

        if (ev.StartEvent == null)
        {
            EditorGUILayout.HelpBox("StartEvent가 비어 있습니다.", MessageType.Info);
            return;
        }

        var visited = new HashSet<SubEventBaseSO>();
        DrawSubEvent(ev.StartEvent, 0, visited);
    }

    private void DrawSubEvent(SubEventBaseSO node, int depth, HashSet<SubEventBaseSO> visited)
    {
        if (node == null)
        {
            DrawNode(depth, "[null]", ColorEnd);
            return;
        }

        if (node.IsFinished)
        {
            DrawNode(depth, $"[END] {node.name}", ColorEnd);
            return;
        }

        if (visited.Contains(node))
        {
            DrawNode(depth, $"[순환 참조] {node.name}", Color.yellow);
            return;
        }

        visited.Add(node);

        switch (node)
        {
            case DialogSubEventSO dialog:
                DrawNode(depth, $"[Dialog] {node.name}  \"{Truncate(dialog.DialogText, 30)}\"", ColorDialog);
                if (dialog.Choices != null)
                {
                    foreach (var choice in dialog.Choices)
                    {
                        DrawNode(depth + 1, $"▶ \"{choice.ChoiceText}\"", ColorDialog * 0.85f);
                        DrawSubEvent(choice.NextEvent, depth + 2, visited);
                    }
                }
                break;

            case CombatSubEventSO combat:
                string enemy = combat.EnemyShip != null ? combat.EnemyShip.name : "(미설정)";
                DrawNode(depth, $"[Combat] {node.name}  적: {enemy}", ColorCombat);
                DrawSubEvent(combat.NextEvent, depth + 1, visited);
                break;

            case RewardSubEventSO reward:
                string rewards = reward.Rewards != null && reward.Rewards.Count > 0
                    ? string.Join(", ", reward.Rewards.ConvertAll(r => $"{r.Type} {(r.Amount >= 0 ? "+" : "")}{r.Amount}"))
                    : "(보상 없음)";
                DrawNode(depth, $"[Reward] {node.name}  {rewards}", ColorReward);
                DrawSubEvent(reward.NextEvent, depth + 1, visited);
                break;

            default:
                DrawNode(depth, $"[Unknown] {node.name}", ColorEnd);
                break;
        }

        visited.Remove(node);
    }

    private static void DrawNode(int depth, string label, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        EditorGUILayout.LabelField(new string(' ', depth * 4) + label, EditorStyles.helpBox);
        GUI.color = prev;
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
