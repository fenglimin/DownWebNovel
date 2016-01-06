using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DownWebNovel.DataAccess;

namespace DownWebNovel
{
	public partial class DownWebNovel : Form, IWebNovelPullerUser
	{
		private List<DownloadTask> _downloadTasks; 
		private Hashtable _rules;
		private Hashtable _translateTags;
		private Hashtable _translateReplace;
		private readonly WebNovelPuller _webNovelPuller;

		delegate void SetTextCallback(string novelName, string curPara, string strNextPara);

		public DownWebNovel()
		{
			InitializeComponent();

			_webNovelPuller = new WebNovelPuller(this);
			_downloadTasks = new List<DownloadTask>();

			_translateTags = new Hashtable();
			_translateTags["标题开始"] = "TitleStart";
			_translateTags["标题结束"] = "TitleEnd";
			_translateTags["正文开始"] = "ContentStart";
			_translateTags["正文结束"] = "ContentEnd";
			_translateTags["下章地址开始"] = "NextParaStart";
			_translateTags["下章地址结束"] = "NextParaEnd";

			_translateReplace = new Hashtable();
			_translateReplace["标题"] = "TitleReplace";
			_translateReplace["正文"] = "ContentReplace";
			_translateReplace["下章地址"] = "NextParaReplace";
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

			//tbName.Text = "超级兵王在都市";
			//tbUrl.Text = "http://www.exiaoshuo.com";
			//tbStartPara.Text = "/chaojibingwangzaidoushi/4175388/";
			//tbEndPara.Text = "/chaojibingwangzaidoushi/";

			foreach (DictionaryEntry translate in _translateTags)
			{
				lbTagDefine.Items.Add(translate.Key);
			}

			foreach (DictionaryEntry translate in _translateReplace)
			{
				lbContentReplace.Items.Add(translate.Key);
			}

		    tbDir.Text = @"D:\";
			LoadRules();
		}

		private void LoadRules()
		{
			_rules = RuleDal.LoadAllRules();

			foreach (DictionaryEntry rule in _rules)
			{
				cbWebSite.Items.Add(rule.Key);
			}
			cbWebSite.SelectedIndex = 0;
		}

		private void cbWebSite_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedRule = _rules[cbWebSite.Text];

			lbTagDefine.SelectedIndex = 0;
			lbContentReplace.SelectedIndex = 2;
		}

		private void lbTagDefine_SelectedIndexChanged(object sender, EventArgs e)
		{
			lbTagValue.DataSource = null;

			var selectedRule = (Rule)_rules[cbWebSite.Text];
			var tags = (List<string>)selectedRule.PositionTag[_translateTags[lbTagDefine.SelectedItem]];

			lbTagValue.DataSource = tags;
		}

		private void btDown_Click(object sender, EventArgs e)
		{
			if (FindDownloadTask(tbName.Text) != null)
			{
				MessageBox.Show("任务已存在");
				return;
			}

			var task = new DownloadTask
			{
				Novel = new Novel
				{
					Name = tbName.Text,
					RootUrl = tbUrl.Text,
					StartPara = tbStartPara.Text,
					EndPara = tbEndPara.Text
				},
				Rule = (Rule)_rules[cbWebSite.Text],
				Thread = new Thread(DownloadNovelThread),
				WebNovelPuller = new WebNovelPuller(this) { Exit = false}
			};

			var lvi = new ListViewItem { Text = "下载中" };
			lvi.SubItems.Add(task.Novel.Name);
			lvi.SubItems.Add("");
			lvi.SubItems.Add("");

			lvDownloadingNovels.Items.Add(lvi);

			_downloadTasks.Add(task);
			task.Thread.Start(task);
		}

		private void DownloadNovelThread(object para)
		{
			var task = para as DownloadTask;
			if (task != null)
				task.WebNovelPuller.DownloadNovel(task.Novel, task.Rule);
		}

