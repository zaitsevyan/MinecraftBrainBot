using System;
namespace BrainBot
{
	public class Mob:Entity
	{
		public XYZ<int> position;
		public Look<byte> look;
		public int metadata = 0;
		public byte type = 0;
		public Mob ()
		{
			position = new XYZ<int> (0, 0, 0);
			look = new Look<byte> (0,0);
		}
	}
}

