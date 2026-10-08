using System;

namespace Parcial.Architecture.Exceptions
{
	public sealed class DataEntryException : Exception
	{
		public DataEntryException(string message) : base(message)
		{
		}
	}

    
}