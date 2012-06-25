using System;
namespace BrainBot
{
	public class Painting:Entity
	{
		public string title = "";
		public int direction = 0;
		public XYZ<int> centerPosition = new XYZ<int> (0, 0, 0);
		public Painting ()
		{
		}
	}
}

