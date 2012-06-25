using System;
using System.Timers;


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
		public XYZ<double> endPosition;
		private Timer moving;
		public Player (string name, Minecraft minecraft)
		{
			this.name = name;
			this.minecraft = minecraft;
			entityID = 0;
			this.position = new XYZ<double> (0, 0, 0);
			look = new Look<float> (0, 0);
			moving = new Timer (50);
			moving.Elapsed += delegate( object source, ElapsedEventArgs e ) {
				this.nextMove ();
			};
			moving.Start ();
			speed = 5;//per 1 second
			//endPosition = new XYZ<double> (0, 0, 0);
		}
		public void MoveTo (XYZ<double> end)
		{
			lock (this) {
				this.endPosition = end;
			}
		}
		public void changeGround (bool ground)
		{
			lock (this) {
				this.onGround = ground;
			}
		}
		private void nextMove ()
		{
			lock (this) {
				if (this.endPosition != null && !this.position.Equals (this.endPosition)) {
					double realSpeed = this.speed / 1000.0 * this.moving.Interval;
					double distance = Math.Sqrt (Math.Pow (endPosition.x - position.z, 2) + Math.Pow (endPosition.y - position.y, 2) + Math.Pow (
						endPosition.z - position.z,
						2
					)
					);
					double u = realSpeed / (distance - realSpeed);
					XYZ<double> nextPosition = new XYZ<double> (0, 0, 0);
					nextPosition.x = (position.x + u * endPosition.x) / (1 + u);
					nextPosition.y = (position.y + u * endPosition.y) / (1 + u);
					nextPosition.z = (position.z + u * endPosition.z) / (1 + u);
					this.position = nextPosition;
					if (realSpeed >= distance)
						this.position = this.endPosition;
					this.minecraft.SendPacket (new object[]{(byte)PacketID.PlayerPosition,this.position.x,this.position.y,this.position.y+this.height,this.position.z,this.onGround});
				}
			}
		}
	}
}

