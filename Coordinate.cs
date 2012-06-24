using System;
namespace BrainBot
{
	public class Coordinate<T>
	{
		public T x{ get {lock (this) return x; } private set{}}
		public T y{ get {lock (this) return y; } private set{}}
		public T z{ get {lock (this) return z; } private set{}}
		public Coordinate (T x, T y, T z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}
}

