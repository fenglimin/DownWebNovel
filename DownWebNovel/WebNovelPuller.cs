using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using DownWebNovel;
using DownWebNovel.DataAccess;

namespace DownWebNovel
{

	public class XWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address) as HttpWebRequest;
			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			return request;
		}
	}

	public class ParseTagException : Exception
	{
		public ParseTagException(string message)
			: base(message)
		{

		}
	}

	public class Task
	{
		public string TaskName { get; set; }
		public string RootUrl { get; set; }
		public string ParaUrlStart { get; set; }
		public string ParaUrlEnd { get; set; }
		public string TaskDir { get; set; }
		public string RuleName { get; set; }
		public bool IsPicture { get; set; }
		public string PictureUrlPrefix { get; set; }
		public string ParaUrlLastDownloaded { get; set; }
		public string ParaTitleLastDownloaded { get; set; }
		public string ContentLastDownloaded { get; set; }
		public string ParaUrlNextToDownload { get; set; }
		public bool IsBookCompleted { get; set; }
		public bool IsBookWatched { get; set; }

		public Rule Rule { get; set; }
		public Thread Thread { get; set; }
		public WebNovelPuller WebNovelPuller { get; set; }
	}

	public class Rule
	{
		public string WebSite { get; set; }
		public Hashtable PositionTag { get; set; }
		public Hashtable ReplaceTag { get; set; }

		public Rule Clone()
		{
			var rule = new Rule { WebSite = WebSite, PositionTag = new Hashtable(), ReplaceTag = new Hashtable() };

			foreach (DictionaryEntry dictionaryEntry in PositionTag)
			{
				var originalItems = (List<string>)dictionaryEntry.Value;
				var items = originalItems.Select(originalItem => originalItem.Clone().ToString()).ToList();
				rule.PositionTag[dictionaryEntry.Key] = items;
			}


			foreach (DictionaryEntry dictionaryEntry in ReplaceTag)
			{
				var originalItems = (List<KeyValuePair<string, string>>)dictionaryEntry.Value;
				var items = originalItems.Select(originalItem => new KeyValuePair<string, string>(originalItem.Key, originalItem.Value)).ToList();
				rule.ReplaceTag[dictionaryEntry.Key] = items;
			}

			return rule;

		}
	}

	public interface IWebNovelPullerUser
	{
		void OnFileDownloaded(Task task);
		void OnTaskStopped(Task task, string stopReason);
		void OnSubTaskCreated(Task task);
		void OnShowMessage(Task task, bool isDownloadError, string errorMessage);
	}

	public class WebNovelPuller
	{
		private readonly XWebClient _webClient = new XWebClient();
		private readonly IWebNovelPullerUser _webNovelPullerUser;
		public bool ExitRequestedByUser { get; set; }

		public WebNovelPuller(IWebNovelPullerUser webNovelPullerUser)
		{
			_webNovelPullerUser = webNovelPullerUser;
		}

		private void ShowMessage(Task task, bool isDownloadError, string errorMessage)
		{
			if (_webNovelPullerUser != null)
				_webNovelPullerUser.OnShowMessage(task, isDownloadError, errorMessage);
		}

		public bool RunTask(Task task)
		{
			while (!FindNextPara(task) && !ExitRequestedByUser)
			{
			}


			var ok = true;
			while (task.ParaUrlNextToDownload != task.ParaUrlEnd && !ExitRequestedByUser)
			{
				ok = task.RuleName.StartsWith("task_") ? CreateTaskFromTask(task) : DownloadCurrentPage(task);
				if (!ok)
					break;
			}

			if (_webNovelPullerUser != null)
			{
				_webNovelPullerUser.OnTaskStopped(task, ExitRequestedByUser ? "用户终止！" : ok? "已下载到终章！" : "出现异常");
			}

			return true;
		}

		private bool CreateTaskFromTask(Task task)
		{
			string downloadedString;
			if (!DownloadStringByUrl(task.RootUrl + task.ParaUrlNextToDownload, out downloadedString))
			{
				ShowMessage(task, true, "下载错误， 任务重试！ - " + downloadedString);
				return true;
			}

			try
			{
				var newTaskRule = task.RuleName.Substring(5);
				var sampleTask = TaskDal.GetSampleTaskByRule(newTaskRule);

				var taskUrlList = ExtractIntrestedList(downloadedString, task.Rule, "Content");
				foreach (var taskUrl in taskUrlList)
				{
					var subTask = new Task()
					{
						TaskName = task.TaskName + "__" + Guid.NewGuid(),
						IsPicture = true,
						ParaUrlEnd = sampleTask.ParaUrlEnd,
						ParaUrlStart = taskUrl,
						PictureUrlPrefix = sampleTask.PictureUrlPrefix,
						RootUrl = task.PictureUrlPrefix,
						RuleName = newTaskRule,
						TaskDir = task.TaskDir
					};

					if (_webNovelPullerUser != null)
					{
						_webNovelPullerUser.OnSubTaskCreated(subTask);
					}
				}

				var nextUrl = ExtractIntrested(downloadedString, task.Rule, "NextPara");
				task.ParaUrlLastDownloaded = task.ParaUrlNextToDownload;
				task.ParaUrlNextToDownload = nextUrl;
				return true;
			}
			catch (ParseTagException ex)
			{
				ShowMessage(task, false, "解析错误，任务终止！ - " + ex.Message);
			}
			catch (Exception ex)
			{
				ShowMessage(task, false, "发生异常，任务终止！ - " + ex.Message);
			}
			return false;
		}

		private void FixNextPara(string content, Task task)
		{
			if (task.ParaUrlNextToDownload == "-1")
			{
				// This spcecfic logic is for downloading picture
				var nextUrl = FixNextPara(content);
				var temp = task.RootUrl.LastIndexOf("/", StringComparison.InvariantCulture);
				task.RootUrl = task.RootUrl.Substring(0, temp - 1) + nextUrl + "&n=";
				task.ParaUrlNextToDownload = "0";
			}
		}

		private string FixNextPara(string content)
		{
			// This spcecfic logic is for downloading picture
			return ExtractIntrested(content, "下一篇 => href=\"", "\"");
		}

		private bool FindNextPara(Task task)
		{
			if (string.IsNullOrEmpty(task.ParaUrlLastDownloaded))
			{
				task.ParaUrlNextToDownload = task.ParaUrlStart;
				ShowMessage(task, false, "下载开始，起始章 - " + task.ParaUrlNextToDownload);
				return true;
			}

			string downloadedString;
			if (!DownloadStringByUrl(task.RootUrl + task.ParaUrlLastDownloaded, out downloadedString))
			{
				ShowMessage(task, true, "下载错误， 任务重试！ - " + downloadedString);
				return true;
			}

			try
			{
				var nextUrl = ExtractIntrested(downloadedString, task.Rule, "NextPara");
				task.ParaUrlNextToDownload = nextUrl;
				ShowMessage(task, false, "下载开始，起始章 - " + task.ParaUrlNextToDownload);
				FixNextPara(downloadedString, task);
				return true;
			}
			catch (ParseTagException ex)
			{
				ShowMessage(task, false, "解析错误，任务终止！ - " + ex.Message);
			}
			catch (Exception ex)
			{
				ShowMessage(task, false, "发生异常，任务终止！ - " + ex.Message);
			}

			return false;
		}

		public string DownloadPageForVerify(string url, Rule rule)
		{
			string downloadedString;
			if (!DownloadStringByUrl(url, out downloadedString))
				return "下载错误! --- " + downloadedString;

			string result;
			try
			{
				var title = ExtractIntrested(downloadedString, rule, "Title");
				var content = ExtractIntrested(downloadedString, rule, "Content");
				var nextUrl = ExtractIntrested(downloadedString, rule, "NextPara");
				if (nextUrl == "-1")
				{
					nextUrl = FixNextPara(downloadedString);
				}

				result = string.Format("章节名：{0}\r\n\r\n正文：\r\n\r\n{1}\r\n\r\n下一章：{2}\r\n", title, content, nextUrl);
			}
			catch (ParseTagException ex)
			{
				result = "解析错误！ --- " + ex.Message; 
			}
			catch (Exception ex)
			{
				result = "发生异常！ --- " + ex.Message; 
			}

			return result;
		}

		private bool DownloadCurrentPage(Task task)
		{
			string downloadedString;
			if (!DownloadStringByUrl(task.RootUrl + task.ParaUrlNextToDownload, out downloadedString))
			{
				ShowMessage(task, true, "下载错误， 任务重试！ - " + downloadedString);
				return true;
			}

			try
			{
				var title = ExtractIntrested(downloadedString, task.Rule, "Title");
				var content = ExtractIntrested(downloadedString, task.Rule, "Content");
				var nextUrl = ExtractIntrested(downloadedString, task.Rule, "NextPara");

				if (task.IsPicture)
				{
					var pictureUrl = task.PictureUrlPrefix + content;
					var part = content.Split('/');
					var fileName = part[part.Count() - 1];
					fileName = Path.Combine(task.TaskDir, title + "_" + fileName);
					_webClient.DownloadFile(pictureUrl, fileName);
				}
				else
				{
					var textWriter = File.AppendText(Path.Combine(task.TaskDir, task.TaskName + ".txt"));
					textWriter.WriteLine(title);
					textWriter.WriteLine(content);
					textWriter.Close();
				}

				task.ParaTitleLastDownloaded = title;
				task.ParaUrlLastDownloaded = task.ParaUrlNextToDownload;
				task.ContentLastDownloaded = content;
				task.ParaUrlNextToDownload = nextUrl;
				FixNextPara(downloadedString, task);

				if (_webNovelPullerUser != null)
				{
					_webNovelPullerUser.OnFileDownloaded(task);
				}
				return true;
			}
			catch (ParseTagException ex)
			{
				ShowMessage(task, false, "解析错误，任务终止！ - " + ex.Message);
			}
			catch (Exception ex)
			{
				ShowMessage(task, false, "发生异常，任务终止！ - " + ex.Message);
			}

			return false;
		}

		private static IEnumerable<string> ExtractIntrestedList(string content, Rule rule, string key)
		{
			var startTagList = (List<string>)rule.PositionTag[key + "Start"];
			var endTagList = (List<string>)rule.PositionTag[key + "End"];
			var replacePairs = (List<KeyValuePair<string, string>>)rule.ReplaceTag[key + "Replace"];

			var intrestedList = new List<string>();

			while (true)
			{
				var startIndex = FindIndex(content, startTagList, false);
				if (startIndex == -1)
					break;

				content = content.Substring(startIndex);

				var endIndex = FindIndex(content, endTagList, true);
				if (endIndex == -1)
					throw new ParseTagException("Error finding end tag of " + key);

				var remainingContent = content.Substring(endIndex);
				content = content.Substring(0, endIndex);

				while (true)
				{
					var reallyStartIndex = FindIndex(content, startTagList, false);
					if (reallyStartIndex == -1)
						break;

					content = content.Substring(reallyStartIndex);
				}

				// Replace
				content = Replace(content, replacePairs);
				intrestedList.Add(content);

				content = remainingContent;
			}

			return intrestedList;
		}

		private static string ExtractIntrested(string content, string startTag, string endTag)
		{
			var startIndex = IndexOf(content, startTag, false);
			if (startIndex == -1)
				throw new ParseTagException("Error finding start tag");

			content = content.Substring(startIndex);
			var endIndex = IndexOf(content, endTag, true);
			if (endIndex == -1)
				throw new ParseTagException("Error finding end tag");

			content = content.Substring(0, endIndex);

			return content;
		}

		private static string ExtractIntrested(string content, Rule rule, string key)
		{
			var startTagList = (List<string>)rule.PositionTag[key + "Start"];
			var endTagList = (List<string>)rule.PositionTag[key + "End"];
			var replacePairs = (List<KeyValuePair<string, string>>)rule.ReplaceTag[key + "Replace"];

			var startIndex = FindIndex(content, startTagList, false);


			if (startIndex != -1)
			{
				content = content.Substring(startIndex);

				var endIndex = FindIndex(content, endTagList, true);
				if (endIndex == -1)
					throw new ParseTagException("Error finding end tag of " + key);

				content = content.Substring(0, endIndex);

				while (true)
				{
					var reallyStartIndex = FindIndex(content, startTagList, false);
					if (reallyStartIndex == -1)
						break;

					content = content.Substring(reallyStartIndex);
				}
			}
			else
			{
				throw new ParseTagException("Error finding start tag of " + key);
			}


			// Replace
			content = Replace(content, replacePairs);

			return content;

		}

		private static int FindIndex(string content, IEnumerable<string> tagSequences, bool forEnd)
		{
			var startIndex = -1;
			foreach (var tagSequence in tagSequences)
			{
				startIndex = IndexOf(content, tagSequence, forEnd);
				if (startIndex != -1)
				{
					break;
				}
			}

			return startIndex;
		}

		private static int IndexOf(string content, string tagSequence, bool forEnd)
		{
			var index = 0;

            if (tagSequence.Contains(" => "))
            {
                var tags = tagSequence.Split(new string[] { " => " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in tags)
                {
                    index = content.IndexOf(tag, index, StringComparison.Ordinal);
                    if (index == -1)
                        return -1;

                    index += tag.Length;
                }

                if (forEnd)
                {
                    index -= tags[tags.Count() - 1].Length;
                }
            }
            else if (tagSequence.Contains(" <= "))
            {
                var tags = tagSequence.Split(new string[] { " <= " }, StringSplitOptions.RemoveEmptyEntries);
                index = content.Length;
                foreach (var tag in tags)
                {
                    index = content.LastIndexOf(tag, index, StringComparison.Ordinal);
                    if (index == -1)
                        return -1;

                    index += tag.Length;
                }

                if (forEnd)
                {
                    index -= tags[tags.Count() - 1].Length;
                }
            }
            else
            {
                index = content.IndexOf(tagSequence, index, StringComparison.Ordinal);
                if (index == -1)
                    return -1;

                index += tagSequence.Length;

                if (forEnd)
                {
                    index -= tagSequence.Length;
                }
            }

			

			return index;
		}

		private static string Replace(string content, IEnumerable<KeyValuePair<string, string>> replacePairs)
		{
			var translate = new Hashtable();
			translate["[空格]"] = " ";
			translate["[换行]"] = "\r\n";
			translate["[删除]"] = string.Empty;

			return replacePairs.Aggregate(content, (current, replace) => current.Replace(replace.Key, 
				translate.ContainsKey(replace.Value)? (string)translate[replace.Value] : replace.Value));
		}

		public bool DownloadStringByUrl(string url, out string content)
		{
			try
			{
				content = _webClient.DownloadString(url);
			}
			catch (Exception ex)
			{
				content = ex.Message;
				return false;
			}

			return true;
		}
	}
}
