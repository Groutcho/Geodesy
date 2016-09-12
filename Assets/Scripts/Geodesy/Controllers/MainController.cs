using OpenTerra.Controllers.Caching;
using OpenTerra.Controllers.Commands;
using OpenTerra.Controllers.Settings;
using OpenTerra.Controllers.Workers;
using OpenTerra.Models;
using OpenTerra.Models.QuadTree;
using OpenTerra.Views;
using UnityEngine;

namespace OpenTerra.Controllers
{
	public class MainController
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
		private QuadTree quadTree;

		public MainController(Gradient elevationColorRamp)
		{
			shell = new Shell();
			settingProvider = new SettingProvider();
			cache = new Cache(shell, settingProvider);
			globe = new Globe(new WGS84(), 0.001f, shell);
			viewpointController = new ViewpointController(shell, globe);
			terrainManager = new TerrainManager(cache, viewpointController);
			quadTree = new QuadTree(globe, viewpointController);
			meshBuilder = new MeshBuilder(settingProvider, globe, quadTree, terrainManager, elevationColorRamp);

			patchManager = new PatchManager(shell, terrainManager, meshBuilder, quadTree);
			compositer = new Compositer(globe, quadTree, shell, cache, settingProvider, patchManager, viewpointController);

			GameObject.Find("UI").GetComponent<UiController>().Initialize(shell);
		}

		public void Update()
		{
			cache.Update();
			quadTree.Update();
			meshBuilder.Update();
			patchManager.Update();
			compositer.Update();
		}

		public void OnDrawGizmos()
		{
			globe.OnDrawGizmos();
			viewpointController.OnDrawGizmos();
			quadTree.OnDrawGizmos();
		}
	}
}
