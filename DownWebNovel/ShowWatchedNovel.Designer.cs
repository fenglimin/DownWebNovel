namespace DownWebNovel
{
	partial class ShowWatchedNovel
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
			this.rtbPara = new System.Windows.Forms.RichTextBox();
			this.tvWatchedNovel = new System.Windows.Forms.TreeView();
			this.btNextPara = new System.Windows.Forms.Button();
			this.btPrevPara = new System.Windows.Forms.Button();
			this.btCurPara = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// rtbPara
			// 
			this.rtbPara.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbPara.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.rtbPara.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtbPara.ForeColor = System.Drawing.Color.OliveDrab;
			this.rtbPara.Location = new System.Drawing.Point(261, 3);
			this.rtbPara.Name = "rtbPara";
			this.rtbPara.ReadOnly = true;
			this.rtbPara.Size = new System.Drawing.Size(820, 665);
			this.rtbPara.TabIndex = 0;
			this.rtbPara.Text = "";
			this.rtbPara.ZoomFactor = 1.5F;
			this.rtbPara.VScroll += new System.EventHandler(this.rtbPara_VScroll);
			this.rtbPara.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtbPara_KeyDown);
			// 
			// tvWatchedNovel
			// 
			this.tvWatchedNovel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.tvWatchedNovel.Location = new System.Drawing.Point(12, 3);
			this.tvWatchedNovel.Name = "tvWatchedNovel";
			this.tvWatchedNovel.Size = new System.Drawing.Size(233, 698);
			this.tvWatchedNovel.TabIndex = 1;
			this.tvWatchedNovel.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvWatchedNovel_AfterSelect);
			// 
			// btNextPara
			// 
			this.btNextPara.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btNextPara.Location = new System.Drawing.Point(928, 674);
			this.btNextPara.Name = "btNextPara";
			this.btNextPara.Size = new System.Drawing.Size(153, 27);
			this.btNextPara.TabIndex = 2;
			this.btNextPara.Text = "下一章";
			this.btNextPara.UseVisualStyleBackColor = true;
			this.btNextPara.Click += new System.EventHandler(this.btNextPara_Click);
			// 
			// btPrevPara
			// 
			this.btPrevPara.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btPrevPara.Location = new System.Drawing.Point(261, 674);
			this.btPrevPara.Name = "btPrevPara";
			this.btPrevPara.Size = new System.Drawing.Size(147, 27);
			this.btPrevPara.TabIndex = 3;
			this.btPrevPara.Text = "上一章";
			this.btPrevPara.UseVisualStyleBackColor = true;
			this.btPrevPara.Click += new System.EventHandler(this.btPrevPara_Click);
			// 
			// btCurPara
			// 
			this.btCurPara.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btCurPara.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btCurPara.Location = new System.Drawing.Point(414, 674);
			this.btCurPara.Name = "btCurPara";
			this.btCurPara.Size = new System.Drawing.Size(508, 27);
			this.btCurPara.TabIndex = 4;
			this.btCurPara.UseVisualStyleBackColor = true;
			// 
			// ShowWatchedNovel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1093, 713);
			this.Controls.Add(this.btCurPara);
			this.Controls.Add(this.btPrevPara);
			this.Controls.Add(this.btNextPara);
			this.Controls.Add(this.tvWatchedNovel);
			this.Controls.Add(this.rtbPara);
			this.Name = "ShowWatchedNovel";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShowWatchedNovel";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShowWatchedNovel_FormClosing);
			this.Load += new System.EventHandler(this.ShowWatchedNovel_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox rtbPara;
		private System.Windows.Forms.TreeView tvWatchedNovel;
		private System.Windows.Forms.Button btNextPara;
		private System.Windows.Forms.Button btPrevPara;
		private System.Windows.Forms.Button btCurPara;
	}
}