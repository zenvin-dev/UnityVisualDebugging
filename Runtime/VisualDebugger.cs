using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.VisualDebugging {
	public static class VisualDebugger {

		private static DebugBehaviour behaviour;
		private static DebugBehaviour Behaviour {
			get {
				if (behaviour == null) {
					SetupBehaviour ();
				}
				return behaviour;
			}
		}




		internal static void Update () {

		}

		internal static void Reset () {

		}


		private static void SetupBehaviour () {
			GameObject go = new GameObject ("[Visual Debug Behaviour]");
			go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			behaviour = go.AddComponent<DebugBehaviour> ();
		}

	}
}