using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownWebNovel
{
	public partial class ShowWatchedNovel : Form
	{
		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lParam);
		const int PFM_SPACEBEFORE = 0x00000040;
		const int PFM_SPACEAFTER = 0x00000080;
		const int PFM_LINESPACING = 0x00000100;
		const int SCF_SELECTION = 1;
		const int EM_SETPARAFORMAT = 1095;

		public ShowWatchedNovel()
		{
			InitializeComponent();
		}

		private void ShowWatchedNovel_Load(object sender, EventArgs e)
		{
			setLineFormat(1, 200);
		}

		public void AddPara(Task task)
		{
			var bookNodes = tvWatchedNovel.Nodes.Find(task.TaskName, true);
			var bookNode = bookNodes.Length == 1 ? bookNodes[0] : tvWatchedNovel.Nodes.Add(task.TaskName, task.TaskName);

			if (!string.IsNullOrEmpty(task.ContentLastDownloaded))
			{
				var node = bookNode.Nodes.Add(task.ContentLastDownloaded, task.ParaTitleLastDownloaded);
				if (tvWatchedNovel.SelectedNode == null)
					tvWatchedNovel.SelectedNode = node;
			}
		}

		private void setLineFormat(byte rule, int space)
		{
			PARAFORMAT fmt = new PARAFORMAT();
			fmt.cbSize = Marshal.SizeOf(fmt);
			fmt.dwMask = PFM_LINESPACING;
			fmt.dyLineSpacing = space;
			fmt.bLineSpacingRule = rule;
			rtbPara.SelectAll();
			SendMessage(new HandleRef(rtbPara, rtbPara.Handle),
						 EM_SETPARAFORMAT,
						 SCF_SELECTION,
						 ref fmt
					   );
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PARAFORMAT
		{
			public int cbSize;
			public uint dwMask;
			public short wNumbering;
			public short wReserved;
			public int dxStartIndent;
			public int dxRightIndent;
			public int dxOffset;
			public short wAlignment;
			public short cTabCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public int[] rgxTabs;
			// PARAFORMAT2 from here onwards
			public int dySpaceBefore;
			public int dySpaceAfter;
			public int dyLineSpacing;
			public short sStyle;
			public byte bLineSpacingRule;
			public byte bOutlineLevel;
			public short wShadingWeight;
			public short wShadingStyle;
			public short wNumberingStart;
			public short wNumberingStyle;
			public short wNumberingTab;
			public short wBorderSpace;
			public short wBorderWidth;
			public short wBorders;
		}

		private void tvWatchedNovel_AfterSelect(object sender, TreeViewEventArgs e)
		{
			rtbPara.Text = tvWatchedNovel.SelectedNode.Name;
			btCurPara.Text = (tvWatchedNovel.SelectedNode.Parent != null? tvWatchedNovel.SelectedNode.Parent.Text + " : " : string.Empty) + tvWatchedNovel.SelectedNode.Text;
			rtbPara.ScrollToCaret();
			rtbPara.Focus();
		}

		private void ShowWatchedNovel_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void btPrevPara_Click(object sender, EventArgs e)
		{
			if (tvWatchedNovel.SelectedNode.PrevNode != null)
				tvWatchedNovel.SelectedNode = tvWatchedNovel.SelectedNode.PrevNode;
		}

		private void btNextPara_Click(object sender, EventArgs e)
		{
			if (tvWatchedNovel.SelectedNode.NextNode != null)
				tvWatchedNovel.SelectedNode = tvWatchedNovel.SelectedNode.NextNode;
		}

		private void rtbPara_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Space)
			{
				e.Handled = true;
				SendKeys.Send("{PGDN}");
			}
		}

		private void rtbPara_VScroll(object sender, EventArgs e)
		{

		}
	}
}
