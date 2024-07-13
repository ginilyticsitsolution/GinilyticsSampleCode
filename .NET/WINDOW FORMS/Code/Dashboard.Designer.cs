using Microsoft.Extensions.Configuration;

namespace DocFinance_WindowApplication
{
    partial class Dashboard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            menuStrip1 = new MenuStrip();
            homeToolStripMenuItem = new ToolStripMenuItem();
            quickBooksOnlineToolStripMenuItem = new ToolStripMenuItem();
            quickBooksDesktopToolStripMenuItem = new ToolStripMenuItem();
            xeroOnlineToolStripMenuItem = new ToolStripMenuItem();
            kToolStripMenuItem = new ToolStripMenuItem();
            versionToolStripMenuItem = new ToolStripMenuItem();
            documentToolStripMenuItem = new ToolStripMenuItem();
            logsToolStripMenuItem = new ToolStripMenuItem();
            logOutToolStripMenuItem = new ToolStripMenuItem();
            Group = new GroupBox();
            pictureBox4 = new PictureBox();
            button1 = new Button();
            button3 = new Button();
            button2 = new Button();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            pictureBox3 = new PictureBox();
            menuStrip1.SuspendLayout();
            Group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { homeToolStripMenuItem, kToolStripMenuItem, logOutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1041, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.ItemClicked += menuStrip1_ItemClicked;
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { quickBooksOnlineToolStripMenuItem, quickBooksDesktopToolStripMenuItem, xeroOnlineToolStripMenuItem });
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.Size = new Size(52, 20);
            homeToolStripMenuItem.Text = "Home";
            // 
            // quickBooksOnlineToolStripMenuItem
            // 
            quickBooksOnlineToolStripMenuItem.Name = "quickBooksOnlineToolStripMenuItem";
            quickBooksOnlineToolStripMenuItem.Size = new Size(183, 22);
            quickBooksOnlineToolStripMenuItem.Text = "QuickBooks Online";
            quickBooksOnlineToolStripMenuItem.Click += QuickBooks_Online;
            // 
            // quickBooksDesktopToolStripMenuItem
            // 
            quickBooksDesktopToolStripMenuItem.Name = "quickBooksDesktopToolStripMenuItem";
            quickBooksDesktopToolStripMenuItem.Size = new Size(183, 22);
            quickBooksDesktopToolStripMenuItem.Text = "QuickBooks Desktop";
            quickBooksDesktopToolStripMenuItem.Click += QuickBooks_Desktop;
            // 
            // xeroOnlineToolStripMenuItem
            // 
            xeroOnlineToolStripMenuItem.Name = "xeroOnlineToolStripMenuItem";
            xeroOnlineToolStripMenuItem.Size = new Size(183, 22);
            xeroOnlineToolStripMenuItem.Text = "Xero Online";
            xeroOnlineToolStripMenuItem.Click += Xero_Online;
            // 
            // kToolStripMenuItem
            // 
            kToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { versionToolStripMenuItem, documentToolStripMenuItem, logsToolStripMenuItem });
            kToolStripMenuItem.Name = "kToolStripMenuItem";
            kToolStripMenuItem.Size = new Size(44, 20);
            kToolStripMenuItem.Text = "Help";
            // 
            // versionToolStripMenuItem
            // 
            versionToolStripMenuItem.Name = "versionToolStripMenuItem";
            versionToolStripMenuItem.Size = new Size(180, 22);
            versionToolStripMenuItem.Text = "Version";
            // 
            // documentToolStripMenuItem
            // 
            documentToolStripMenuItem.Name = "documentToolStripMenuItem";
            documentToolStripMenuItem.Size = new Size(180, 22);
            documentToolStripMenuItem.Text = "Document";
            // 
            // logsToolStripMenuItem
            // 
            logsToolStripMenuItem.Name = "logsToolStripMenuItem";
            logsToolStripMenuItem.Size = new Size(180, 22);
            logsToolStripMenuItem.Text = "Logs";
            logsToolStripMenuItem.Click += Logs;
            // 
            // logOutToolStripMenuItem
            // 
            logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
            logOutToolStripMenuItem.Size = new Size(62, 20);
            logOutToolStripMenuItem.Text = "Log Out";
            logOutToolStripMenuItem.Click += logout;
            // 
            // Group
            // 
            Group.BackColor = Color.Snow;
            Group.Controls.Add(pictureBox4);
            Group.Controls.Add(button1);
            Group.Controls.Add(button3);
            Group.Controls.Add(button2);
            Group.Controls.Add(pictureBox2);
            Group.Controls.Add(pictureBox1);
            Group.Controls.Add(pictureBox3);
            Group.Font = new Font("Segoe UI", 12F, FontStyle.Bold | FontStyle.Italic);
            Group.ForeColor = Color.Black;
            Group.Location = new Point(1, 25);
            Group.Name = "Group";
            Group.Size = new Size(1038, 337);
            Group.TabIndex = 6;
            Group.TabStop = false;
            Group.Text = "Select an Option";
            Group.Enter += Group_Enter;
            // 
            // pictureBox4
            // 
            pictureBox4.BackgroundImage = (Image)resources.GetObject("pictureBox4.BackgroundImage");
            pictureBox4.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox4.Location = new Point(700, 51);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(306, 260);
            pictureBox4.TabIndex = 10;
            pictureBox4.TabStop = false;
            pictureBox4.Tag = "";
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(30, 150, 80);
            button1.Cursor = Cursors.Hand;
            button1.FlatStyle = FlatStyle.Popup;
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Image = (Image)resources.GetObject("button1.Image");
            button1.Location = new Point(973, 51);
            button1.Name = "button1";
            button1.Size = new Size(33, 32);
            button1.TabIndex = 8;
            button1.UseVisualStyleBackColor = false;
            button1.Click += QuickBookDesktopSettings;
            // 
            // button3
            // 
            button3.BackColor = Color.FromArgb(30, 150, 80);
            button3.Cursor = Cursors.Hand;
            button3.FlatStyle = FlatStyle.Popup;
            button3.ForeColor = SystemColors.ButtonHighlight;
            button3.Image = (Image)resources.GetObject("button3.Image");
            button3.Location = new Point(639, 51);
            button3.Name = "button3";
            button3.Size = new Size(33, 32);
            button3.TabIndex = 9;
            button3.UseVisualStyleBackColor = false;
            button3.Click += XeroOnlineSettings;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(30, 150, 80);
            button2.Cursor = Cursors.Hand;
            button2.FlatStyle = FlatStyle.Popup;
            button2.ForeColor = SystemColors.ButtonHighlight;
            button2.Image = (Image)resources.GetObject("button2.Image");
            button2.Location = new Point(299, 51);
            button2.Name = "button2";
            button2.Size = new Size(33, 32);
            button2.TabIndex = 7;
            button2.UseVisualStyleBackColor = false;
            button2.Click += QuickBookOnlineSettings;
            // 
            // pictureBox2
            // 
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(366, 51);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(306, 260);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 5;
            pictureBox2.TabStop = false;
            pictureBox2.Click += Xero_Online;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(26, 51);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(306, 260);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            pictureBox1.Click += QuickBooks_Online;
            // 
            // pictureBox3
            // 
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(700, 51);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(306, 260);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 6;
            pictureBox3.TabStop = false;
            pictureBox3.Click += QuickBooks_Desktop;
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 40);
            ClientSize = new Size(1041, 364);
            Controls.Add(Group);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            Name = "Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            Load += groupLoad;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            Group.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private GroupBox groupBox1;
        private ProgressBar progressBar1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem homeToolStripMenuItem;
        private ToolStripMenuItem kToolStripMenuItem;
        private ToolStripMenuItem quickBooksOnlineToolStripMenuItem;
        private ToolStripMenuItem quickBooksDesktopToolStripMenuItem;
        private ToolStripMenuItem xeroOnlineToolStripMenuItem;
        private ToolStripMenuItem versionToolStripMenuItem;
        private ToolStripMenuItem documentToolStripMenuItem;
        private ToolStripMenuItem logOutToolStripMenuItem;
        private GroupBox Group;
        private PictureBox pictureBox3;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private Button button2;
        private Button button3;
        private PictureBox pictureBox4;
        private ToolStripMenuItem logsToolStripMenuItem;
    }
}