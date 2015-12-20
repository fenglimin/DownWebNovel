using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownWebNovel
{
	public partial class DownWebNovel : Form, IWebNovelPullerUser
	{
		private List<Rule> _rules;
		private readonly WebNovelPuller _webNovelPuller;

		delegate void SetTextCallback(string novelName, string curPara, string strNextPara);

		public DownWebNovel()
		{
			InitializeComponent();

			_webNovelPuller = new WebNovelPuller(this);
		}

		private void btDownloadFirstPara_Click(object sender, EventArgs e)
		{
			string content;
			var url = tbUrl.Text + tbStartPara.Text;

			tbMessage.Text = "正在从" + url + "下载...\r\n";
			_webNovelPuller.DownloadStringByUrl(url, out content);
			tbMessage.Text += content;
		}

		private void DownWebNovel_Load(object sender, EventArgs e)
		{
			tbName.Text = "圣手邪医";
			tbUrl.Text = "http://www.biquge.la/book/5474/";
			tbStartPara.Text = "3184584.html";
			tbEndPara.Text = "book/5474/"; 
			LoadRules();
		}

		private void LoadRules()
		{
			_rules = new List<Rule>();

			var rule = new Rule
			{
				WebSite = "笔趣阁",
				TitleStartTagList = new List<string> {"<h1>"},
				TitleEndTagList = new List<string> {"</h1>"},
				ConetentStartTagList = new List<string> {"<div id=\"content\"><script>readx();</script>"},
				ContentEndTagList = new List<string> {"</div>"},
				NextParaStartTagList = new List<string> {"章节列表</a> &rarr; <a href=\""},
				NextParaEndTagList = new List<string> {"\">下一章</a>"},
				ReplaceTagList = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("&nbsp;", " "),
					new KeyValuePair<string, string>("<br /><br />", "\r\n")
				}
			};

			_rules.Add(rule);

			cbWebSite.Items.Add(rule.WebSite);
			cbWebSite.SelectedIndex = 0;
		}

		private void cbWebSite_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedRule = _rules.SingleOrDefault(rule => rule.WebSite == cbWebSite.Text);

			lvReplace.Items.Clear();
			if (selectedRule != null)
			{
				foreach (var replace in selectedRule.ReplaceTagList)
				{
					var lvi = new ListViewItem {Text = replace.Key};
					lvi.SubItems.Add(replace.Value);
					lvReplace.Items.Add(lvi);
				}
			}
		}

		private void lbTagDefine_SelectedIndexChanged(object sender, EventArgs e)
		{
			lbTagValue.Items.Clear();

		}

		private void btDown_Click(object sender, EventArgs e)
		{
			var task = new DownloadTask
			{
				Novel = new Novel
				{
					Name = tbName.Text,
					RootUrl = tbUrl.Text,
					StartPara = tbStartPara.Text,
					EndPara = tbEndPara.Text
				},
				Rule = _rules.SingleOrDefault(rule => rule.WebSite == cbWebSite.Text),
				Thread = new Thread(DownloadNovelThread)
			};

			var lvi = new ListViewItem {Text = task.Novel.Name};
			lvi.SubItems.Add("开始下载");

			lvDownloadingNovels.Items.Add(lvi);

			task.Thread.Start(task);
		}

		private void DownloadNovelThread(object para)
		{
			var task = para as DownloadTask;
			if (task != null)
				_webNovelPuller.DownloadNovel(task.Novel, task.Rule);
		}

		private void SetText(string novelName, string curPara, string nextPara)
		{
			for (var i = 0; i < lvDownloadingNovels.Items.Count; i ++)
			{
				if (lvDownloadingNovels.Items[i].Text == novelName)
				{
					lvDownloadingNovels.Items[i].SubItems[1].Text = curPara;
					break;
				}
			}

			tbMessage.AppendText(novelName + " " + curPara + "， 下一章节 " + nextPara + "\r\n");
		}

		public void OnFileDownloaded(string novelName, string curPara, string nextPara)
		{
			if (tbMessage.InvokeRequired)
			{
				var d = new SetTextCallback(SetText);
				Invoke(d, new object[] { novelName, curPara, nextPara });
			}
			else
			{
				SetText(novelName, curPara, nextPara);
			}
		}
	}
}
