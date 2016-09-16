using OpenTerra.DataModel;
using OpenTerra.Unity.Patches;

namespace OpenTerra
{
	public interface IPatchManager
	{
		void Update();
		Patch Get(Location location);
	}
}