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
			m.Start("HypnoToad");
		}
	}
}
