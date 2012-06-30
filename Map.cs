using System;
using System.Threading;


namespace BrainBot
{
	public class Map
	{
		private Chunk[,] chunks;
		private XYZ<int> playerChunk;
		private Minecraft minecraft;
		private int center;
		public Map (Minecraft minecraft)
		{
			this.minecraft = minecraft;
			chunks = new Chunk[21, 21];
			center = this.chunks.GetLength (0) / 2;
			playerChunk = null;
		}
		public void initChunk (int x, int z)
		{
			chunks [center - playerChunk.x + x, center - playerChunk.z + z] = null;
			chunks [center - playerChunk.x + x, center - playerChunk.z + z] = new Chunk (x,z);			
		}
		public void unloadChunk (int x, int z)
		{
			if(chunks [center - playerChunk.x + x, center - playerChunk.z + z] != null && chunks[center - playerChunk.x + x, center - playerChunk.z + z].x==x && chunks[center - playerChunk.x + x, center - playerChunk.z + z].z == z)
				chunks [center - playerChunk.x + x, center - playerChunk.z + z] = null;
		}
		public void loadChunk (int x, int z, byte[] compressedData)
		{
			if (chunks [center - playerChunk.x + x, center - playerChunk.z + z] != null) {
				chunks [center - playerChunk.x + x, center - playerChunk.z + z].size = 0;
			}
		}
		private Chunk getChunk(int x, int z)
		{
			if(x<0 || x>=this.chunks.GetLength(0))
				return null;
			if(z<0 || z>=this.chunks.GetLength(1))
				return null;
			return this.chunks[x,z];
		}
		public void updateMap ()
		{
			int nx = (int)(minecraft.player.position.x / 16);
			int nz = (int)(minecraft.player.position.z / 16);
			if (playerChunk == null) {
				playerChunk = new XYZ<int> (nx, 0, nz);
			}
			if (nx == this.playerChunk.x && nz == this.playerChunk.z)
				return;
			Chunk[,] nchunks = new Chunk[this.chunks.GetLength (0), this.chunks.GetLength (1)];
			for (int i = 0; i<this.chunks.GetLength(0); i++)
				for (int j = 0; j<this.chunks.GetLength(1); j++) {
					nchunks [i, j] = this.getChunk (i + playerChunk.x - nx, j + nz - playerChunk.z);
				}
			this.chunks = nchunks;
			playerChunk.x = nx;
			playerChunk.z = nz;
		}
		public void WriteMap ()
		{
			for (int i = 0; i<this.chunks.GetLength(0); i++) {
				for (int j = 0; j<this.chunks.GetLength(1); j++) {
					if (chunks [i, j] != null && chunks [i, j].x == playerChunk.x && chunks [i, j].z == playerChunk.z)
						Console.Write ("P");
					else Console.Write (this.chunks [i, j] == null ? "O" : "X");
				}
				Console.WriteLine ();
			}
			Thread.Sleep (5);
		}
	}
}

