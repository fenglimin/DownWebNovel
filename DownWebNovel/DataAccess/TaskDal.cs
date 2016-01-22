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
				var task = FillTask(reader);

				allTasks.Add(task);
			}

			reader.Close();
			return allTasks;
		}

		private static Task FillTask(OleDbDataReader reader)
		{
			var task = new Task
			{
				TaskName = Common.GetSafeString(reader, 1),
				TaskDir = Common.GetSafeString(reader, 2),
				RootUrl = Common.GetSafeString(reader, 3),
				ParaUrlStart = Common.GetSafeString(reader, 4),
				ParaUrlEnd = Common.GetSafeString(reader, 5),
				RuleName = Common.GetSafeString(reader, 6),
				IsPicture = reader.GetBoolean(7),
				PictureUrlPrefix = Common.GetSafeString(reader, 8),
				ParaUrlLastDownloaded = Common.GetSafeString(reader, 9),
				ParaTitleLastDownloaded = Common.GetSafeString(reader, 10)
			};

			return task;
		}

		public static Task GetSampleTaskByRule(string ruleName)
		{
			var strSql = string.Format("SELECT * from Task WHERE RuleName = '{0}'", ruleName);
			var comm = new OleDbCommand(strSql, DbManager.OleDbConn);
			var reader = comm.ExecuteReader();
			if (reader == null) return null;
			if (!reader.Read()) return null;

			var task = FillTask(reader);
			reader.Close();
			return task;
		}

	    public static void AddTask(Task task)
	    {
	        var strSql =
	            string.Format(
					"INSERT INTO Task (TaskName, TaskDir, RootUrl, ParaStart, ParaEnd, RuleName, IsPicture, PictureUrlPrefix, ParaUrlLastDownloaded, ParaTitleLastDownloaded) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', '{8}', '{9}')",
	                task.TaskName, task.TaskDir, task.RootUrl, task.ParaUrlStart, task.ParaUrlEnd, task.RuleName, task.IsPicture, task.PictureUrlPrefix, task.ParaUrlLastDownloaded, task.ParaTitleLastDownloaded);

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