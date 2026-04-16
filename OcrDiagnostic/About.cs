using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace AtalaDemos.AboutBox
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel downloadHelpLinkLabel;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.LinkLabel demoGalleryLinkLabel;
		private System.Windows.Forms.LinkLabel downloadDotImageLinkLabel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtAssemblies;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public About(string windowTitle, string progName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.Text = windowTitle;
			this.label5.Text = progName;
			// Load assembly versions.
			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			System.Reflection.AssemblyName[] refs = asm.GetReferencedAssemblies();
			System.Text.StringBuilder msg = new System.Text.StringBuilder();

			foreach (System.Reflection.AssemblyName name in refs)
			{
				if (name.Name.StartsWith("Atalasoft"))
				{
					if (msg.Length != 0) msg.Append("\r\n");
					msg.Append(name.Name + " - " + name.Version.ToString());
				}
			}

			this.txtAssemblies.Text = msg.ToString();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.downloadHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.demoGalleryLinkLabel = new System.Windows.Forms.LinkLabel();
            this.downloadDotImageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAssemblies = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(19, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(448, 144);
            this.label1.TabIndex = 1;
            this.label1.Text = "This demo showcases most of the basic functions that can be performed with DotIma" +
    "ge.  This is a good starting place for learning about the SDK.";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(75, 200);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(336, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please Check the following for help programming with DotImage:";
            // 
            // downloadHelpLinkLabel
            // 
            this.downloadHelpLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 36);
            this.downloadHelpLinkLabel.Location = new System.Drawing.Point(128, 256);
            this.downloadHelpLinkLabel.Name = "downloadHelpLinkLabel";
            this.downloadHelpLinkLabel.Size = new System.Drawing.Size(200, 16);
            this.downloadHelpLinkLabel.TabIndex = 4;
            this.downloadHelpLinkLabel.TabStop = true;
            this.downloadHelpLinkLabel.Text = "DotImage Documentation";
            this.downloadHelpLinkLabel.UseCompatibleTextRendering = true;
            this.downloadHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.apiReferenceLinkLabel_LinkClicked);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Location = new System.Drawing.Point(348, 550);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 24);
            this.button1.TabIndex = 5;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // demoGalleryLinkLabel
            // 
            this.demoGalleryLinkLabel.Location = new System.Drawing.Point(128, 280);
            this.demoGalleryLinkLabel.Name = "demoGalleryLinkLabel";
            this.demoGalleryLinkLabel.Size = new System.Drawing.Size(192, 16);
            this.demoGalleryLinkLabel.TabIndex = 6;
            this.demoGalleryLinkLabel.TabStop = true;
            this.demoGalleryLinkLabel.Text = "OCR Diagnostic Demo Home";
            this.demoGalleryLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.demoGalleryLinkLabel_LinkClicked);
            // 
            // downloadDotImageLinkLabel
            // 
            this.downloadDotImageLinkLabel.Location = new System.Drawing.Point(128, 233);
            this.downloadDotImageLinkLabel.Name = "downloadDotImageLinkLabel";
            this.downloadDotImageLinkLabel.Size = new System.Drawing.Size(184, 23);
            this.downloadDotImageLinkLabel.TabIndex = 7;
            this.downloadDotImageLinkLabel.TabStop = true;
            this.downloadDotImageLinkLabel.Text = "Download the Latest DotImage";
            this.downloadDotImageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadLinkLabel_LinkClicked);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(128, 336);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(264, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sales / Support:  +1 781.743.2119";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(48, 492);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(272, 88);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.OnMouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.DarkOrange;
            this.label5.Location = new System.Drawing.Point(39, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(408, 32);
            this.label5.TabIndex = 10;
            this.label5.Text = "[demo title here]";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAssemblies);
            this.groupBox1.Location = new System.Drawing.Point(91, 364);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 117);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Assemblies";
            // 
            // txtAssemblies
            // 
            this.txtAssemblies.BackColor = System.Drawing.Color.White;
            this.txtAssemblies.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAssemblies.Location = new System.Drawing.Point(9, 16);
            this.txtAssemblies.Multiline = true;
            this.txtAssemblies.Name = "txtAssemblies";
            this.txtAssemblies.Size = new System.Drawing.Size(275, 74);
            this.txtAssemblies.TabIndex = 0;
            // 
            // About
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(486, 588);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.downloadDotImageLinkLabel);
            this.Controls.Add(this.demoGalleryLinkLabel);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.downloadHelpLinkLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "[about title here]";
            this.Load += new System.EventHandler(this.About_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Dispose();
		}

		private void apiReferenceLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
            System.Diagnostics.Process.Start("https://www.atalasoft.com/Support/APIs-Dev-Guides");
        }

		private void demoGalleryLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
            System.Diagnostics.Process.Start("https://www.atalasoft.com/KB2/KB/50086/");
        }

		private void downloadLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
            System.Diagnostics.Process.Start("https://www.atalasoft.com/BeginDownload");
        }

		private void pictureBox1_Click(object sender, System.EventArgs e)
		{
            System.Diagnostics.Process.Start("https://www.atalasoft.com");
        }

		private void OnMouseEnter(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.Hand;
		}

		private void OnMouseLeave(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.Default;
		}

		private string _description = "";

		private void About_Load(object sender, System.EventArgs e)
		{
			this.label1.Text = _description;
		}
	
		public string Description
		{
			get { return _description; }
			set { 
				_description = value;
			}
		}


	}
}
