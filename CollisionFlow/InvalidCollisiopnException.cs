using System;
using System.Runtime.Serialization;

namespace CollisionFlow
{
	public class InvalidCollisiopnException : InvalidOperationException
	{
		public InvalidCollisiopnException()
		{
		}

		public InvalidCollisiopnException(string message) : base(message)
		{
		}

		public InvalidCollisiopnException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidCollisiopnException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
