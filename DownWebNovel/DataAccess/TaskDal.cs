using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownWebNovel.DataAccess
{
	public class TaskDal
	{
		public static List<Task> LoadAllTasks()
		{
			var allTasks = new List<Task>();
			const string strSql = "SELECT * from Task";
			var comm = new OleDbCommand(strSql, DbManager.OleDbConn);
			var reader = comm.ExecuteReader();
			if (reader == null) return null;

			while (reader.Read())
			{
				var task = new Task
				{
					TaskName = Common.GetSafeString(reader, 1),
					TaskDir = Common.GetSafeString(reader, 2),
					RootUrl = Common.GetSafeString(reader, 3),
					ParaStart = Common.GetSafeString(reader, 4),
					ParaEnd = Common.GetSafeString(reader, 5),
					RuleName = Common.GetSafeString(reader, 6)
				};

				allTasks.Add(task);
			}

			reader.Close();
			return allTasks;
		}
	}
}
