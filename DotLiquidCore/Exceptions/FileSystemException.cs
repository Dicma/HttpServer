#region Changes
/*
Changed by Miha Strehar in 2016, Removed [Serializable] as it does not exist in .NET Core and
cleaned up unnecessary using.
*/
#endregion

namespace DotLiquidCore.Exceptions
{
    public class FileSystemException : LiquidException
	{
		public FileSystemException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}
	}
}