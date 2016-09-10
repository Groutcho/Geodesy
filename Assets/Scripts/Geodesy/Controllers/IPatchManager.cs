using OpenTerra.Models;
using OpenTerra.Views;

namespace OpenTerra.Controllers
{
	public interface IPatchManager
	{
		void Update();
		Patch Get(Location location);
	}
}