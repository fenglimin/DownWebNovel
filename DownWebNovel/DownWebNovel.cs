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
using System.Windows.Forms.VisualStyles;
using DownWebNovel.DataAccess;

namespace DownWebNovel
{
	public partial class DownWebNovel : Form, IWebNovelPullerUser
	{
		private List<Task> _downloadTasks; 
		private Hashtable _rules;
		private Hashtable _translateTags;
		private Hashtable _translateReplace;
		private readonly WebNovelPuller _webNovelPuller;

		delegate void FileDownloadedCallback(string novelName, string curPara, string strNextPara);

	    delegate void TaskStoppedCallback(Task task);

		public DownWebNovel()
		{
			InitializeComponent();

			_webNovelPuller = new WebNovelPuller(this);

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
			foreach (DictionaryEntry translate in _translateTags)
			{
				lbTagDefine.Items.Add(translate.Key);
			}

			foreach (DictionaryEntry translate in _translateReplace)
			{
				lbContentReplace.Items.Add(translate.Key);
			}

		    tbUrl.Text = "http://mm.xmeise.com/rufang/meixiong/5497.html";
		    tbDir.Text = @"D:\";
			LoadRules();
			LoadTasks();
		}

		private void LoadRules()
		{
			_rules = RuleDal.LoadAllRules();

			foreach (DictionaryEntry rule in _rules)
			{
				lbWebSite.Items.Add(rule.Key);
			}
			lbWebSite.SelectedIndex = 0;
		}

		private void LoadTasks()
		{
			_downloadTasks = TaskDal.LoadAllTasks();

			foreach (var downloadTask in _downloadTasks)
			{
				var lvi = new ListViewItem { Text = "停止" };
				lvi.SubItems.Add(downloadTask.TaskName);
				lvi.SubItems.Add(downloadTask.RuleName);
				lvi.SubItems.Add(downloadTask.TaskDir);
				lvi.SubItems.Add(downloadTask.RootUrl);
				lvi.SubItems.Add(downloadTask.ParaStart);
				lvi.SubItems.Add(downloadTask.ParaEnd);
				lvDownloadingNovels.Items.Add(lvi);
			}
		}

		private void btDown_Click(object sender, EventArgs e)
		{
            if (FindTaskInMemory(tbName.Text) != null)
            {
                MessageBox.Show("任务已存在");
                return;
            }

            var downloadTask = new Task
            {
                TaskName = tbName.Text,
                RootUrl = tbUrl.Text,
                TaskDir = tbDir.Text,
                ParaStart = tbStartPara.Text,
                ParaEnd = tbEndPara.Text,
                RuleName = lbWebSite.SelectedItem.ToString()
            };

            var lvi = new ListViewItem { Text = "停止" };
            lvi.SubItems.Add(downloadTask.TaskName);
            lvi.SubItems.Add(downloadTask.RuleName);
            lvi.SubItems.Add(downloadTask.TaskDir);
            lvi.SubItems.Add(downloadTask.RootUrl);
            lvi.SubItems.Add(downloadTask.ParaStart);
            lvi.SubItems.Add(downloadTask.ParaEnd);
            lvDownloadingNovels.Items.Add(lvi);

            _downloadTasks.Add(downloadTask);

            TaskDal.AddTask(downloadTask);

            StartTaskInTheList(downloadTask.TaskName);
		}

		private void DownloadNovelThread(object para)
		{
			var task = para as Task;
			if (task != null)
				task.WebNovelPuller.DownloadNovel(task);
		}

		private Task FindTaskInMemory(string novelName)
		{
			return _downloadTasks.FirstOrDefault(downloadTask => downloadTask.TaskName == novelName);
		}

	    private int FindTaskItemItemInTheList(string taskName)
	    {
            var item = -1;
            for (var i = 0; i < lvDownloadingNovels.Items.Count; i++)
            {
                if (lvDownloadingNovels.Items[i].SubItems[1].Text == taskName)
                {
                    item = i;
                }
            }

	        return item;
	    }