		private DownloadTask FindDownloadTask(string novelName)
		{
			return _downloadTasks.FirstOrDefault(downloadTask => downloadTask.Novel.Name == novelName);
		}

		private void SetText(string novelName, string curPara, string nextPara)
		{
			for (var i = 0; i < lvDownloadingNovels.Items.Count; i ++)
			{
				if (lvDownloadingNovels.Items[i].SubItems[1].Text == novelName)
				{
					lvDownloadingNovels.Items[i].SubItems[2].Text = curPara;
					lvDownloadingNovels.Items[i].SubItems[3].Text = nextPara;
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

        private void btSelectDir_Click(object sender, EventArgs e)
        {
            //var dialog = new FolderBrowserDialog {Description = "请选择文件路径"};
            //dialog.SelectedPath = tbDir.Text;

            //if (dialog.ShowDialog() != DialogResult.OK) 
            //    return;

            //tbDir.Text = dialog.SelectedPath; 

            var mywebclient = new WebClient();
            //for (int i = 0; i < 38; i ++)
            //{
            //    var url = string.Format("http://mm.xmeise.com//uploads/allimg/151221/1-1512211113{0}.jpg", i+25);
            //    var filepath = string.Format("D:\\pic{0}.jpg", i);

            //    try
            //    {
            //        mywebclient.DownloadFile(url, filepath); 
            //    }
            //    catch (Exception)
            //    {

            //    }
               
            //}

            //_webNovelPuller.DownloadPicturesByUrl("http://www.renti114.com", "/html/35/3504.html");

            _webNovelPuller.DownloadPicturesByUrl("http://www.renti114.com", "/html/36/3632.html");

        }

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			var novalName = lvDownloadingNovels.SelectedItems[0].SubItems[1].Text;
			var task = FindDownloadTask(novalName);
			if (task == null)
				return;

			if (contextMenuStrip1.Items[0].Text == "停止")
			{
				task.WebNovelPuller.Exit = true;
				lvDownloadingNovels.SelectedItems[0].SubItems[0].Text = "停止";
			}
			else
			{
				lvDownloadingNovels.SelectedItems[0].SubItems[0].Text = "下载中";
				task.WebNovelPuller.Exit = false;
				task.Thread = new Thread(DownloadNovelThread);
				task.Thread.Start(task);
			}

		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			if (lvDownloadingNovels.SelectedItems.Count == 0)
			{
				e.Cancel = true;
				return;
			}

			contextMenuStrip1.Items[0].Text = lvDownloadingNovels.SelectedItems[0].Text == "下载中"? "停止" : "开始";
		}

		private void lbTagValue_SelectedIndexChanged(object sender, EventArgs e)
		{
			tbTagValue.Text = lbTagValue.SelectedItem as string;
		}

		private void btAddTag_Click(object sender, EventArgs e)
		{

		}

		private void btDeleteTag_Click(object sender, EventArgs e)
		{

		}

		private void lbContentReplace_SelectedIndexChanged(object sender, EventArgs e)
		{
			lvReplace.Items.Clear();

			var selectedRule = (Rule)_rules[cbWebSite.Text];
			var replaceList = (List<KeyValuePair<string, string>>)selectedRule.ReplaceTag[_translateReplace[lbContentReplace.SelectedItem]];

			foreach (var replace in replaceList)
			{
				var lvi = new ListViewItem { Text = replace.Key };
				lvi.SubItems.Add(replace.Value);
				lvReplace.Items.Add(lvi);
			}

			lvReplace.Items[0].Selected = true;
			lvReplace.Select();
		}

		private void lvReplace_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (lvReplace.SelectedItems.Count != 1)
				return;

			tbReplaceFrom.Text = lvReplace.SelectedItems[0].SubItems[0].Text;
			tbReplaceTo.Text = lvReplace.SelectedItems[0].SubItems[1].Text;
		}

		private void btAddReplacement_Click(object sender, EventArgs e)
		{

		}

		private void btDeleteReplacement_Click(object sender, EventArgs e)
		{

		}
	}
}
