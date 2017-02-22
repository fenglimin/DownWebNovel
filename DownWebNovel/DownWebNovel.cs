using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
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
		private bool _showCompletedBooks;
		private List<Task> _downloadTasks; 
		private Hashtable _rules;
		private Hashtable _translateTags;
		private Hashtable _translateReplace;
		private readonly WebNovelPuller _webNovelPuller;
		private ShowWatchedNovel _showWatchNovelForm = new ShowWatchedNovel();

		delegate void ShowMessageCallback(Task task, bool isDownloadError, string errorMessage);
	    delegate void TaskOperationCallback(Task task);
		delegate void TaskStoppedCallback(Task task, string stopReason);

		public DownWebNovel()
		{
			InitializeComponent();

			_showCompletedBooks = false;

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

		private void DownloadOnePara(string url, bool downloadSourceOnly)
		{
			string content;
			tbMessage.Text = "正在从" + url + "下载...\r\n";

			if (downloadSourceOnly)
				_webNovelPuller.DownloadStringByUrl(url, out content);
			else
			{
				var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
				content = _webNovelPuller.DownloadPageForVerify(url, selectedRule);
			}

			tbMessage.Text += content + "\r\n";
		}

		private void btDownloadFirstPara_Click(object sender, EventArgs e)
		{
			DownloadOnePara(tbUrl.Text + tbStartPara.Text, cbParaStartSource.Checked);
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

		    tbDir.Text = @"D:\Temp\Novel";
			LoadRules();
			LoadTasks();

			if (lvDownloadingNovels.Items.Count > 0)
			{
				lvDownloadingNovels.Items[0].Selected = true;
			}
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

		private ListViewItem FillListViewItemWithTask(Task task)
		{
			var lvi = new ListViewItem { Text = "停止" };
			lvi.SubItems.Add(task.TaskName);
			lvi.SubItems.Add(task.RuleName);
			lvi.SubItems.Add(task.ParaTitleLastDownloaded);
			lvi.SubItems.Add(task.ParaUrlLastDownloaded);
			lvi.SubItems.Add(task.RootUrl);
			lvi.SubItems.Add(task.ParaUrlStart);
			lvi.SubItems.Add(task.ParaUrlEnd);
			lvi.SubItems.Add(task.TaskDir);
			lvi.SubItems.Add(task.IsPicture ? "是" : "否");
			lvi.SubItems.Add(task.PictureUrlPrefix);
			lvi.SubItems.Add(task.IsBookCompleted ? "是" : "否");
			lvi.SubItems.Add(task.IsBookWatched ? "是" : "否");

			return lvi;
		}

		private void LoadTasks()
		{
			_downloadTasks = TaskDal.LoadAllTasks();

			foreach (var downloadTask in _downloadTasks.OrderBy(x => x.IsBookCompleted))
			{
				if (downloadTask.IsBookCompleted && !_showCompletedBooks)
					continue;

				var lvi = FillListViewItemWithTask(downloadTask);
				lvDownloadingNovels.Items.Add(lvi);
			}
		}

		private void AddTaskToViewAndMemoryAndDatabase(Task task)
		{
			var lvi = FillListViewItemWithTask(task);
			lvDownloadingNovels.Items.Add(lvi);

			_downloadTasks.Add(task);

			TaskDal.AddTask(task);

		}

		private void btDown_Click(object sender, EventArgs e)
		{
			var downloadTask = new Task
			{
				TaskName = tbName.Text,
				RootUrl = tbUrl.Text,
				TaskDir = tbDir.Text,
				ParaUrlStart = tbStartPara.Text,
				ParaUrlLastDownloaded = tbParaLastDownloaded.Text,
				ParaUrlEnd = tbEndPara.Text,
				RuleName = lbWebSite.SelectedItem.ToString(),
				IsPicture = cbIsPicture.Checked,
				PictureUrlPrefix = tbPictureUrlPrefix.Text,
				IsBookCompleted = cbIsBookCompleted.Checked,
				IsBookWatched = cbIsBookWatched.Checked
			};

            if (FindTaskInMemory(tbName.Text) != null)
            {
                DeleteTaskInTheList(downloadTask.TaskName);
            }
           

			AddTaskToViewAndMemoryAndDatabase(downloadTask);
            //RunTask(downloadTask.TaskName, false);
		}

		private void DownloadNovelThread(object para)
		{
			var task = para as Task;
			if (task != null)
				task.WebNovelPuller.RunTask(task);
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

		private void ShowMessage(Task task, bool isDownloadError, string errorMessage)
		{
			if (isDownloadError && !cbShowError.Checked)
				return;

			tbMessage.AppendText(task.TaskName + "  " + errorMessage + "\r\n");
		}


		public void OnShowMessage(Task task, bool isDownloadError, string errorMessage)
		{
			if (tbMessage.InvokeRequired)
			{
				var d = new ShowMessageCallback(ShowMessage);
				Invoke(d, new object[] { task, isDownloadError, errorMessage });
			}
			else
			{
				ShowMessage(task, isDownloadError, errorMessage);
			}
		}

        private void FileDownloaded(Task task)
		{
            tbMessage.AppendText(task.TaskName + " " + task.ParaTitleLastDownloaded + "， 下一章节 " + task.ParaUrlNextToDownload + "\r\n");

            var item = FindTaskItemItemInTheList(task.TaskName);
            if (item == -1)
                return;

			lvDownloadingNovels.Items[item].SubItems[3].Text = task.ParaTitleLastDownloaded;
            lvDownloadingNovels.Items[item].SubItems[4].Text = task.ParaUrlLastDownloaded;

			_showWatchNovelForm.AddPara(task);
		}

        public void OnFileDownloaded(Task task)
		{
			if (tbMessage.InvokeRequired)
			{
				var d = new TaskOperationCallback(FileDownloaded);
				Invoke(d, new object[] { task });
			}
			else
			{
				FileDownloaded(task);
			}
		}

		private void TaskStopped(Task task, string stopReason)
	    {
            tbMessage.AppendText(task.TaskName + " " + "下载停止！ --- " + stopReason +"\r\n");

            TaskDal.DeleteTask(task.TaskName);
            TaskDal.AddTask(task);

            var item = FindTaskItemItemInTheList(task.TaskName);
            if (item == -1)
                return;

            lvDownloadingNovels.Items[item].SubItems[0].Text = "停止";
	    }

		public void OnTaskStopped(Task task, string stopReason)
	    {
	        if (lvDownloadingNovels.InvokeRequired)
	        {
	            var d = new TaskStoppedCallback(TaskStopped);
				Invoke(d, new object[] { task, stopReason });
	        }
	        else
	        {
	            TaskStopped(task, stopReason);
	        }
	    }


		private void SubTaskCreated(Task task)
		{
			tbMessage.AppendText(task.TaskName + " 已生成！\r\n");
			AddTaskToViewAndMemoryAndDatabase(task);
		}

		public void OnSubTaskCreated(Task task)
		{
			if (lvDownloadingNovels.InvokeRequired)
			{
				var d = new TaskOperationCallback(SubTaskCreated);
				Invoke(d, new object[] { task });
			}
			else
			{
				SubTaskCreated(task);
			}
		}

		private void btSelectDir_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog {Description = "请选择文件路径", SelectedPath = tbDir.Text};

			if (dialog.ShowDialog() != DialogResult.OK)
                return;

            tbDir.Text = dialog.SelectedPath; 
        }

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}

		private void RunTask(string taskName, bool isRestart)
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
				task.WebNovelPuller = new WebNovelPuller(this) { ExitRequestedByUser = false };
			}
			else
			{
				task.WebNovelPuller.ExitRequestedByUser = false;
			}

			if (isRestart)
			{
				lvDownloadingNovels.Items[item].SubItems[4].Text = string.Empty;
				task.ParaUrlLastDownloaded = string.Empty;
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
                task.WebNovelPuller.ExitRequestedByUser = true;
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

			if (lvDownloadingNovels.Items[item].SubItems[0].Text != "停止")
				return;

	        _downloadTasks.Remove(task);
            lvDownloadingNovels.Items.RemoveAt(item);
            TaskDal.DeleteTask(taskName);
	    }

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			var count = lvDownloadingNovels.SelectedItems.Count;

			if (count == 0)
			{
				contextMenuStrip1.Items[0].Visible = false;
				contextMenuStrip1.Items[1].Visible = false;
				contextMenuStrip1.Items[2].Visible = false;
				contextMenuStrip1.Items[3].Visible = false;
				contextMenuStrip1.Items[4].Visible = false;
				contextMenuStrip1.Items[5].Visible = false;
				contextMenuStrip1.Items[6].Visible = true;
				contextMenuStrip1.Items[7].Visible = true;
				contextMenuStrip1.Items[8].Visible = true;
				contextMenuStrip1.Items[9].Visible = false;
				contextMenuStrip1.Items[10].Visible = true;
				contextMenuStrip1.Items[11].Visible = true;
				contextMenuStrip1.Items[12].Visible = true;
			}
			else if (count == 1)
			{
				var stopped = lvDownloadingNovels.SelectedItems[0].Text == "停止";
				contextMenuStrip1.Items[0].Visible = stopped;
				contextMenuStrip1.Items[1].Visible = !stopped;
				contextMenuStrip1.Items[2].Visible = stopped;
				contextMenuStrip1.Items[3].Visible = stopped;
				contextMenuStrip1.Items[4].Visible = stopped;
				contextMenuStrip1.Items[5].Visible = false;
				contextMenuStrip1.Items[6].Visible = false;
				contextMenuStrip1.Items[7].Visible = false;
				contextMenuStrip1.Items[8].Visible = true;
				contextMenuStrip1.Items[9].Visible = true;
				contextMenuStrip1.Items[10].Visible = false;
				contextMenuStrip1.Items[11].Visible = true;
				contextMenuStrip1.Items[12].Visible = true;
			}
			else
			{
				contextMenuStrip1.Items[0].Visible = true;
				contextMenuStrip1.Items[1].Visible = true;
				contextMenuStrip1.Items[2].Visible = true;
				contextMenuStrip1.Items[3].Visible = false;
				contextMenuStrip1.Items[4].Visible = false;
				contextMenuStrip1.Items[5].Visible = false;
				contextMenuStrip1.Items[6].Visible = false;
				contextMenuStrip1.Items[7].Visible = false;
				contextMenuStrip1.Items[8].Visible = true;
				contextMenuStrip1.Items[9].Visible = true;
				contextMenuStrip1.Items[10].Visible = false;
				contextMenuStrip1.Items[11].Visible = true;
				contextMenuStrip1.Items[12].Visible = true;
			}
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

			if (lbContentReplace.SelectedIndex == 2)
				OnContentReplaceSelectedIndexChanged();
			else
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
			btAddReplacement.Enabled = !contain && tbReplaceFrom.Text != string.Empty && cbReplaceTo.Text != string.Empty;
			btDeleteReplacement.Enabled = contain && tbReplaceFrom.Text != string.Empty && cbReplaceTo.Text != string.Empty;
		}

		private void OnContentReplaceSelectedIndexChanged()
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
				cbReplaceTo.Text = string.Empty;
				RefershRepalceButtons();
			}
		}

		private void lbContentReplace_SelectedIndexChanged(object sender, EventArgs e)
		{
			OnContentReplaceSelectedIndexChanged();
		}

		private void lvReplace_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (lvReplace.SelectedItems.Count != 1)
				return;

			tbReplaceFrom.Text = lvReplace.SelectedItems[0].SubItems[0].Text;
			cbReplaceTo.Text = lvReplace.SelectedItems[0].SubItems[1].Text;

			RefershRepalceButtons();
		}

		private void tbReplaceFrom_TextChanged(object sender, EventArgs e)
		{
			RefershRepalceButtons();
		}

		private void btAddReplacement_Click(object sender, EventArgs e)
		{
			var lvi = new ListViewItem { Text = tbReplaceFrom.Text };
			lvi.SubItems.Add(cbReplaceTo.Text);
			lvReplace.Items.Add(lvi);
			lvi.Selected = true;

			var selectedRule = (Rule)_rules[lbWebSite.SelectedItem];
			var replaceList = (List<KeyValuePair<string, string>>)selectedRule.ReplaceTag[_translateReplace[lbContentReplace.SelectedItem]];
			replaceList.Add(new KeyValuePair<string, string>(tbReplaceFrom.Text, cbReplaceTo.Text));

			RuleDal.AddRuleItem(tbWebSite.Text, _translateReplace[lbContentReplace.SelectedItem].ToString(), tbReplaceFrom.Text, cbReplaceTo.Text);

			RefershRepalceButtons();
		}

		private void btDeleteReplacement_Click(object sender, EventArgs e)
		{
			if (lvReplace.SelectedItems.Count == 0)
				return;

			var from = tbReplaceFrom.Text;
			var to = cbReplaceTo.Text;

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

	        var taskName = GetSelectTaskNames()[0];
	        var task = FindTaskInMemory(taskName);
	        if (task == null)
		        return;

	        tbName.Text = task.TaskName;
	        tbDir.Text = task.TaskDir;
	        tbUrl.Text = task.RootUrl;
	        tbStartPara.Text = task.ParaUrlStart;
	        tbParaLastDownloaded.Text = task.ParaUrlLastDownloaded;
	        tbEndPara.Text = task.ParaUrlEnd;
	        cbIsPicture.Checked = task.IsPicture;
	        tbPictureUrlPrefix.Text = task.PictureUrlPrefix;
	        cbIsBookCompleted.Checked = task.IsBookCompleted;
	        cbIsBookWatched.Checked = task.IsBookWatched;

	        btDown.Text = "更改";

	        lbWebSite.SelectedItem = task.RuleName;
        }

	    private IList<string> GetSelectTaskNames()
	    {
		    return (from ListViewItem selectedItem in lvDownloadingNovels.SelectedItems select selectedItem.SubItems[1].Text).ToList();
	    }

		private void startMenuItem_Click(object sender, EventArgs e)
	    {
			foreach (var selectTaskName in GetSelectTaskNames())
			{
				RunTask(selectTaskName, false);
			}
	    }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
	        foreach (var selectTaskName in GetSelectTaskNames())
	        {
		        StopTaskInTheList(selectTaskName);
	        }
        }

        private void deleteMenuItem_Click(object sender, EventArgs e)
        {
			var selectTaskNames = GetSelectTaskNames();
			var message = selectTaskNames.Count > 1? string.Format("确定要删除这些任务吗？") : 
				string.Format("确定要删除 {0} 吗？", selectTaskNames[0]);
			if (MessageBox.Show(message, "确认", MessageBoxButtons.YesNo) == DialogResult.No)
				return;

	        foreach (var selectTaskName in selectTaskNames)
	        {
				DeleteTaskInTheList(selectTaskName);
	        }
            
        }

        private void continueAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvDownloadingNovels.Items)
            {
                RunTask(item.SubItems[1].Text, false);
            }
        }

        private void stopAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvDownloadingNovels.Items)
            {
                StopTaskInTheList(item.SubItems[1].Text);
            }
        }

		private void btDownloadLast_Click(object sender, EventArgs e)
		{
			DownloadOnePara(tbUrl.Text + tbParaLastDownloaded.Text, cbDownloadedSource.Checked);
		}

		private void restartDownloadMenuItem_Click(object sender, EventArgs e)
		{
			var taskName = GetSelectTaskNames()[0];
			var downloadedFile = Path.Combine(tbDir.Text, taskName+".txt");
			var message = string.Format("确定要重新下载 {0} 吗？\r\n如果重新下载，已下载的小说文件 {1} 将会被删除！", taskName, downloadedFile);

			if (cbIsPicture.Checked)
			{
				downloadedFile = string.Empty;
				message =  string.Format("确定要重新下载 {0} 吗？\r\n如果重新下载，已下载的图像将会被覆盖！", taskName);
			}

			if (MessageBox.Show(message, "确认", MessageBoxButtons.YesNo) == DialogResult.No)
				return;

			if (!cbIsPicture.Checked)
				File.Delete(downloadedFile);

			RunTask(taskName, true);
		}

		private void tbName_TextChanged(object sender, EventArgs e)
		{
			btDown.Text = (FindTaskInMemory(tbName.Text) != null) ? "更改" : "增加";
		}

		private void tbStartPara_TextChanged(object sender, EventArgs e)
		{
			var textList = tbStartPara.Text.Split('/');
			if (textList.Length > 1)
			{
				tbEndPara.Text = "/" + textList[1] + "/";

				var downloadedTextList = tbParaLastDownloaded.Text.Split('/');
				if (downloadedTextList.Length == 4)
				{
					tbParaLastDownloaded.Text = "/" + textList[1] + "/" + downloadedTextList[2] + "/";
				}
			}
		}

		private void WatchMenuItem_Click(object sender, EventArgs e)
		{
			_showWatchNovelForm.Show();
			_showWatchNovelForm.BringToFront();
			foreach (var taskName in GetSelectTaskNames())
			{
				var taskIndex = FindTaskItemItemInTheList(taskName);
				if (taskIndex == -1)
					continue;

				if (lvDownloadingNovels.Items[taskIndex].SubItems[0].Text == "下载中")
					continue;

				var task = FindTaskInMemory(taskName);
				if (task.IsBookCompleted || !task.IsBookWatched)
					continue;

				RunTask(taskName, false);
			}
		}

		private void showFinishedBookMenuItem_Click(object sender, EventArgs e)
		{
			_showCompletedBooks = !_showCompletedBooks;

			showFinishedBookMenuItem.Text = _showCompletedBooks ? "隐藏已完本小说" : "显示已完本小说";

			if (_showCompletedBooks)
			{
				foreach (var downloadTask in _downloadTasks.Where(x => x.IsBookCompleted))
				{
					var lvi = FillListViewItemWithTask(downloadTask);
					lvDownloadingNovels.Items.Add(lvi);
				}
			}
			else
			{
				foreach (var downloadTask in _downloadTasks.Where(x => x.IsBookCompleted))
				{
					var item = FindTaskItemItemInTheList(downloadTask.TaskName);
					if (item == -1)
						continue;
					
					lvDownloadingNovels.Items.RemoveAt(item);
				}
			}
		}

		private void watchAllMenuItem_Click(object sender, EventArgs e)
		{
			_showWatchNovelForm.Show();
			_showWatchNovelForm.BringToFront();
			foreach (var downloadTask in _downloadTasks.Where(x => x.IsBookWatched && !x.IsBookCompleted))
			{
				var task = FindTaskItemItemInTheList(downloadTask.TaskName);
				if (task == -1)
					continue;

				if (lvDownloadingNovels.Items[task].SubItems[0].Text == "下载中")
					continue;
					
				RunTask(downloadTask.TaskName, false);
			}
		}
	}
}