		private void FileDownloaded(string taskName, string curPara, string nextPara)
		{
			tbMessage.AppendText(taskName + " " + curPara + "， 下一章节 " + nextPara + "\r\n");

            var item = FindTaskItemItemInTheList(taskName);
            if (item == -1)
                return;

            lvDownloadingNovels.Items[item].SubItems[5].Text = nextPara;
		}

		public void OnFileDownloaded(string novelName, string curPara, string nextPara)
		{
			if (tbMessage.InvokeRequired)
			{
				var d = new FileDownloadedCallback(FileDownloaded);
				Invoke(d, new object[] { novelName, curPara, nextPara });
			}
			else
			{
				FileDownloaded(novelName, curPara, nextPara);
			}
		}

	    private void TaskStopped(Task task)
	    {
            tbMessage.AppendText(task.TaskName + " " + "下载已停止！" + "\r\n");

            TaskDal.DeleteTask(task.TaskName);
            TaskDal.AddTask(task);

            var item = FindTaskItemItemInTheList(task.TaskName);
            if (item == -1)
                return;

            lvDownloadingNovels.Items[item].SubItems[0].Text = "停止";

            
	    }

	    public void OnTaskStopped(Task task)
	    {
	        if (lvDownloadingNovels.InvokeRequired)
	        {
	            var d = new TaskStoppedCallback(TaskStopped);
	            Invoke(d, new object[] {task});
	        }
	        else
	        {
	            TaskStopped(task);
	        }
	    }

