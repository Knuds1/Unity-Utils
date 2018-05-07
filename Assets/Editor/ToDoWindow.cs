using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ToDoWindow : EditorWindow {

	private Vector2 scrollPos = Vector2.zero;
	private Dictionary<string, List<ToDoItem>> todos = new Dictionary<string, List<ToDoItem>>();

	struct ToDoItem {
		public int line;
		public string text;
		public bool important;
		public MonoScript script;
	}

	private void OnEnable() {
		RefreshAll();
	}

	[MenuItem("Window/To-Do")]
	private static void Init() {
		ToDoWindow window = EditorWindow.GetWindow<ToDoWindow>(false, "To-Do");
        window.Show();
	}

	private void RefreshAll() {
		string[] assetPaths = AssetDatabase.GetAllAssetPaths();

		foreach(string assetPath in assetPaths)
		{
			if (assetPath.EndsWith (".cs") || assetPath.EndsWith (".js")) {
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
				if (script != null) {
					//Debug.Log("found script: " + script.name);
					RefreshScript(script);
				}
			}
		}
	}

	private void RefreshScript(MonoScript script) {
		/*
		if (!Regex.IsMatch(script.text, @"(?im)\/\/\s*todo:?\s*(.*)")) {
			todos.Remove(script.name);
			return;
		}

		using (StringReader reader = new StringReader(script.text)) {
			string line;
			while ((line = reader.ReadLine()) != null) {
				
			}
		}
		*/


		MatchCollection matches = Regex.Matches(script.text, @"(?im)\/\/\s*todo:(.*)");
		if (matches.Count == 0) {
			todos.Remove(script.name);
			return;
		}

		List<ToDoItem> items = new List<ToDoItem>();
		foreach (Match match in matches) {
			ToDoItem todo = new ToDoItem();
			string text = match.Groups[1].Value.Trim();
			todo.important = text[0] == '!';
			if (todo.important) {
				text = text.Substring(1, text.Length - 1).TrimStart();
			}
			todo.text = text;
			todo.script = script;
			todo.line = script.text.Take(match.Index).Count(c => c == '\n') + 1;
			items.Add(todo);
		}
		todos[script.name] = items;
	}

	private void OnGUI() {
		DoToolbarGUI();

		scrollPos = GUILayout.BeginScrollView (scrollPos);

		foreach (KeyValuePair<string, List<ToDoItem>> pair in todos) {

			GUILayout.Label (pair.Key, EditorStyles.boldLabel);

			List<ToDoItem> items = pair.Value;
			foreach (ToDoItem todo in items) {
				GUILayout.BeginHorizontal();
				if (todo.important) {
					GUI.backgroundColor = Color.yellow;
				} else {
					GUI.backgroundColor = Color.red;
				}
				if (GUILayout.Button (todo.text, EditorStyles.label)) {
					AssetDatabase.OpenAsset (todo.script, todo.line);
				}
				GUILayout.EndHorizontal();
			}
		}

		if (todos.Count == 0) {
			GUILayout.Box ("You are all set. Great work!", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		}

		GUILayout.EndScrollView();
	}

	void DoToolbarGUI() {
		GUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[0]);
		if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, new GUILayoutOption[0])) {
			RefreshAll();
		}
		GUILayout.EndHorizontal();
	}
}
