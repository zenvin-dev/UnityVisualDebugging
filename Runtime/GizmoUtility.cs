using UnityEngine;

namespace Zenvin.VisualDebugging {
	public static class GizmoUtility {

		public static void DrawWireCircle (Vector3 position, Quaternion rotation, float radius, int segments, Color color) {
			Color col = Gizmos.color;
			Gizmos.color = color;

			if (segments < 3) {
				segments = 3;
			}

			Vector3 firstPoint = position + (rotation * Vector3.forward * radius);
			Vector3 lastPoint = firstPoint;

			float frac = 360f / segments;
			for (int i = 1; i < segments; i++) {
				Vector3 point = position + ((Quaternion.Euler (Vector3.up * (frac * i)) * rotation) * Vector3.forward) * radius;
				Gizmos.DrawLine (lastPoint, point);
				lastPoint = point;
			}
			Gizmos.DrawLine (lastPoint, firstPoint);

			Gizmos.color = col;
		}

		public static void DrawWireAngle (Vector3 position, Quaternion rotation, float angle, float radius, int segments, Color color) {

			Color col = Gizmos.color;
			Gizmos.color = color;

			angle = Mathf.Clamp (Mathf.Abs(angle), 0f, 360f);

			if (angle == 0f) {
				Gizmos.DrawRay (position, rotation * Vector3.forward * radius);
				Gizmos.color = col;
				return;
			}
			if (angle == 360f) {
				DrawWireCircle (position, rotation, radius, segments, color);
				Gizmos.color = col;
				return;
			}

			segments = (int)(segments * (angle / 360f));

			if (segments < 2) {
				segments = 2;
			}

			Vector3 lastPoint = (rotation * Quaternion.Euler (Vector3.up * -angle * 0.5f) * Vector3.forward) * radius + position;
			Vector3 startPoint = lastPoint;

			float frac = angle / segments;
			for (int i = 1; i < segments + 1; i++) {

				Vector3 point = (rotation * Quaternion.Euler (Vector3.up * (-angle * 0.5f + frac * i)) * Vector3.forward) * radius + position;

				Gizmos.DrawLine (lastPoint, point);
				lastPoint = point;
			}

			Gizmos.DrawLine (position, startPoint);
			Gizmos.DrawLine (position, lastPoint);

			Gizmos.color = col;
		}

		public static void DrawLine (Vector3 from, Vector3 to, Color color) {
			Color col = Gizmos.color;
			Gizmos.color = color;

			Gizmos.DrawLine (from, to);

			Gizmos.color = col;
		}

		public static void DrawRay (Vector3 from, Vector3 direction, Color color) {
			Color col = Gizmos.color;
			Gizmos.color = color;

			Gizmos.DrawRay (from, direction);

			Gizmos.color = col;
		}

		public static void DrawPointLines (Vector3 point, Color color, float size = 1f) {
			Color col = Gizmos.color;
			Gizmos.color = color;

			Gizmos.DrawLine (point + Vector3.right * size, point + Vector3.left * size);
			Gizmos.DrawLine (point + Vector3.up * size, point + Vector3.down * size);
			Gizmos.DrawLine (point + Vector3.forward * size, point + Vector3.back * size);
			
			Gizmos.color = col;
		}

		public static void DrawArrow (Vector3 from, Vector3 to, Vector3 normal, float size, Color color) {

			Vector3 dir = to - from;
			Quaternion rot = Quaternion.LookRotation (dir, normal);

			DrawLine (from, to, color);

			size *= dir.magnitude;
			DrawLine (to, (to + rot * (Vector3.back + Vector3.right) * size), color);
			DrawLine (to, (to + rot * (Vector3.back + Vector3.left) * size), color);

		}

		public static void DrawRect (Vector3 position, Vector2 size, Quaternion rotation, Color color) {
			size *= 0.5f;

			var p0 = new Vector3 (+size.x, 0f, +size.y);	// top rgt
			var p1 = new Vector3 (+size.x, 0f, -size.y);	// btm rgt
			var p2 = new Vector3 (-size.x, 0f, +size.y);	// top lft
			var p3 = new Vector3 (-size.x, 0f, -size.y);	// btm lft

			p0 = rotation * p0;
			p1 = rotation * p1;
			p2 = rotation * p2;
			p3 = rotation * p3;

			p0 += position;
			p1 += position;
			p2 += position;
			p3 += position;

			DrawLine (p0, p1, color);	// top rgt - btm rgt
			DrawLine (p1, p3, color);	// btm rgt - btm lft
			DrawLine (p3, p2, color);	// btm lft - top lft
			DrawLine (p2, p0, color);	// top lft - top rgt
		}

	}
}