	    private void btSelectDir_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { Description = "请选择文件路径" };
            dialog.SelectedPath = tbDir.Text;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            tbDir.Text = dialog.SelectedPath; 
        }

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}

	    private void StartTaskInTheList(string taskName)
	    {
            var item = FindTaskItemItemInTheList(taskName);
            if (item == -1)
                return;

	        if (lvDownloadingNovels.Items[item].SubItems[0].Text == "下载中")
	            return;

	        var task = FindTaskInMemory(taskName);
	        if (task == null)
	            return;

            lvDownloadingNovels.Items[item].SubItems[0].Text = "下载中";
            if (task.WebNovelPuller == null)
            {
                task.WebNovelPuller = new WebNovelPuller(this) { Exit = false };
            }
            else
            {
                task.WebNovelPuller.Exit = false;
            }

            task.Rule = (Rule)_rules[task.RuleName];
            task.Thread = new Thread(DownloadNovelThread);
            task.Thread.Start(task);
	    }

	    private void StopTaskInTheList(string taskName)
	    {
            var item = FindTaskItemItemInTheList(taskName);
            if (item == -1)
                return;

	        if (lvDownloadingNovels.Items[item].SubItems[0].Text.StartsWith("停止"))
	            return;

            var task = FindTaskInMemory(taskName);
            if (task == null)
                return;

            if (task.WebNovelPuller != null)
                task.WebNovelPuller.Exit = true;
            lvDownloadingNovels.Items[item].SubItems[0].Text = "停止中";
	    }

	    private void DeleteTaskInTheList(string taskName)
	    {
            var task = FindTaskInMemory(taskName);
            if (task == null)
                return;

            var item = FindTaskItemItemInTheList(taskName);
            if (item == -1)
                return;

	        _downloadTasks.Remove(task);
            lvDownloadingNovels.Items.RemoveAt(item);
            TaskDal.DeleteTask(taskName);
	    }

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
		    var visible = lvDownloadingNovels.SelectedItems.Count != 0;
		    var downloading = true;
		    
            if (visible)
                downloading = lvDownloadingNovels.SelectedItems[0].Text == "下载中";

            contextMenuStrip1.Items[0].Visible = visible && !downloading;
            contextMenuStrip1.Items[1].Visible = visible && downloading;
            contextMenuStrip1.Items[2].Visible = visible && !downloading;
            contextMenuStrip1.Items[3].Visible = visible;
		}

		#region WebSite

		private void RefershWebSiteButtons()
		{
			var contain = lbWebSite.Items.Contains(tbWebSite.Text);
			btAddWebSite.Enabled = !contain && tbWebSite.Text != string.Empty;
			btDeleteWebSite.Enabled = contain && tbWebSite.Text != string.Empty;
		}

		private void lbWebSite_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lbWebSite.SelectedItem == null)
				return;

			tbWebSite.Text = lbWebSite.SelectedItem.ToString();

			if (lbTagDefine.SelectedIndex == 0)
				OnTagDefineSelectedIndexChanged();
			else
				lbTagDefine.SelectedIndex = 0;

			lbContentReplace.SelectedIndex = 2;
		}

		private void tbWebSite_TextChanged(object sender, EventArgs e)
		{
			RefershWebSiteButtons();
		}

		private void btAddWebSite_Click(object sender, EventArgs e)
		{
			var index = lbWebSite.Items.Add(tbWebSite.Text);

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			RuleDal.CopyRule(tbWebSite.Text, selectedRule);

			_rules[tbWebSite.Text] = selectedRule.Clone();

			lbWebSite.SelectedIndex = index;

			RefershWebSiteButtons();
		}

		private void btDeleteWebSite_Click(object sender, EventArgs e)
		{
			var value = tbWebSite.Text;
			lbWebSite.Items.Remove(value);

			_rules.Remove(value);

			RuleDal.DeleteRule(value);

			if (lbWebSite.Items.Count > 0)
				lbWebSite.SelectedIndex = 0;

			RefershWebSiteButtons();
		}

		#endregion

		#region	Tag

		private void RefershTagButtons()
		{
			var contain = lbTagValue.Items.Contains(tbTagValue.Text);
			btAddTag.Enabled = !contain && tbTagValue.Text != string.Empty;
			btDeleteTag.Enabled = contain && tbTagValue.Text != string.Empty;
		}

		private void OnTagDefineSelectedIndexChanged()
		{
			lbTagValue.Items.Clear();

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var tags = (List<string>)selectedRule.PositionTag[_translateTags[lbTagDefine.SelectedItem]];

			foreach (var tag in tags)
			{
				lbTagValue.Items.Add(tag);
			}

			if (lbTagValue.Items.Count > 0)
				lbTagValue.SelectedIndex = 0;
		}

		private void lbTagDefine_SelectedIndexChanged(object sender, EventArgs e)
		{
			OnTagDefineSelectedIndexChanged();
		}

		private void lbTagValue_SelectedIndexChanged(object sender, EventArgs e)
		{
			tbTagValue.Text = lbTagValue.SelectedItem as string;
		}

		private void tbTagValue_TextChanged(object sender, EventArgs e)
		{
			RefershTagButtons();
		}

		private void btAddTag_Click(object sender, EventArgs e)
		{
			var index = lbTagValue.Items.Add(tbTagValue.Text);


			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var tags = (List<string>)selectedRule.PositionTag[_translateTags[lbTagDefine.SelectedItem]];
			tags.Add(tbTagValue.Text);

			RuleDal.AddRuleItem(tbWebSite.Text, _translateTags[lbTagDefine.SelectedItem].ToString(), tbTagValue.Text, string.Empty);

			lbTagValue.SelectedIndex = index;

			RefershTagButtons();
		}

		private void btDeleteTag_Click(object sender, EventArgs e)
		{
			var value = tbTagValue.Text;
			lbTagValue.Items.Remove(value);

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var tags = (List<string>)selectedRule.PositionTag[_translateTags[lbTagDefine.SelectedItem]];
			tags.Remove(value);

			RuleDal.DeleteRuleItem(tbWebSite.Text, _translateTags[lbTagDefine.SelectedItem].ToString(), value);

			if (lbTagValue.Items.Count > 0)
				lbTagValue.SelectedIndex = 0;

			RefershTagButtons();
		}
		#endregion

		#region	Replace

		private void RefershRepalceButtons()
		{
			var contain = lvReplace.Items.Cast<ListViewItem>().Any(item => item.Text == tbReplaceFrom.Text);
			btAddReplacement.Enabled = !contain && tbReplaceFrom.Text != string.Empty && tbReplaceTo.Text != string.Empty;
			btDeleteReplacement.Enabled = contain && tbReplaceFrom.Text != string.Empty && tbReplaceTo.Text != string.Empty;
		}

		private void lbContentReplace_SelectedIndexChanged(object sender, EventArgs e)
		{
			lvReplace.Items.Clear();

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var replaceList = (List<KeyValuePair<string, string>>)selectedRule.ReplaceTag[_translateReplace[lbContentReplace.SelectedItem]];

			foreach (var replace in replaceList)
			{
				var lvi = new ListViewItem { Text = replace.Key };
				lvi.SubItems.Add(replace.Value);
				lvReplace.Items.Add(lvi);
			}

			if (lvReplace.Items.Count > 0)
			{
				lvReplace.Items[0].Selected = true;
				lvReplace.Select();
			}
			else
			{
				tbReplaceFrom.Text = string.Empty;
				tbReplaceTo.Text = string.Empty;
				RefershRepalceButtons();
			}
		}

		private void lvReplace_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (lvReplace.SelectedItems.Count != 1)
				return;

			tbReplaceFrom.Text = lvReplace.SelectedItems[0].SubItems[0].Text;
			tbReplaceTo.Text = lvReplace.SelectedItems[0].SubItems[1].Text;

			RefershRepalceButtons();
		}

		private void tbReplaceFrom_TextChanged(object sender, EventArgs e)
		{
			RefershRepalceButtons();
		}

		private void btAddReplacement_Click(object sender, EventArgs e)
		{
			var lvi = new ListViewItem { Text = tbReplaceFrom.Text };
			lvi.SubItems.Add(tbReplaceTo.Text);
			lvReplace.Items.Add(lvi);
			lvi.Selected = true;

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var replaceList = (List<KeyValuePair<string, string>>)selectedRule.ReplaceTag[_translateReplace[lbContentReplace.SelectedItem]];
			replaceList.Add(new KeyValuePair<string, string>(tbReplaceFrom.Text, tbReplaceTo.Text));

			RuleDal.AddRuleItem(tbWebSite.Text, _translateReplace[lbContentReplace.SelectedItem].ToString(), tbReplaceFrom.Text, tbReplaceTo.Text);

			RefershRepalceButtons();
		}

		private void btDeleteReplacement_Click(object sender, EventArgs e)
		{
			if (lvReplace.SelectedItems.Count == 0)
				return;

			var from = tbReplaceFrom.Text;
			var to = tbReplaceTo.Text;

			lvReplace.Items.Remove(lvReplace.SelectedItems[0]);

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var replaceList = (List<KeyValuePair<string, string>>)selectedRule.ReplaceTag[_translateReplace[lbContentReplace.SelectedItem]];
			replaceList.Remove(new KeyValuePair<string, string>(from, to));

			RuleDal.DeleteRuleItem(tbWebSite.Text, _translateReplace[lbContentReplace.SelectedItem].ToString(), from);

			if (lvReplace.Items.Count > 0)
			{
				lvReplace.Items[0].Selected = true;
				lvReplace.Select();
			}

			RefershRepalceButtons();
		}

		#endregion

        private void lvDownloadingNovels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvDownloadingNovels.SelectedItems.Count == 0)
                return;

            var subItems = lvDownloadingNovels.SelectedItems[0].SubItems;
            tbName.Text = subItems[1].Text;
            tbDir.Text = subItems[3].Text;
            tbUrl.Text = subItems[4].Text;
            tbStartPara.Text = subItems[5].Text;
            tbEndPara.Text = subItems[6].Text;

        }

	    private string GetSelectTaskName()
	    {
	        return lvDownloadingNovels.SelectedItems.Count == 0 ? string.Empty : lvDownloadingNovels.SelectedItems[0].SubItems[1].Text;
	    }

	    private void startMenuItem_Click(object sender, EventArgs e)
	    {
	        var taskName = GetSelectTaskName();
            StartTaskInTheList(taskName);
	    }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            var taskName = GetSelectTaskName();
            StopTaskInTheList(taskName);
        }

        private void deleteMenuItem_Click(object sender, EventArgs e)
        {
            var taskName = GetSelectTaskName();
            DeleteTaskInTheList(taskName);
        }

        private void startAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvDownloadingNovels.Items)
            {
                StartTaskInTheList(item.SubItems[1].Text);
            }
        }

        private void stopAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvDownloadingNovels.Items)
            {
                StopTaskInTheList(item.SubItems[1].Text);
            }
        }


	}
}
