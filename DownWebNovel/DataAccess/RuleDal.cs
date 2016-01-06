using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownWebNovel.DataAccess
{
	public class RuleDal
	{
		public static Hashtable LoadAllRules()
		{
			var allRules = new Hashtable();
			const string strSql = "SELECT * from Rule order by WebSite";
			var comm = new OleDbCommand(strSql, DbManager.OleDbConn);
			var reader = comm.ExecuteReader();
			if (reader == null) return null;

			while (reader.Read())
			{
				var webSite = Common.GetSafeString(reader, 1);

				Rule rule;
				if (allRules.ContainsKey(webSite))
				{
					rule = (Rule)allRules[webSite];
				}
				else
				{
					rule = new Rule
					{
						WebSite = webSite,
						PositionTag = new Hashtable(),
						ReplaceTag = new Hashtable()
					};

					rule.PositionTag["TitleStart"] = new List<string>();
					rule.PositionTag["TitleEnd"] = new List<string>();
					rule.PositionTag["ContentStart"] = new List<string>();
					rule.PositionTag["ContentEnd"] = new List<string>();
					rule.PositionTag["NextParaStart"] = new List<string>();
					rule.PositionTag["NextParaEnd"] = new List<string>();
					rule.ReplaceTag["TitleReplace"] = new List<KeyValuePair<string, string>>();
					rule.ReplaceTag["ContentReplace"] = new List<KeyValuePair<string, string>>();
					rule.ReplaceTag["NextParaReplace"] = new List<KeyValuePair<string, string>>();
					allRules[webSite] = rule;
				}

				var key = Common.GetSafeString(reader, 2);
				var value1 = Common.GetSafeString(reader, 3);
				if (key.EndsWith("Replace"))
				{
					var value2 = Common.GetSafeString(reader, 4);
					var list = (List<KeyValuePair<string, string>>) rule.ReplaceTag[key];
					list.Add(new KeyValuePair<string, string>(value1, value2));
				}
				else
				{
					var list = (List<string>) rule.PositionTag[key];
					list.Add(value1);
				}
				
			}

			reader.Close();
			return allRules;
		}
	}
}
