using System.Collections;
using OpenTerra.Commands;
using OpenTerra.Controllers.Caching;
using OpenTerra.DataModel;
using OpenTerra.DataModel.Features;
using OpenTerra.ImportExport;
using OpenTerra.Plugins;
using OpenTerra.Settings;
using OpenTerra.Unity;
using OpenTerra.Unity.Compositing;
using OpenTerra.Unity.Geometry;
using OpenTerra.Unity.Patches;
using OpenTerra.Unity.SpatialStructures;
using OpenTerra.Unity.Terrain;
using OpenTerra.Unity.UI;
using OpenTerra.Unity.Views;
using OpenTerra.Unity.Workers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenTerra.Controllers
{
	public class Bootstrapper : MonoBehaviour
	{
		private IShell shell;
		private ICache cache;
		private ISettingProvider settingProvider;
		private IMeshBuilder meshBuilder;
		private ITerrainManager terrainManager;
		private IGlobe globe;
		private IPatchManager patchManager;
		private ICompositer compositer;
		private IViewpointController viewpointController;
		private IPluginManager pluginManager;
		private QuadTree quadTree;
		private IImportManager importManager;
		private IGeometryManager geometryManager;
		private IFeatureManager featureManager;

		public Gradient ElevationColorRamp;
		public int[] ScenesToLoad;

		public void Start()
		{
			Debug.Log("Starting bootstrapper...");

			StartCoroutine(Startup());
		}

		private IEnumerator Startup()
		{
			foreach (var scene in ScenesToLoad)
			{
				SceneManager.LoadScene(scene, LoadSceneMode.Additive);
				yield return new WaitForEndOfFrame();
			}

			shell = new Shell();
			pluginManager = new PluginManager();
			settingProvider = new SettingProvider();
			cache = new Cache(shell, settingProvider);
			importManager = new ImportManager(shell, pluginManager, cache);
			globe = new Globe(new WGS84(), 0.001f, shell);
			viewpointController = new ViewpointController(shell, globe);
			terrainManager = new TerrainManager(cache, viewpointController);
			quadTree = new QuadTree(globe, viewpointController);
			featureManager = new FeatureManager(importManager);
			geometryManager = new GeometryManager(featureManager, globe);
			meshBuilder = new MeshBuilder(settingProvider, globe, quadTree, terrainManager, ElevationColorRamp);

			patchManager = new PatchManager(shell, terrainManager, meshBuilder, quadTree);
			compositer = new Compositer(globe, quadTree, shell, cache, settingProvider, patchManager, viewpointController);

			GameObject.Find("UI").GetComponent<UiController>().Initialize(shell);

			quadTree.Update();

			yield return new WaitForEndOfFrame();

			compositer.Initialize();

			Ready = true;
		}

		public bool Ready { get; private set; }

		public void Update()
		{
			if (!Ready)
				return;

			cache.Update();
			quadTree.Update();
			meshBuilder.Update();
			patchManager.Update();
			compositer.Update();
		}

		public void OnDrawGizmos()
		{
			if (!Ready)
				return;

			globe.OnDrawGizmos();
			viewpointController.OnDrawGizmos();
			quadTree.OnDrawGizmos();
		}
	}
}
