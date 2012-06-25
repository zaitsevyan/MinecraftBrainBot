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
				m.writeToChat (command);
			}
		}
	}
}
