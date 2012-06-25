using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Timers;
namespace BrainBot
{
	class MainClass
	{ 
		public static void Main (string[] args)
		{
			Minecraft m = new Minecraft ("main.ttyh.ru", 25565);
			m.Status ();
			Thread connection = new Thread (new ThreadStart (delegate {
				m.Start ("HypnoToad");
			}
			)
			);
			connection.Start ();
			while (!m.isLogged)
				Thread.Sleep (100);
			while (m.isLogged) {
				string command = Console.ReadLine ();
				if (command.Length > 0) {
					if (command [0] == '!') {
						string[] line = command.Split (' ');
						if (line [0] == "!move" && line.Length >= 4) {
							XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
							double.TryParse (line [1], out next.x);
							double.TryParse (line [2], out next.y);
							double.TryParse (line [3], out next.z);
							m.player.MoveTo (next);
						}
						if (line [0] == "!player" && line.Length >= 2) {
							bool onGround = true;
							bool.TryParse (line [1], out onGround);
							m.player.changeGround (onGround);
						}
					} else {
						m.writeToChat (command);
					}
				}
			}
		}
	}
}
