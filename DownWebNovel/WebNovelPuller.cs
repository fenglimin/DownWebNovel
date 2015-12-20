using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace DownWebNovel
{
	public class DownloadTask
	{
		public Novel Novel { get; set; }
		public Rule Rule { get; set; }
		public Thread Thread { get; set; }
	}

	public class Novel
	{
		public string Name { get; set; }
		public string RootUrl { get; set; }
		public string StartPara { get; set; }
		public string EndPara { get; set; }
	}

	public class Rule
	{
		public string WebSite { get; set; }
		public List<string> TitleStartTagList { get; set; }
		public List<string> TitleEndTagList { get; set; }
		public List<string> ConetentStartTagList { get; set; }
		public List<string> ContentEndTagList { get; set; }
		public List<string> NextParaStartTagList { get; set; }
		public List<string> NextParaEndTagList { get; set; }

		public List<KeyValuePair<string, string>> ReplaceTagList { get; set; }
	}

	public interface IWebNovelPullerUser
	{
		void OnFileDownloaded(string novelName, string curPara, string nextPara);
	}

	public class WebNovelPuller
	{
		private readonly WebClient _webClient = new WebClient();
		private readonly IWebNovelPullerUser _webNovelPullerUser;

		public WebNovelPuller(IWebNovelPullerUser webNovelPullerUser)
		{
			_webNovelPullerUser = webNovelPullerUser;
		}

		public bool DownloadNovel(Novel novel, Rule rule)
		{
			var curFile = novel.StartPara;
			while (curFile != novel.EndPara)
				curFile = DownloadTextByUrl(novel.Name, novel.RootUrl, curFile, rule);
			return true;
		}

		private static string ExtractIntrested(string content, IEnumerable<string> startTagList, IEnumerable<string> endTagList)
		{
			var startIndex = -1;
			var startTagLen = 0;
			foreach (var startTag in startTagList)
			{
				startIndex = content.IndexOf(startTag, StringComparison.Ordinal);
				if (startIndex != -1)
				{
					startTagLen = startTag.Length;
					break;
				}
			}

			if (startIndex == -1)
				return "Error finding start tag" + startTagList.ToString();

			content = content.Substring(startIndex + startTagLen);


			var endIndex = -1;
			foreach (var endTag in endTagList)
			{
				endIndex = content.IndexOf(endTag, StringComparison.Ordinal);
				if (endIndex != -1)
				{
					break;
				}
			}

			if (endIndex == -1)
				return "Error finding end tag" + endTagList.ToString();

			content = content.Substring(0, endIndex);

			return content;
		}

		private static string Replace(string content, IEnumerable<KeyValuePair<string, string>> replacePairs)
		{
			return replacePairs.Aggregate(content, (current, replace) => current.Replace(replace.Key, replace.Value));
		}

		private string DownloadTextByUrl(string novelName, string rootUrl, string fileName, Rule rule)
		{
			string downloadedString;
			if (!DownloadStringByUrl(rootUrl + fileName, out downloadedString))
				return fileName;

			// Biquge
			var title = ExtractIntrested(downloadedString, rule.TitleStartTagList, rule.TitleEndTagList);
			var content = ExtractIntrested(downloadedString, rule.ConetentStartTagList, rule.ContentEndTagList);
			var nextUrl = ExtractIntrested(downloadedString, rule.NextParaStartTagList, rule.NextParaEndTagList);

			var textWriter = File.AppendText("D:\\" + novelName + ".txt");

			textWriter.WriteLine(title);
			content = Replace(content, rule.ReplaceTagList);
			textWriter.WriteLine(content);

			if (_webNovelPullerUser != null)
			{
				_webNovelPullerUser.OnFileDownloaded(novelName, title, nextUrl);
			}

			textWriter.Close();
			return nextUrl;
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
