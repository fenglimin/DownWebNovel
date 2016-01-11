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

	public class Task
	{
		public string TaskName { get; set; }
		public string RootUrl { get; set; }
		public string ParaStart { get; set; }
		public string ParaEnd { get; set; }
		public string TaskDir { get; set; }
		public string RuleName { get; set; }
		public bool IsPicture { get; set; }
		public string PictureUrlPrefix { get; set; }

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
		    var rule = new Rule {WebSite = WebSite, PositionTag = new Hashtable(), ReplaceTag = new Hashtable()};

		    foreach (DictionaryEntry dictionaryEntry in PositionTag)
		    {
		        var originalItems = (List<string>) dictionaryEntry.Value;
		        var items = originalItems.Select(originalItem => originalItem.Clone().ToString()).ToList();
		        rule.PositionTag[dictionaryEntry.Key] = items;
		    }

            
		    foreach (DictionaryEntry dictionaryEntry in ReplaceTag)
		    {
		        var originalItems = (List<KeyValuePair<string, string>>) dictionaryEntry.Value;
		        var items = originalItems.Select(originalItem => new KeyValuePair<string, string>(originalItem.Key, originalItem.Value)).ToList();
		        rule.ReplaceTag[dictionaryEntry.Key] = items;
		    }

		    return rule;

		}
	}

	public interface IWebNovelPullerUser
	{
		void OnFileDownloaded(bool hasError, string novelName, string curPara, string nextPara);
	    void OnTaskStopped(Task task);
	}

	public class WebNovelPuller
	{
		private readonly XWebClient _webClient = new XWebClient();
		private readonly IWebNovelPullerUser _webNovelPullerUser;
		public bool Exit { get; set; }

		public WebNovelPuller(IWebNovelPullerUser webNovelPullerUser)
		{
			_webNovelPullerUser = webNovelPullerUser;
		}

		//public bool DownloadNovel(Task task)
		//{
		//	var curFile = task.ParaStart;
		//	while (curFile != task.ParaEnd && !Exit)
		//		curFile = DownloadNovelByUrl(task.TaskName, task.RootUrl, curFile, task.Rule, task.TaskDir);

		//	task.ParaStart = curFile;
		//	if (_webNovelPullerUser != null)
		//		_webNovelPullerUser.OnTaskStopped(task);

		//	return true;
		//}

		public bool RunTask(Task task)
		{
			while (task.ParaStart != task.ParaEnd && !Exit)
			{
				task.ParaStart = task.RuleName.StartsWith("task_") ? CreateTaskFromTask(task) : DownloadCurrentPage(task);
			}

			if (_webNovelPullerUser != null)
				_webNovelPullerUser.OnTaskStopped(task);

			return true;
		}

		private string CreateTaskFromTask(Task task)
		{
			string downloadedString;
			if (!DownloadStringByUrl(task.RootUrl + task.ParaStart, out downloadedString))
			{
				if (_webNovelPullerUser != null)
				{
					_webNovelPullerUser.OnFileDownloaded(true, task.TaskName, "下载错误", downloadedString);
				}
				return task.ParaStart;
			}

			var newTaskRule = task.RuleName.Substring(5);
			var sampleTask = TaskDal.GetSampleTaskByRule(newTaskRule);

			var taskUrlList = ExtractIntrestedList(downloadedString, task.Rule, "Content");
			foreach (var taskUrl in taskUrlList)
			{
				var subTask = new Task()
				{
					TaskName = task.TaskName + "__" + Guid.NewGuid(),
					IsPicture = true,
					ParaEnd = sampleTask.ParaEnd,
					ParaStart = taskUrl,
					PictureUrlPrefix = sampleTask.PictureUrlPrefix,
					RootUrl = task.PictureUrlPrefix,
					RuleName = newTaskRule,
					TaskDir = task.TaskDir
				};

				TaskDal.AddTask(subTask);
			}

			var nextUrl = ExtractIntrested(downloadedString, task.Rule, "NextPara");

			return nextUrl;
		}

		private string DownloadCurrentPage(Task task)
		{
			string downloadedString;
			if (!DownloadStringByUrl(task.RootUrl + task.ParaStart, out downloadedString))
			{
				if (_webNovelPullerUser != null)
				{
					_webNovelPullerUser.OnFileDownloaded(true, task.TaskName, "下载错误", downloadedString);
				}
				return task.ParaStart;
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
					fileName = task.TaskDir + title + "_" + fileName;
					_webClient.DownloadFile(pictureUrl, fileName);
				}
				else
				{
					var textWriter = File.AppendText(task.TaskDir + task.TaskName + ".txt");
					textWriter.WriteLine(title);
					textWriter.WriteLine(content);
					textWriter.Close();
				}

				if (_webNovelPullerUser != null)
				{
					_webNovelPullerUser.OnFileDownloaded(false, task.TaskName, title, nextUrl);
				}

				return nextUrl;
			}
			catch (Exception ex)
			{
				if (_webNovelPullerUser != null)
				{
					_webNovelPullerUser.OnFileDownloaded(true, task.TaskName, "解析错误", ex.Message);
				}
				return task.ParaStart;
			}
		}

		private static List<string> ExtractIntrestedList(string content, Rule rule, string key)
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
					throw new Exception("Error finding end tag of " + key);

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

	    private static string ExtractIntrested(string content, Rule rule, string key)
	    {
	        var startTagList = (List<string>) rule.PositionTag[key + "Start"];
            var endTagList = (List<string>)rule.PositionTag[key + "End"];
            var replacePairs = (List<KeyValuePair<string, string>>)rule.ReplaceTag[key + "Replace"];

	        var startIndex = FindIndex(content, startTagList, false);
            

	        if (startIndex != -1)
	        {
                content = content.Substring(startIndex);

                var endIndex = FindIndex(content, endTagList, true);
                if (endIndex == -1)
                    throw new Exception("Error finding end tag of " + key);

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
                throw new Exception("Error finding start tag of " + key);
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
	        var tags = tagSequence.Split( new string[] { " => "}, StringSplitOptions.RemoveEmptyEntries);
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

	        return index;
	    }

        //private static string ExtractIntrested(string content, IEnumerable<string> startTagList, IEnumerable<string> endTagList, IEnumerable<KeyValuePair<string, string>> replacePairs)
        //{
        //    var startIndex = -1;
        //    var startTagLen = 0;
        //    foreach (var startTag in startTagList)
        //    {
        //        //startIndex = content.IndexOf(startTag, StringComparison.Ordinal);
        //        startIndex = IndexOf(content, startTag);
        //        if (startIndex != -1)
        //        {
        //            //startTagLen = startTag.Length;
        //            break;
        //        }
        //    }

        //    if (startIndex == -1)
        //        throw new Exception("Error finding start tag " + startTagList.ToString());

        //    content = content.Substring(startIndex + startTagLen);


        //    var endIndex = -1;
        //    foreach (var endTag in endTagList)
        //    {
        //        endIndex = content.IndexOf(endTag, StringComparison.Ordinal);
        //        if (endIndex != -1)
        //        {
        //            break;
        //        }
        //    }

        //    if (endIndex == -1)
        //        throw new Exception("Error finding end tag " + endTagList.ToString());

        //    content = content.Substring(0, endIndex);

        //    // Replace
        //    content = Replace(content, replacePairs);

        //    return content;
        //}

		private static string Replace(string content, IEnumerable<KeyValuePair<string, string>> replacePairs)
		{
			var translate = new Hashtable();
			translate["[空格]"] = " ";
			translate["[换行]"] = "\r\n";

			return replacePairs.Aggregate(content, (current, replace) => current.Replace(replace.Key, (string)translate[replace.Value]));
		}

		private string DownloadNovelByUrl(string novelName, string rootUrl, string fileName, Rule rule, string taskDir)
		{
			string downloadedString;
		    if (!DownloadStringByUrl(rootUrl + fileName, out downloadedString))
		    {
                if (_webNovelPullerUser != null)
                {
                    _webNovelPullerUser.OnFileDownloaded(true, novelName, "下载错误", downloadedString);
                }
                return fileName;
		    }

		    try
		    {
                //var title = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["TitleStart"], (List<string>)rule.PositionTag["TitleEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["TitleReplace"]);
                //var content = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["ContentStart"], (List<string>)rule.PositionTag["ContentEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["ContentReplace"]);
                //var nextUrl = ExtractIntrested(downloadedString, (List<string>)rule.PositionTag["NextParaStart"], (List<string>)rule.PositionTag["NextParaEnd"], (List<KeyValuePair<string, string>>)rule.ReplaceTag["NextParaReplace"]);

                var title = ExtractIntrested(downloadedString, rule, "Title");
                var content = ExtractIntrested(downloadedString, rule, "Content");
                var nextUrl = ExtractIntrested(downloadedString, rule, "NextPara");


                var textWriter = File.AppendText(taskDir + novelName + ".txt");

                textWriter.WriteLine(title);
                textWriter.WriteLine(content);

                if (_webNovelPullerUser != null)
                {
                    _webNovelPullerUser.OnFileDownloaded(false, novelName, title, nextUrl);
                }

                textWriter.Close();
                return nextUrl;
		    }
		    catch (Exception ex)
		    {
                if (_webNovelPullerUser != null)
                {
                    _webNovelPullerUser.OnFileDownloaded(true, novelName, "解析错误", ex.Message);
                }
		        return fileName;
		    }

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
