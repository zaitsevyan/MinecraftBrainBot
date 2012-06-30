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
							bool onGround = false;
							bool.TryParse (line [1], out onGround);
							Console.WriteLine ("onGround: {0}", onGround);
							m.player.changeGround (onGround);
						}
						if (line [0] == "!map") {
							m.map.WriteMap ();
						}
						if (line [0] == "!control") {
							ConsoleKeyInfo info = Console.ReadKey ();
							while (info.KeyChar!='q') {
								if (info.KeyChar == 'w') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.x += 0.25;
									m.player.MoveTo (next);
								}
								if (info.KeyChar == 's') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.x -= 0.25;
									m.player.MoveTo (next);
								}
								if (info.KeyChar == 'a') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.z -= 0.25;
									m.player.MoveTo (next);
								}
								if (info.KeyChar == 'd') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.z += 0.25;
									m.player.MoveTo (next);
								}
								if (info.KeyChar == 'x') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.y -= 0.25;
									m.player.MoveTo (next);
								}
								if (info.KeyChar == ' ') {
									XYZ<double> next = new XYZ<double> (m.player.position.x, m.player.position.y, m.player.position.z);
									next.y += 0.25;
									m.player.MoveTo (next);
								}
								info = Console.ReadKey ();
							}
						}
					} else {
						m.writeToChat (command);
					}
				}
			}
		}
	}
}
