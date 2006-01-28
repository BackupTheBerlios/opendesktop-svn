/*
 * Created by SharpDevelop.
 * User: pravin
 * Date: 1/22/2006
 * Time: 12:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDesktop
{
	/// <summary>
	/// Description of IndexGeneration.
	/// </summary>
	public class IndexGeneration : System.Windows.Forms.Form
	{
		public IndexGeneration()
		{
			InitializeComponent();
			// The following helps other threads call the ShowProgress
			// function and change the label text
			Control.CheckForIllegalCrossThreadCalls = false;
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IndexGeneration));
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.lblMessage = new System.Windows.Forms.Label();
			this.lblFilePath = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(12, 97);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(268, 20);
			this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar.TabIndex = 0;
			// 
			// lblMessage
			// 
			this.lblMessage.Location = new System.Drawing.Point(12, 9);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(268, 41);
			this.lblMessage.TabIndex = 1;
			this.lblMessage.Text = "Generating Index for the first time. This  process may take a few minutes";
			// 
			// lblFilePath
			// 
			this.lblFilePath.Location = new System.Drawing.Point(12, 50);
			this.lblFilePath.Name = "lblFilePath";
			this.lblFilePath.Size = new System.Drawing.Size(268, 44);
			this.lblFilePath.TabIndex = 2;
			// 
			// IndexGeneration
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 129);
			this.Controls.Add(this.lblFilePath);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "IndexGeneration";
			this.Text = "Generating Index ";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.Label lblFilePath;
		private System.Windows.Forms.ProgressBar progressBar;
		#endregion
		
		public void ShowProgress(string currentFile)
		{
			this.lblFilePath.Text = currentFile;
		}
		
		public new void Dispose()
		{
			this.Hide();
			this.Close();
			base.Dispose();
		}
	}
}
