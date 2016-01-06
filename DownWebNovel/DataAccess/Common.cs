using System;
using System.Data.OleDb;

namespace DownWebNovel.DataAccess
{
	/// <summary>
	/// Summary description for Common
	/// </summary>
	public static class Common
	{
		public static string GetSafeString(OleDbDataReader reader, int order)
		{
			var rt = string.Empty;
			try
			{
				rt = reader.GetString(order);
			}
			catch (Exception ex)
			{
				string message = ex.Message;
			}

			return rt;
		}

        public static int SafeConvertToInt(string data)
        {
            var ret = 0;
            try
            {
                ret = Convert.ToInt32(data);
            }
            catch(Exception ex)
            {

            }

            return ret;
        }

		public static string GetSafeDateTime(OleDbDataReader reader, int order)
		{
			string rt;
			try
			{
				var dt = reader.GetDateTime(order);
				rt = dt.ToString("yyyy-MM-dd");
			}
			catch (Exception ex)
			{
				rt = "";
			}

			return rt;
		}
	}
}