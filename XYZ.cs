using System;
namespace BrainBot
{
	public class XYZ<T>
	{
		public T x;
		public T y;
		public T z;
		public XYZ (T x, T y, T z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public override bool Equals (object obj)
		{
			if (obj.GetType () == typeof(XYZ<T>)) {
				XYZ<T> target = (XYZ<T>)obj;
				return this.x.Equals (target.x) && this.y.Equals (target.y) && this.z.Equals (target.z);
			} else
				return base.Equals (obj);
		}
	}
}

