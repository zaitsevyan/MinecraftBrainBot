using System;
namespace BrainBot
{
	public class Chunk
	{
		public XYZ<int> position;
		public Chunk ()
		{
			position = new XYZ<int> (0,0,0);
		}
	}
}

