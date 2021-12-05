using CollisionFlow;

namespace SolidFlow
{
	public interface ISpeedHandler
	{
		Vector128 Vector { get; set; }
		Vector128 Limit { get; set; }
	}
}
