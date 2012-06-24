using System;
namespace BrainBot
{
	public class Player
	{
		public string name;
		public Coordinate<double> position{ get; internal set;}
		private Minecraft minecraft;
		private double speed;
		public Player (string name, Minecraft minecraft,Coordinate<double> startPosition)
		{
			this.name = name;
			this.minecraft = minecraft;
			position = startPosition;
		}
		public void MoveTo(Coordinate<double> position)
		{
			
		}
	}
}

