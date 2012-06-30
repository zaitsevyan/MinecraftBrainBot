using System;
namespace BrainBot
{
	public class Map
	{
		Chunk[,] chunks;
		XYZ<int> playerChunk;
		Minecraft minecraft;
		public Map (Minecraft minecraft)
		{
			this.minecraft = minecraft;
			//chunks = new Chunk[21, 21] ();
			playerChunk = new XYZ<int> ((int)minecraft.player.position.x/16,0,(int)minecraft.player.position.z/16);
		}
		public void initChunk()
		{
			
		}
		public void unloadChunk()
		{
		}
		public void loadChunk()
		{
		}
	}
}

