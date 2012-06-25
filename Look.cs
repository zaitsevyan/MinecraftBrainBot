using System;
namespace BrainBot
{
	public class Look<T>
	{
		public T yaw;
		public T pitch;
		public Look (T yaw, T pitch)
		{
			this.yaw = yaw;
			this.pitch = pitch;
		}
	}
}

