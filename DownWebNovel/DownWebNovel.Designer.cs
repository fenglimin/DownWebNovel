namespace DownWebNovel
{
	partial class DownWebNovel
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cbWebSite = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btDownloadFirstPara = new System.Windows.Forms.Button();
			this.btDeleteReplacement = new System.Windows.Forms.Button();
			this.tbReplaceTo = new System.Windows.Forms.TextBox();
			this.btAddOrUpdateReplacement = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.tbReplaceFrom = new System.Windows.Forms.TextBox();
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lvReplace = new System.Windows.Forms.ListView();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.btDeleteTag = new System.Windows.Forms.Button();
			this.btAddOrUpdateTag = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.lbTagValue = new System.Windows.Forms.ListBox();
			this.lbTagDefine = new System.Windows.Forms.ListBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbEndPara = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbUrl = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbStartPara = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tbName = new System.Windows.Forms.TextBox();
			this.btDown = new System.Windows.Forms.Button();
			this.tbMessage = new System.Windows.Forms.TextBox();
			this.lvDownloadingNovels = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupBox4.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cbWebSite
			// 
			this.cbWebSite.FormattingEnabled = true;
			this.cbWebSite.Location = new System.Drawing.Point(43, 22);
			this.cbWebSite.Name = "cbWebSite";
			this.cbWebSite.Size = new System.Drawing.Size(191, 21);
			this.cbWebSite.TabIndex = 13;
			this.cbWebSite.SelectedIndexChanged += new System.EventHandler(this.cbWebSite_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 26);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(31, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "网站";
			// 
			// btDownloadFirstPara
			// 
			this.btDownloadFirstPara.Location = new System.Drawing.Point(9, 160);
			this.btDownloadFirstPara.Name = "btDownloadFirstPara";
			this.btDownloadFirstPara.Size = new System.Drawing.Size(225, 23);
			this.btDownloadFirstPara.TabIndex = 10;
			this.btDownloadFirstPara.Text = "下载首章网页文本内容";
			this.btDownloadFirstPara.UseVisualStyleBackColor = true;
			this.btDownloadFirstPara.Click += new System.EventHandler(this.btDownloadFirstPara_Click);
			// 
			// btDeleteReplacement
			// 
			this.btDeleteReplacement.Location = new System.Drawing.Point(145, 160);
			this.btDeleteReplacement.Name = "btDeleteReplacement";
			this.btDeleteReplacement.Size = new System.Drawing.Size(93, 23);
			this.btDeleteReplacement.TabIndex = 6;
			this.btDeleteReplacement.Text = "删除";
			this.btDeleteReplacement.UseVisualStyleBackColor = true;
			// 
			// tbReplaceTo
			// 
			this.tbReplaceTo.Location = new System.Drawing.Point(145, 131);
			this.tbReplaceTo.Name = "tbReplaceTo";
			this.tbReplaceTo.Size = new System.Drawing.Size(93, 20);
			this.tbReplaceTo.TabIndex = 9;
			// 
			// btAddOrUpdateReplacement
			// 
			this.btAddOrUpdateReplacement.Location = new System.Drawing.Point(11, 160);
			this.btAddOrUpdateReplacement.Name = "btAddOrUpdateReplacement";
			this.btAddOrUpdateReplacement.Size = new System.Drawing.Size(93, 23);
			this.btAddOrUpdateReplacement.TabIndex = 5;
			this.btAddOrUpdateReplacement.Text = "增加/更改";
			this.btAddOrUpdateReplacement.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(119, 133);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(19, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "=>";
			// 
			// tbReplaceFrom
			// 
			this.tbReplaceFrom.Location = new System.Drawing.Point(11, 131);
			this.tbReplaceFrom.Name = "tbReplaceFrom";
			this.tbReplaceFrom.Size = new System.Drawing.Size(93, 20);
			this.tbReplaceFrom.TabIndex = 5;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "替换为";
			this.columnHeader2.Width = 100;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "查找内容";
			this.columnHeader1.Width = 100;
			// 
			// lvReplace
			// 
			this.lvReplace.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.lvReplace.FullRowSelect = true;
			this.lvReplace.GridLines = true;
			this.lvReplace.Location = new System.Drawing.Point(11, 23);
			this.lvReplace.Name = "lvReplace";
			this.lvReplace.Size = new System.Drawing.Size(227, 95);
			this.lvReplace.TabIndex = 0;
			this.lvReplace.UseCompatibleStateImageBehavior = false;
			this.lvReplace.View = System.Windows.Forms.View.Details;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.btDeleteReplacement);
			this.groupBox4.Controls.Add(this.tbReplaceTo);
			this.groupBox4.Controls.Add(this.btAddOrUpdateReplacement);
			this.groupBox4.Controls.Add(this.label5);
			this.groupBox4.Controls.Add(this.tbReplaceFrom);
			this.groupBox4.Controls.Add(this.lvReplace);
			this.groupBox4.Location = new System.Drawing.Point(562, 12);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(248, 192);
			this.groupBox4.TabIndex = 16;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "替换";
			// 
			// btDeleteTag
			// 
			this.btDeleteTag.Location = new System.Drawing.Point(211, 160);
			this.btDeleteTag.Name = "btDeleteTag";
			this.btDeleteTag.Size = new System.Drawing.Size(49, 23);
			this.btDeleteTag.TabIndex = 4;
			this.btDeleteTag.Text = "删除";
			this.btDeleteTag.UseVisualStyleBackColor = true;
			// 
			// btAddOrUpdateTag
			// 
			this.btAddOrUpdateTag.Location = new System.Drawing.Point(152, 160);
			this.btAddOrUpdateTag.Name = "btAddOrUpdateTag";
			this.btAddOrUpdateTag.Size = new System.Drawing.Size(49, 23);
			this.btAddOrUpdateTag.TabIndex = 3;
			this.btAddOrUpdateTag.Text = "增加/更改";
			this.btAddOrUpdateTag.UseVisualStyleBackColor = true;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(152, 131);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(108, 20);
			this.textBox1.TabIndex = 2;
			// 
			// lbTagValue
			// 
			this.lbTagValue.FormattingEnabled = true;
			this.lbTagValue.Location = new System.Drawing.Point(152, 23);
			this.lbTagValue.Name = "lbTagValue";
			this.lbTagValue.Size = new System.Drawing.Size(108, 95);
			this.lbTagValue.TabIndex = 1;
			// 
			// lbTagDefine
			// 
			this.lbTagDefine.FormattingEnabled = true;
			this.lbTagDefine.Items.AddRange(new object[] {
            "标题开始",
            "标题结束",
            "正文开始",
            "正文结束",
            "下章地址开始",
            "下章地址结束"});
			this.lbTagDefine.Location = new System.Drawing.Point(11, 23);
			this.lbTagDefine.Name = "lbTagDefine";
			this.lbTagDefine.Size = new System.Drawing.Size(128, 160);
			this.lbTagDefine.TabIndex = 0;
			this.lbTagDefine.SelectedIndexChanged += new System.EventHandler(this.lbTagDefine_SelectedIndexChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.btDeleteTag);
			this.groupBox2.Controls.Add(this.btAddOrUpdateTag);
			this.groupBox2.Controls.Add(this.textBox1);
			this.groupBox2.Controls.Add(this.lbTagValue);
			this.groupBox2.Controls.Add(this.lbTagDefine);
			this.groupBox2.Location = new System.Drawing.Point(276, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(274, 192);
			this.groupBox2.TabIndex = 15;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "标签";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 134);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(31, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "终章";
			// 
			// tbEndPara
			// 
			this.tbEndPara.Location = new System.Drawing.Point(43, 131);
			this.tbEndPara.Name = "tbEndPara";
			this.tbEndPara.Size = new System.Drawing.Size(191, 20);
			this.tbEndPara.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "首章";
			// 
			// tbUrl
			// 
			this.tbUrl.Location = new System.Drawing.Point(43, 77);
			this.tbUrl.Name = "tbUrl";
			this.tbUrl.Size = new System.Drawing.Size(191, 20);
			this.tbUrl.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 82);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(31, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "网址";
			// 
			// tbStartPara
			// 
			this.tbStartPara.Location = new System.Drawing.Point(43, 104);
			this.tbStartPara.Name = "tbStartPara";
			this.tbStartPara.Size = new System.Drawing.Size(191, 20);
			this.tbStartPara.TabIndex = 3;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbWebSite);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.btDownloadFirstPara);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.tbEndPara);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.tbUrl);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.tbStartPara);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.tbName);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(245, 192);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "小说";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 54);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(31, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "名称";
			// 
			// tbName
			// 
			this.tbName.Location = new System.Drawing.Point(43, 50);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(191, 20);
			this.tbName.TabIndex = 1;
			// 
			// btDown
			// 
			this.btDown.Location = new System.Drawing.Point(12, 219);
			this.btDown.Name = "btDown";
			this.btDown.Size = new System.Drawing.Size(798, 27);
			this.btDown.TabIndex = 17;
			this.btDown.Text = "开始下载";
			this.btDown.UseVisualStyleBackColor = true;
			this.btDown.Click += new System.EventHandler(this.btDown_Click);
			// 
			// tbMessage
			// 
			this.tbMessage.Location = new System.Drawing.Point(276, 262);
			this.tbMessage.Multiline = true;
			this.tbMessage.Name = "tbMessage";
			this.tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbMessage.Size = new System.Drawing.Size(534, 297);
			this.tbMessage.TabIndex = 18;
			// 
			// lvDownloadingNovels
			// 
			this.lvDownloadingNovels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
			this.lvDownloadingNovels.FullRowSelect = true;
			this.lvDownloadingNovels.GridLines = true;
			this.lvDownloadingNovels.Location = new System.Drawing.Point(12, 262);
			this.lvDownloadingNovels.Name = "lvDownloadingNovels";
			this.lvDownloadingNovels.Size = new System.Drawing.Size(245, 297);
			this.lvDownloadingNovels.TabIndex = 10;
			this.lvDownloadingNovels.UseCompatibleStateImageBehavior = false;
			this.lvDownloadingNovels.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "小说";
			this.columnHeader3.Width = 90;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "当前下载章节";
			this.columnHeader4.Width = 150;
			// 
			// DownWebNovel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(822, 571);
			this.Controls.Add(this.lvDownloadingNovels);
			this.Controls.Add(this.tbMessage);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btDown);
			this.Name = "DownWebNovel";
			this.Text = "下载网络小说";
			this.Load += new System.EventHandler(this.DownWebNovel_Load);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cbWebSite;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btDownloadFirstPara;
		private System.Windows.Forms.Button btDeleteReplacement;
		private System.Windows.Forms.TextBox tbReplaceTo;
		private System.Windows.Forms.Button btAddOrUpdateReplacement;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbReplaceFrom;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ListView lvReplace;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button btDeleteTag;
		private System.Windows.Forms.Button btAddOrUpdateTag;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ListBox lbTagValue;
		private System.Windows.Forms.ListBox lbTagDefine;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbEndPara;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbUrl;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbStartPara;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbName;
		private System.Windows.Forms.Button btDown;
		private System.Windows.Forms.TextBox tbMessage;
		private System.Windows.Forms.ListView lvDownloadingNovels;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;

	}
}

