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

	    public static void AddTask(Task task)
	    {
	        var strSql =
	            string.Format(
	                "INSERT INTO Task (TaskName, TaskDir, RootUrl, ParaStart, ParaEnd, RuleName) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
	                task.TaskName, task.TaskDir, task.RootUrl, task.ParaStart, task.ParaEnd, task.RuleName);

            var comm = new OleDbCommand(strSql, DbManager.OleDbConn);
            comm.ExecuteNonQuery(); 
	    }

	    public static void DeleteTask(string taskName)
	    {
            var strSql = string.Format("DELETE FROM Task WHERE ( TaskName = '{0}')", taskName);

            var comm = new OleDbCommand(strSql, DbManager.OleDbConn);
            comm.ExecuteNonQuery();
	    }

	}
}