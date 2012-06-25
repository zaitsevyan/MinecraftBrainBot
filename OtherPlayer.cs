using System;
using System.Collections.Generic;


namespace BrainBot
{
	public class OtherPlayer:Entity
	{
		public XYZ<int> position;
		public Look<byte> look;
		public short currentItem;
		public string name;
		public Dictionary<int,Armor> armor;
		public OtherPlayer ()
		{
			position = new XYZ<int> (0, 0, 0);
			look = new Look<byte> (0, 0);
			armor = new Dictionary<int, Armor> ();
		}
	}
}

