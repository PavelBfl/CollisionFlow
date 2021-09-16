using Gui.Core;
using System;

namespace Game.WindowsDX
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var game = new GameCore();
			game.Run();
		}
	}
}
