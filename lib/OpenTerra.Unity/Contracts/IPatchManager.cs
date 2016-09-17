using OpenTerra.DataModel;
using OpenTerra.Unity.Patches;

namespace OpenTerra.Unity
{
	public interface IPatchManager
	{
		void Update();
		Patch Get(Location location);
	}
}