using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Zenvin.VisualDebugging {
	[DisallowMultipleComponent]
	public class ValueDebugger : MonoBehaviour {

		public enum TargetVisibilityOption {
			/// <summary> Global debug targets are always shown. </summary>
			Always,
			/// <summary> Global debug targets are only shown when contextualized ones are not. </summary>
			WithoutContextOnly,
			/// <summary> Global debug targets are only shown when contextualized ones are as well. </summary>
			WithContextOnly,
		}

		/// <summary>
		/// Helper for calling <c>ToString()</c> on debugged values, to make them appear uniform.
		/// </summary>
		public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo ("en-US") ?? CultureInfo.CurrentCulture;

		private static ValueDebugger debugger;
		private static int referenceID = 0;

		private readonly Dictionary<int, DebugTarget> debugTargets = new Dictionary<int, DebugTarget> ();
		private readonly List<DebugTargetValue> targetValues = new List<DebugTargetValue> ();

		private Coroutine coroutine;
		private GUIStyle labelStyle;
		private GUIStyle valueStyle;

		private float updateInterval = 0f;
		private float spacing = 5f;
		private float margin = 5f;
		private Vector2 cellSize = new Vector2 (150, 40);


		/// <summary>
		/// The current instance of the debugger, if any.
		/// </summary>
		public static ValueDebugger Instance => debugger;
		/// <summary>
		/// Whether debug targets should be drawn.
		/// </summary>
		public bool Enabled { get; set; }
		/// <summary>
		/// When global debug targets should be drawn.
		/// </summary>
		public TargetVisibilityOption GlobalTargetVisibility { get; set; }
		/// <summary>
		/// If true, all contextualized debug targets are shown when no context is set.
		/// </summary>
		public bool ShowContextualizedAsGlobal { get; set; }
		/// <summary>
		/// The currently selected context.
		/// </summary>
		public GameObject CurrentContext { get; set; }
		/// <summary>
		/// The size of each drawn value's cell on screen.
		/// </summary>
		public Vector2 CellSize {
			get {
				return cellSize;
			}
			set {
				value.x = Mathf.Max (value.x, 10);
				value.y = Mathf.Max (value.y, 40);
				cellSize = value;
			}
		}
		/// <summary>
		/// The spacing between value cells. Cannot be less than 0.
		/// </summary>
		public float Spacing {
			get {
				return spacing;
			}
			set {
				spacing = Mathf.Max (0f, value);
			}
		}
		/// <summary>
		/// The margin around value cells. Cannot be less than 0.
		/// </summary>
		public float Margin {
			get {
				return margin;
			}
			set {
				margin = Mathf.Max (0f, value);
			}
		}
		/// <summary>
		/// The current interval at which debug targets' values are updated.
		/// If the value is 0 or less, updates will happen every frame.<br></br>
		/// Use <see cref="SetUpdateInterval(float)"/> to change the interval.
		/// </summary>
		public float UpdateInterval => updateInterval;


		/// <summary>
		/// Creates a new <see cref="ValueDebugger"/> instance if necessary and updates the current instance.
		/// </summary>
		/// <returns> The current instance of the debugger. </returns>
		public static ValueDebugger GetOrCreateInstance () {
			Initialize ();
			return debugger;
		}

		/// <summary>
		/// Sets the interval with which target values will be updated.
		/// Setting the <paramref name="interval"/> to 0 or less will cause updates to happen every frame.
		/// </summary>
		public void SetUpdateInterval (float interval) {
			SetUpdateIntervalInternal (interval);
		}
		/// <summary>
		/// Registers a new <see cref="DebugTarget"/> to be drawn as a value on screen.
		/// </summary>
		/// <returns> A handle by which the added target may be removed, or -1 if the target could not be added. </returns>
		public int RegisterTarget (DebugTarget target) {
			if (!target.Valid) {
				return -1;
			}
			debugTargets[referenceID] = target;
			return referenceID++;
		}
		/// <summary>
		/// Unregisters a <see cref="DebugTarget"/> from the debugger, using its handle (see <see cref="RegisterTarget(DebugTarget)"/>).
		/// </summary>
		public void UnregisterTarget (int handle) {
			if (handle < 0) {
				return;
			}
			debugTargets?.Remove (handle);
		}


		private void Start () {
			if (debugger != null)
				return;

			debugger = this;
			SetUpdateIntervalInternal (0.25f);
		}

		private void OnGUI () {
			if (labelStyle == null || valueStyle == null) {
				SetupStyles ();
			}

			Rect screen = new Rect (margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);
			Vector2Int pos = Vector2Int.zero;

			for (int i = 0; i < targetValues.Count; i++) {
				var target = targetValues[i];
				if (!GetCellIsVisible (target.Context)) {
					continue;
				}

				Rect cell = GetCellRect (pos);
				if (cell.y + cell.height > screen.y + screen.height) {
					pos.x++;
					pos.y = 0;
					cell = GetCellRect (pos);
				}

				DrawCell (cell, ref target);
				pos.y++;
			}
		}


		private void DrawCell (Rect rect, ref DebugTargetValue target) {
			const float frame = 3f;
			const float header = 15f;

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

		private bool GetCellIsVisible (GameObject cellContext) {
			if (cellContext != null) {
				return ShowContextualizedAsGlobal || cellContext == CurrentContext;
			}

			switch (GlobalTargetVisibility) {
				case TargetVisibilityOption.Always:
					return true;
				case TargetVisibilityOption.WithoutContextOnly:
					return cellContext == null;
				case TargetVisibilityOption.WithContextOnly:
					return cellContext != null;
				default:
					return true;
			}
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
			coroutine = StartCoroutine (CoroutineUpdateValues (interval));
		}

		private IEnumerator CoroutineUpdateValues (float interval) {
			WaitForSecondsRealtime wfs = interval <= 0f ? null : new WaitForSecondsRealtime (interval);
			while (true) {
				UpdateValues ();
				yield return wfs;
			}
		}

		private void UpdateValues () {
			targetValues.Clear ();

			foreach (var dt in debugTargets.Values) {
				if (dt.Valid) {
					targetValues.Add (new DebugTargetValue (dt.Label, dt.Value, dt.Context));
				}
			}
		}
	}

	public struct DebugTarget {
		private readonly Func<string> valueCallback;

		public readonly string Label;
		public readonly GameObject Context;


		public bool Valid => valueCallback != null && !string.IsNullOrEmpty (Label);
		public string Value => valueCallback?.Invoke () ?? "";


		public DebugTarget (Func<string> valueCallback, string label, GameObject context) {
			this.valueCallback = valueCallback;
			Label = label;
			Context = context;
		}

		public DebugTarget (Func<string> valueCallback, string label, Component context) : this (valueCallback, label, context != null ? context.gameObject : null) { }

		public DebugTarget (Func<string> valueCallback, string label) : this (valueCallback, label, (GameObject)null) { }
	}

	internal struct DebugTargetValue {
		public readonly string Label;
		public readonly string Value;
		public readonly GameObject Context;


		public DebugTargetValue (string label, string value, GameObject context) {
			Label = label;
			Value = value;
			Context = context;
		}
	}
}
