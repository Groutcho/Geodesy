using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenTerra.Controllers
{
	public class Bootstrapper : MonoBehaviour
	{
		public Gradient ElevationColorRamp;
		public int[] ScenesToLoad;

		private MainController mainController;

		public void Start()
		{
			mainController = new MainController(ElevationColorRamp);

			StartCoroutine(Startup());
		}

		private IEnumerator Startup()
		{
			foreach (var scene in ScenesToLoad)
			{
				SceneManager.LoadScene(scene, LoadSceneMode.Additive);
				yield return new WaitForEndOfFrame();
			}

			StartCoroutine(mainController.Startup());
		}

		public void Update()
		{
			if (!mainController.Ready)
				return;

			mainController.Update();
		}

		public void OnDrawGizmos()
		{
			if (mainController == null || !mainController.Ready)
				return;

			mainController.OnDrawGizmos();
		}
	}
}
