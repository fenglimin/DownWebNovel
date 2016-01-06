using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Threading;

namespace DownWebNovel
{
	public class DownloadTask
	{
		public Novel Novel { get; set; }
		public Rule Rule { get; set; }
		public Thread Thread { get; set; }
		public WebNovelPuller WebNovelPuller { get; set; }
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
		public Hashtable PositionTag { get; set; }
		public Hashtable ReplaceTag { get; set; }
		//public List<string> TitleStartTagList { get; set; }
		//public List<string> TitleEndTagList { get; set; }
		//public List<string> ConetentStartTagList { get; set; }
		//public List<string> ContentEndTagList { get; set; }
		//public List<string> NextParaStartTagList { get; set; }
		//public List<string> NextParaEndTagList { get; set; }

		//public List<KeyValuePair<string, string>> ContentReplaceTagList { get; set; }
	}

	public interface IWebNovelPullerUser
	{
		void OnFileDownloaded(string novelName, string curPara, string nextPara);
	}

	public class WebNovelPuller
	{
		private readonly WebClient _webClient = new WebClient();
		private readonly IWebNovelPullerUser _webNovelPullerUser;
		public bool Exit { get; set; }

		public WebNovelPuller(IWebNovelPullerUser webNovelPullerUser)
		{
			_webNovelPullerUser = webNovelPullerUser;
		}

		public bool DownloadNovel(Novel novel, Rule rule)
		{
			var curFile = novel.StartPara;
			while (curFile != novel.EndPara && !Exit)
				curFile = DownloadTextByUrl(novel.Name, novel.RootUrl, curFile, rule);

			novel.StartPara = curFile;
			return true;
		}

		private static string ExtractIntrested(string content, IEnumerable<string> startTagList, IEnumerable<string> endTagList, IEnumerable<KeyValuePair<string, string>> replacePairs)
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

			// Replace
			content = Replace(content, replacePairs);

			return content;
		}

		private static string Replace(string content, IEnumerable<KeyValuePair<string, string>> replacePairs)
		{
			var translate = new Hashtable();
			translate["[空格]"] = " ";
			translate["[换行]"] = "\r\n";

			return replacePairs.Aggregate(content, (current, replace) => current.Replace(replace.Key, (string)translate[replace.Value]));
		}

		private string DownloadTextByUrl(string novelName, string rootUrl, string fileName, Rule rule)
		{
			string downloadedString;
			if (!DownloadStringByUrl(rootUrl + fileName, out downloadedString))
				return fileName;

			// Biquge
			var title = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["TitleStart"], (List<string>)rule.PositionTag["TitleEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["TitleReplace"]);
			var content = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["ContentStart"], (List<string>)rule.PositionTag["ContentEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["ContentReplace"]);
			var nextUrl = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["NextParaStart"], (List<string>)rule.PositionTag["NextParaEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["NextParaReplace"]);

			var textWriter = File.AppendText("D:\\" + novelName + ".txt");

			textWriter.WriteLine(title);
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

        public bool DownloadPicturesByUrl(string rootUrl, string curImageFile)
        {
            var nextImageFile = curImageFile;
            while (nextImageFile != null)
                nextImageFile = DownloadPictureByUrl(rootUrl, nextImageFile);
            
	        return true;
	    }

	    private string DownloadPictureByUrl(string rootUrl, string curImageFile)
	    {
		    return string.Empty;
		    //try
		    //{
		    //	_webClient.Encoding = System.Text.Encoding.UTF8;
		    //	string text;
		    //	DownloadStringByUrl(rootUrl+curImageFile, out text);

		    //	var rule = new Rule
		    //	{
		    //		WebSite = "114",
		    //		TitleStartTagList = new List<string> { "<h1 class=\"h1-title\">" },
		    //		TitleEndTagList = new List<string> { "</h1>" },
		    //		ConetentStartTagList = new List<string> { "<img src=\"" },
		    //		ContentEndTagList = new List<string> { "\"" },
		    //		NextParaStartTagList = new List<string> { "class=\"picbox\"><a href=\"" },
		    //		NextParaEndTagList = new List<string> { "\"" },
		    //		ContentReplaceTagList = new List<KeyValuePair<string, string>>()
		    //	};


		    //	var title = ExtractIntrested(text, rule.TitleStartTagList, rule.TitleEndTagList);
		    //	var imageUrl = ExtractIntrested(text, rule.ConetentStartTagList, rule.ContentEndTagList);
		    //	var nextUrl = ExtractIntrested(text, rule.NextParaStartTagList, rule.NextParaEndTagList);

		    //	var curImageNumStartTagList = new List<string> { "<strong>" };
		    //	var curImageNumEndTagList = new List<string> { "</strong>" };
		    //	var curImageNum = ExtractIntrested(text, curImageNumStartTagList, curImageNumEndTagList);


		    //	var imageFile = string.Format("{0}{1}{2}.jpg", "D:\\TestOutput3\\", title, curImageNum);
		    //	_webClient.DownloadFile(imageUrl, imageFile);

		    //	if (_webNovelPullerUser != null)
		    //	{
		    //		_webNovelPullerUser.OnFileDownloaded(title, imageFile, nextUrl);
		    //	}

		    //	return nextUrl;

		    //}
		    //catch (Exception ex)
		    //{
		    //	return curImageFile;
		    //} 
	    }
	}
}
