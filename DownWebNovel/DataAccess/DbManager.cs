using System;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace DownWebNovel.DataAccess
{
	/// <summary>
	/// Summary description for DbConnManager
	/// </summary>
	public static class DbManager
	{
		/// <summary>
		/// the sync lock.
		/// </summary>
		private static readonly object SyncRoot = new Object();

		/// <summary>
		/// Database connection
		/// </summary>
		private static OleDbConnection oleDbConn;

		/// <summary>
		/// 
		/// </summary>
		private const string DatabaseConnectionStsring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=..\\..\\..\\DownWebNovel.mdb";

		/// <summary>
		/// 
		/// </summary>
		public static OleDbConnection OleDbConn
		{
			get
			{
				if (oleDbConn == null)
				{
					lock (SyncRoot)
					{
						if (oleDbConn == null)
						{
							//var connStringSetting = ConfigurationManager.ConnectionStrings[1];
							//oleDbConn = new OleDbConnection(connStringSetting.ConnectionString);
							oleDbConn = new OleDbConnection(DatabaseConnectionStsring);
							oleDbConn.Open();
						}
					}
				}

				return oleDbConn;
			}
		}
	}
}