using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Zenvin.VisualDebugging {
	public class ValueDebugger : MonoBehaviour {

		public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo ("en-US") ?? CultureInfo.CurrentCulture;

		private static ValueDebugger debugger;
		private static int referenceID = 0;

		private readonly Dictionary<int, DebugTarget> debugTargets = new Dictionary<int, DebugTarget> ();
		private readonly List<(string Label, string Value)> targetValues = new List<(string Label, string Value)> ();

		private Coroutine coroutine;
		private GUIStyle labelStyle;
		private GUIStyle valueStyle;


		private float updateInterval = 0f;
		private float spacing = 5f;
		private float margin = 5f;
		private Vector2 cellSize = new Vector2 (150, 40);


		public static bool Enabled {
			get => debugger != null && debugger.enabled;
			set {
				if (debugger != null) {
					debugger.enabled = value;
				}
			}
		}

		public static Vector2 CellSize {
			get {
				if (debugger == null) {
					return Vector2.zero;
				}
				return debugger.cellSize;
			}
			set {
				if (debugger != null) {
					value.x = Mathf.Max (value.x, 10);
					value.y = Mathf.Max (value.y, 40);
					debugger.cellSize = value;
				}
			}
		}

		public static float Spacing {
			get {
				if (debugger == null) {
					return 0f;
				}
				return debugger.spacing;
			}
			set {
				if (debugger != null) {
					debugger.spacing = Mathf.Max (0f, value);
				}
			}
		}

		public static float Margin {
			get {
				if (debugger == null) {
					return 0f;
				}
				return debugger.margin;
			}
			set {
				if (debugger != null) {
					debugger.margin = Mathf.Max (0f, value);
				}
			}
		}


		public static bool SetUpdateInterval (float interval) {
			if (interval <= 0f) {
				return false;
			}
			Initialize ();
			debugger.SetUpdateIntervalInternal (interval);
			return true;
		}

		public static int RegisterTarget (DebugTarget target) {
			if (!target.Valid) {
				return -1;
			}
			Initialize ();
			debugger.debugTargets[referenceID] = target;
			return referenceID++;
		}

		public static void UnregisterTarget (int id) {
			debugger?.debugTargets?.Remove (id);
		}



		private void OnGUI () {
			if (!Debug.isDebugBuild) {
				return;
			}

			if (labelStyle == null || valueStyle == null) {
				SetupStyles ();
			}

			Rect screen = new Rect (margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);
			float cellWidth = (screen.width - spacing * cellSize.x) / cellSize.x;
			float cellHeight = (screen.height - spacing * cellSize.y) / cellSize.y;

			Vector2Int pos = Vector2Int.zero;
			for (int i = 0; i < targetValues.Count; i++) {
				Rect cell = GetCellRect (pos);
				if (cell.y + cell.height > screen.y + screen.height) {
					pos.x++;
					pos.y = 0;
					cell = GetCellRect (pos);
				}

				DrawCell (cell, i);
				pos.y++;
			}
		}

		private void DrawCell (Rect rect, int index) {
			const float frame = 3f;
			const float header = 15f;
			var target = targetValues[index];

			GUI.Box (rect, string.Empty);

			rect.x += frame;
			rect.width -= frame * 2f;
			rect.y += frame;
			rect.height -= frame * 2f;

			Rect labelRect = new Rect (rect);
			labelRect.height = header;

			Rect valueRect = new Rect (rect);
			valueRect.y += header;
			valueRect.height -= header;

			GUI.Label (labelRect, target.Label, labelStyle);
			GUI.Label (valueRect, target.Value, valueStyle);
		}

		private Rect GetCellRect (Vector2Int pos) {
			return new Rect (
				margin + ((cellSize.x + spacing) * pos.x),
				margin + ((cellSize.y + spacing) * pos.y),
				cellSize.x,
				cellSize.y
			);
		}


		private static void Initialize () {
			if (debugger != null) {
				return;
			}
			GameObject obj = new GameObject ("[GUI Debugger]");
			obj.hideFlags = HideFlags.HideInHierarchy;
			DontDestroyOnLoad (obj);

			debugger = obj.AddComponent<ValueDebugger> ();
			//debugger.SetupStyles ();
			debugger.SetUpdateIntervalInternal (0.25f);
		}

		private void SetupStyles () {
			valueStyle = new GUIStyle (GUI.skin.label) {
				fontSize = 14,
				alignment = TextAnchor.MiddleRight,
				margin = new RectOffset (),
				padding = new RectOffset ()
			};
			labelStyle = new GUIStyle (GUI.skin.label) {
				fontSize = 11,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft,
				margin = new RectOffset (),
				padding = new RectOffset ()
			};
		}

		private void SetUpdateIntervalInternal (float interval) {
			if (Mathf.Approximately (interval, updateInterval)) {
				return;
			}
			if (coroutine != null) {
				StopCoroutine (coroutine);
			}
			updateInterval = interval;
			coroutine = StartCoroutine (UpdateValues (interval));
		}

		private IEnumerator UpdateValues (float interval) {
			WaitForSecondsRealtime wfs = new WaitForSecondsRealtime (interval);
			while (true) {
				targetValues.Clear ();
				List<DebugTarget> targets = new List<DebugTarget> (debugTargets.Values);

				foreach (var dt in targets) {
					if (dt.Valid) {
						targetValues.Add ((dt.Label, dt.Value));
					}
				}

				yield return wfs;
			}
		}


	}

	public struct DebugTarget {
		private readonly Func<string> valueCallback;

		public readonly string Label;

		public bool Valid => !string.IsNullOrEmpty (Label) && valueCallback != null;
		public string Value => valueCallback == null ? "" : valueCallback.Invoke ();

		public DebugTarget (Func<string> valueCallback, string label) {
			this.valueCallback = valueCallback;
			Label = label;
		}
	}
}