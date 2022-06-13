using System.Collections;
using UnityEngine;

namespace Zenvin.VisualDebugging {
	public class DebugBehaviour : MonoBehaviour {

		private void Start () {
			StartCoroutine (DisableDebugVisuals());
		}

		private void LateUpdate () {
			VisualDebugger.Update ();
		}


		private IEnumerator DisableDebugVisuals () {
			var wait = new WaitForEndOfFrame ();

			while (true) {
				VisualDebugger.Reset ();
				yield return wait;
			}
		}

	}
}