using UnityEngine;
using OpenTerra.DataModel.Features;
using System.Collections.Generic;
using System;

namespace OpenTerra.Unity.Geometries
{
	/// <summary>
	/// The Unity handler of all geometric features.
	/// </summary>
	public class GeometryManager : IGeometryManager
	{
		// The mapping between the features and their visual representation.
		private IDictionary<Feature, GameObject> featureMap;

		// All geometry GameObjects will be children of this transform.
		private Transform geometryRoot;

		private IGlobe globe;

		private GameObject pointPrefab;

		/// <summary>
		/// Create a new instance of the <see cref="GeometryManager"/> class using the provided <see cref="IFeatureManager"/> and <see cref="IGlobe"/> services.
		/// The <see cref="IGlobe"/> service is used to project the features on the globe, while the <see cref="IFeatureManager"/> provides the features.
		/// </summary>
		/// <param name="featureManager">The provider of features for this <see cref="GeometryManager"/></param>
		/// <param name="globe">The service used to project the features on the globe.</param>
		public GeometryManager(IFeatureManager featureManager, IGlobe globe)
		{
			this.globe = globe;
			geometryRoot = GameObject.Find("Geometries").transform;

			LoadPrefabs(geometryRoot.Find("prefabs"));

			featureMap = new Dictionary<Feature, GameObject>();

			featureManager.FeatureCreated += OnFeatureCreated;
		}

		/// <summary>
		/// Load all geometric prefabs that will serve as
		/// instances of geometric feature representations.
		/// </summary>
		/// <param name="prefabRoot"></param>
		private void LoadPrefabs(Transform prefabRoot)
		{
			pointPrefab = prefabRoot.Find("point").gameObject;
		}

		/// <summary>
		/// Generate the visual representation of a <see cref="Landmark"/> object.
		/// </summary>
		/// <param name="landmark">The landmark to represent.</param>
		/// <exception cref="ArgumentNullException">landmark is null.</exception>
		/// <exception cref="NotImplementedException">If the landmark geometry is unsupported.</exception>
		private void GenerateLandmark(Landmark landmark)
		{
			if (landmark == null)
			{
				throw new ArgumentNullException("landmark");
			}

			GameObject landmarkObject;

			Geometry geometry = landmark.Geometry;

			if (geometry is Point)
			{
				landmarkObject = GeneratePoint(geometry as Point);
			}
			else
			{
				throw new NotImplementedException(string.Format("The geometry type {0} is unsupported.", geometry.GetType().Name));
			}

			landmarkObject.name = landmark.Name;
			landmarkObject.SetActive(landmark.Visible);
		}

		/// <summary>
		/// Generate a <see cref="GameObject"/> at the location of <see cref="Point"/> geometry,
		/// using the default point prefab mesh and material.
		/// </summary>
		private GameObject GeneratePoint(Point point)
		{
			GameObject pointObject = GameObject.Instantiate(pointPrefab);
			pointObject.transform.localPosition = globe.Project(point.Coordinates);
			pointObject.transform.parent = geometryRoot;

			return pointObject;
		}

		/// <summary>
		/// Generate the visual representation of the a newly created <see cref="Feature"/> whose type is supported.
		/// </summary>
		private void OnFeatureCreated(object sender, FeatureImportedEventArgs e)
		{
			Feature imported = e.Feature;
			if (imported is Landmark)
			{
				GenerateLandmark(imported as Landmark);
			}
		}
	}
}
