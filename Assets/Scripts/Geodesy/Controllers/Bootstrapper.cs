using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenTerra.Controllers
{
	public class Bootstrapper : MonoBehaviour
	{
		public Gradient ElevationColorRamp;
		public int[] ScenesToLoad;

		private bool ready;

		private MainController mainController;

		public void Start()
		{
			StartCoroutine(Startup());
		}

		private IEnumerator Startup()
		{
			foreach (var scene in ScenesToLoad)
			{
				SceneManager.LoadScene(scene, LoadSceneMode.Additive);
				yield return new WaitForEndOfFrame();
			}

			mainController = new MainController(ElevationColorRamp);

			ready = true;
		}

		public void Update()
		{
			if (!ready)
				return;

			mainController.Update();
		}

		public void OnDrawGizmos()
		{
			if (!ready)
				return;

			mainController.OnDrawGizmos();
		}
	}
}
