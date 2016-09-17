namespace OpenTerra.Unity
{
	public interface ICompositer
	{
		bool BackgroundVisible { get; set; }
		void Update();
		void Initialize();
	}
}