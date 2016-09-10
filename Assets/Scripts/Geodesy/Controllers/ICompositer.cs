namespace OpenTerra.Controllers
{
	public interface ICompositer
	{
		bool BackgroundVisible { get; set; }
		void Update();
	}
}