using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.VisualDebugging {
	public static class VisualDebugger {

		private const string LineMaterialShader = "Zenvin/DebugShader";
		private const string LineMaterialShaderFallback = "GUI/Text Shader";

		private static readonly List<LineRenderer> pool = new List<LineRenderer> (32);
		private static int position = 0;


		private static DebugBehaviour behaviour;
		private static DebugBehaviour Behaviour {
			get {
				if (behaviour == null) {
					SetupBehaviour ();
				}
				return behaviour;
			}
		}

		private static Material material;
		private static Material LineMaterial {
			get {
				if (material == null) {
					SetupMaterial ();
				}
				return material;
			}
		}

		private static LineRenderer prefab;
		private static LineRenderer Prefab {
			get {
				if (prefab == null) {
					SetupPrefab ();
				}
				return prefab;
			}
		}


		private static int poolSizeLimit = 128;
		public static int PoolSizeLimit {
			get {
				return poolSizeLimit;
			}
			set {
				if (value < 0) {
					value = 0;
				}
				poolSizeLimit = value;
			}
		}

		public static int PoolSize {
			get {
				return pool.Count;
			}
		}

		public static float LineWidth { get; set; } = 0.02f;

		public static Color DefaultColor { get; set; } = Color.white;


		// ----------- Debug Methods ---------------------------------------------------------------

		// Draw RAY
		public static void DrawRay (Vector3 start, Vector3 direction, bool depth = false) {
			DrawRay (start, direction, DefaultColor, depth);
		}

		public static void DrawRay (Vector3 start, Vector3 direction, Color color, bool depth = false) {
			DrawLine (start, start + direction, color, depth);
		}

		// Draw PATH
		public static void DrawPath (params Vector3[] points) {
			DrawPath (false, false, DefaultColor, points);
		}

		public static void DrawPath (bool depth, params Vector3[] points) {
			DrawPath (depth, false, DefaultColor, points);
		}

		public static void DrawPath (bool depth, bool loop, params Vector3[] points) {
			DrawPath (depth, loop, DefaultColor, points);
		}

		public static void DrawPath (bool depth, bool loop, Color color, params Vector3[] points) {
			var lr = GetNextRenderer ();
			lr.positionCount = points.Length;
			lr.SetPositions (points);
			EnableRendererWithProperties (lr, color, depth, loop);
		}

		// Draw RECT

		public static void DrawRectangle (Vector3 position, Vector2 dimensions) {
			DrawRectangle (position, dimensions, Quaternion.identity, DefaultColor);
		}

		public static void DrawRectangle (Vector3 position, Vector2 dimensions, Color color) {
			DrawRectangle (position, dimensions, Quaternion.identity, color);
		}

		public static void DrawRectangle (Vector3 position, Vector2 dimensions, Quaternion rotation) {
			DrawRectangle (position, dimensions, rotation, DefaultColor);
		}

		public static void DrawRectangle (Vector3 position, Vector2 dimensions, Quaternion rotation, Color color, bool depth = false) {
			Vector3 topRgt = rotation * new Vector3 (dimensions.x, dimensions.y, 0f);
			Vector3 topLft = rotation * new Vector3 (-dimensions.x, dimensions.y, 0f);
			Vector3 btmRgt = rotation * new Vector3 (dimensions.x, -dimensions.y, 0f);
			Vector3 btmLft = rotation * new Vector3 (-dimensions.x, -dimensions.y, 0f);

			DrawPath (depth, true, color, position + topRgt, position + topLft, position + btmLft, position + btmRgt);
		}

		// Draw CIRCLE

		public static void DrawCircle (Vector3 position, Quaternion rotation, float radius, int vertexCount) {
			DrawCircle (position, rotation, radius, vertexCount, DefaultColor, false);
		}

		public static void DrawCircle (Vector3 position, Quaternion rotation, float radius, int vertexCount, Color color, bool depth) {
			if (radius == 0f || vertexCount < 3) {
				return;
			}
			
			radius = Mathf.Abs (radius);

			Vector3[] points = new Vector3[vertexCount];
			float frac = 360f / vertexCount;

			for (int i = 0; i < vertexCount; i++) {
				points[i] = position + (rotation * ((Quaternion.Euler (Vector3.forward * (frac * i))) * Vector3.up)) * radius;
			}

			DrawPath (depth, true, color, points);
		}

		// Draw LINE

		public static void DrawLine (Vector3 start, Vector3 end, bool depth = false) {
			DrawLine (start, end, DefaultColor, depth);
		}

		public static void DrawLine (Vector3 start, Vector3 end, Color color, bool depth = false) {
			var lr = GetNextRenderer ();
			lr.positionCount = 2;
			lr.SetPositions (new Vector3[] { start, end });
			EnableRendererWithProperties (lr, color, depth, false);
		}

		// Draw SPHERE

		public static void DrawSphere (Vector3 position, float radius, Color color, bool depth = false) {
			DrawCircle (position, Quaternion.identity, radius, 32, color, depth);
			DrawCircle (position, Quaternion.Euler (90f, 0f, 0f), radius, 32, color, depth);
			DrawCircle (position, Quaternion.Euler (0f, 90f, 90f), radius, 32, color, depth);
		}



		// ----------- Updating & Resetting --------------------------------------------------------

		internal static void Update () {
			Reset ();
		}

		private static void Reset () {
			position = 0;
			for (int i = 0; i < pool.Count; i++) {
				pool[i].gameObject.SetActive (false);
			}
		}


		// ----------- Utility Methods -------------------------------------------------------------

		private static LineRenderer GetNextRenderer () {
			if (position >= pool.Count) {
				ExpandPool ((position - pool.Count) + 1);
			}
			position++;
			return pool[position - 1];
		}

		private static void EnableRendererWithProperties (LineRenderer lr, Color color, bool depth, bool loop) {
			MaterialPropertyBlock block = new MaterialPropertyBlock ();
			block.SetColor ("_Color", color);
			block.SetFloat ("_ZWrite", depth ? 1f : 0f);
			lr.SetPropertyBlock (block);
			lr.widthMultiplier = LineWidth;
			lr.loop = loop;

			lr.gameObject.SetActive (true);
		}


		// ----------- Setup Methods ---------------------------------------------------------------

		private static void SetupBehaviour () {
			GameObject go = new GameObject ("[Visual Debug Behaviour]");
			//go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			behaviour = go.AddComponent<DebugBehaviour> ();
		}

		private static void SetupMaterial () {
			Shader shader = Shader.Find (LineMaterialShader) ?? Shader.Find (LineMaterialShaderFallback);
			Material mat = new Material (shader);
			material = mat;
		}

		private static void SetupPrefab () {
			GameObject go = new GameObject ("Debug Line");
			go.transform.SetParent (Behaviour.transform);
			go.SetActive (false);

			LineRenderer lr = go.AddComponent<LineRenderer> ();
			lr.material = LineMaterial;
			lr.useWorldSpace = true;
			lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			prefab = lr;
		}

		private static void ExpandPool (int amount = 1) {
			if (amount < 0) {
				return;
			}
			for (int i = 0; i < amount; i++) {
				if (PoolSizeLimit <= 0 || pool.Count < PoolSizeLimit) {
					pool.Add (Object.Instantiate (Prefab, Behaviour.transform));
				}
			}
		}

	}
}