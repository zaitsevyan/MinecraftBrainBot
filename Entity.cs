using System;
namespace BrainBot
{
	public class Entity
	{
		public int entityID = 0;
		public byte headYaw = 0;
		public XYZ<short> velocity;
		public Entity ()
		{
			velocity = new XYZ<short> (0, 0, 0);
		}
	}
}

