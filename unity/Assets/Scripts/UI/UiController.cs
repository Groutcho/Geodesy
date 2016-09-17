using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace OpenTerra.Unity.UI
{
	public class UiController : MonoBehaviour
	{
		private static UiController instance;

		public static UiController Instance { get { return instance; } }

		private bool initialized;

		#region UI elements

		Image introBg;
		RawImage introTitle;
		Terminal terminal;

		private void CollectUIElements ()
		{
			introTitle = transform.Find ("introScreen/terra").GetComponent<RawImage> ();
			introBg = transform.Find ("introScreen").GetComponent<Image> ();

			terminal = GetComponent<Terminal>();
		}

		#endregion

		private void Awake ()
		{
			instance = this;
			CollectUIElements ();
		}

		private void Start ()
		{
			#if !UNITY_EDITOR
			StartCoroutine (HideIntroScreen ());
			#endif
		}

		private IEnumerator HideIntroScreen ()
		{
			introTitle.enabled = true;
			introBg.enabled = true;

			yield return new WaitForSeconds (1);

			float t = 1;

			while (t > 0)
			{
				Color w = Color.white;
				w.a = t;
				introTitle.color = w;
				introBg.color = w;
				yield return new WaitForEndOfFrame ();
				t -= Time.deltaTime;
			}
		}

		/// <summary>
		/// The UI controller remains inactive until is it initialized.
		/// </summary>
		/// <param name="globe">The globe instance.</param>
		public void Initialize (IShell shell)
		{
			initialized = true;
			terminal.Initialize(shell);
		}

		private void Update ()
		{
			if (!initialized)
				return;
		}
	}
}
