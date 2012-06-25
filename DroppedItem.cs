using System;
namespace BrainBot
{
	public class DroppedItem:Entity
	{
		public XYZ<int> position;
		public byte rotation;
		public byte pitch;
		public byte roll;
		public short data;
		public byte count;
		public short item;
		public DroppedItem ()
		{
			position = new XYZ<int> (0,0,0);
		}
	}
}

