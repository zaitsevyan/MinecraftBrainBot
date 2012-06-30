using System;
namespace BrainBot
{
	public class Chunk
	{
		public int x;
		public int z;
		public int size;
		public Chunk (int x, int z)
		{
			this.x = x;
			this.z = z;
			this.size = 0;
		}
	}
}

