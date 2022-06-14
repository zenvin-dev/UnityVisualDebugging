using UnityEngine;

namespace Zenvin.VisualDebugging {
	[DefaultExecutionOrder(-100)]
	public class DebugBehaviour : MonoBehaviour {
		private void Update () {
			VisualDebugger.Update ();
		}
	}
}