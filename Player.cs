using System;
namespace BrainBot
{
	public class Player:Entity
	{
		public string name;
		public XYZ<double> position;
		public Look<float> look;
		public double height;
		private Minecraft minecraft;
		private double speed;
		public bool invulnerability = false;
		public bool isFlying = false;
		public bool canFly = false;
		public bool instantDestroy = false;
		public bool onGround = false;
		public short health = 20;
		public short food = 10;
		public float saturation = 0;
		public Player (string name, Minecraft minecraft)
		{
			this.name = name;
			this.minecraft = minecraft;
			entityID = 0;
			this.position = new XYZ<double> (0, 0, 0);
			look = new Look<float> (0, 0);
		}
	}
}

