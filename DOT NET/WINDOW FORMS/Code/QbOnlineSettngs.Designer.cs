namespace DocFinance_WindowApplication.Forms.QbOnline
{
    partial class QuickBookOnlineSettngs
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
            Panel panel1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickBookOnlineSettngs));
            textBox6 = new TextBox();
            label7 = new Label();
            textBox5 = new TextBox();
            label6 = new Label();
            textBox4 = new TextBox();
            label5 = new Label();
            textBox3 = new TextBox();
            button4 = new Button();
            label4 = new Label();
            testConnectionBtn = new Button();
            label3 = new Label();
            textBox2 = new TextBox();
            button3 = new Button();
            label2 = new Label();
            textBox1 = new TextBox();
            button2 = new Button();
            label1 = new Label();
            button1 = new Button();
            panel2 = new Panel();
            checkBox1 = new CheckBox();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.None;
            panel1.AutoSize = true;
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(checkBox1);
            panel1.Controls.Add(textBox6);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(textBox5);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(textBox4);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(textBox3);
            panel1.Controls.Add(button4);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(testConnectionBtn);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(textBox2);
            panel1.Controls.Add(button3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(panel2);
            panel1.Cursor = Cursors.Hand;
            panel1.Location = new Point(-1, -2);
            panel1.Name = "panel1";
            panel1.Size = new Size(754, 531);
            panel1.TabIndex = 1;
            panel1.Paint += panel1_Paint;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(182, 368);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(486, 23);
            textBox6.TabIndex = 21;
            textBox6.TextChanged += Scope;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = SystemColors.ControlLight;
            label7.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.Location = new Point(29, 373);
            label7.Name = "label7";
            label7.Size = new Size(59, 18);
            label7.TabIndex = 19;
            label7.Text = "Scope :";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(182, 313);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(486, 23);
            textBox5.TabIndex = 18;
            textBox5.TextChanged += ClientSecret;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = SystemColors.ControlLight;
            label6.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.Location = new Point(29, 318);
            label6.Name = "label6";
            label6.Size = new Size(100, 18);
            label6.TabIndex = 16;
            label6.Text = "Client Secret :";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(182, 259);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(486, 23);
            textBox4.TabIndex = 15;
            textBox4.TextChanged += ClientId;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = SystemColors.ControlLight;
            label5.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.Location = new Point(29, 263);
            label5.Name = "label5";
            label5.Size = new Size(71, 18);
            label5.TabIndex = 13;
            label5.Text = "Client ID :";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(182, 189);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(486, 23);
            textBox3.TabIndex = 12;
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // button4
            // 
            button4.Location = new Point(674, 189);
            button4.Name = "button4";
            button4.Size = new Size(30, 23);
            button4.TabIndex = 11;
            button4.Text = "...";
            button4.UseVisualStyleBackColor = true;
            button4.Click += TokenFileSelection;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(29, 194);
            label4.Name = "label4";
            label4.Size = new Size(112, 18);
            label4.TabIndex = 10;
            label4.Text = "Token file path :";
            // 
            // testConnectionBtn
            // 
            testConnectionBtn.BackColor = Color.FromArgb(30, 150, 80);
            testConnectionBtn.FlatStyle = FlatStyle.Flat;
            testConnectionBtn.Font = new Font("Segoe UI", 9.75F);
            testConnectionBtn.ForeColor = SystemColors.ButtonHighlight;
            testConnectionBtn.Location = new Point(94, 476);
            testConnectionBtn.Name = "testConnectionBtn";
            testConnectionBtn.Size = new Size(224, 34);
            testConnectionBtn.TabIndex = 8;
            testConnectionBtn.Text = "Get Token File";
            testConnectionBtn.UseVisualStyleBackColor = false;
            testConnectionBtn.Click += testConnectionBtn_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 11.25F);
            label3.Location = new Point(28, 133);
            label3.Name = "label3";
            label3.Size = new Size(130, 18);
            label3.TabIndex = 7;
            label3.Text = "Incoming file path :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(183, 128);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(486, 23);
            textBox2.TabIndex = 6;
            // 
            // button3
            // 
            button3.Location = new Point(675, 128);
            button3.Name = "button3";
            button3.Size = new Size(30, 23);
            button3.TabIndex = 5;
            button3.Text = "...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += IncomingFilePath;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 11.25F);
            label2.Location = new Point(27, 73);
            label2.Name = "label2";
            label2.Size = new Size(155, 18);
            label2.TabIndex = 4;
            label2.Text = "DocFinance File path :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(184, 70);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(486, 23);
            textBox1.TabIndex = 3;
            // 
            // button2
            // 
            button2.Location = new Point(676, 70);
            button2.Name = "button2";
            button2.Size = new Size(30, 23);
            button2.TabIndex = 2;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += DocFinanceFilePath;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold);
            label1.Location = new Point(258, 14);
            label1.Name = "label1";
            label1.Size = new Size(187, 24);
            label1.TabIndex = 1;
            label1.Text = "QB Online Settings";
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(30, 150, 80);
            button1.Cursor = Cursors.Hand;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Microsoft Sans Serif", 11.25F);
            button1.ForeColor = SystemColors.ControlLightLight;
            button1.Location = new Point(390, 476);
            button1.Name = "button1";
            button1.Size = new Size(224, 34);
            button1.TabIndex = 0;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLight;
            panel2.Location = new Point(19, 238);
            panel2.Name = "panel2";
            panel2.Size = new Size(708, 178);
            panel2.TabIndex = 22;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkBox1.Location = new Point(182, 435);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(159, 24);
            checkBox1.TabIndex = 23;
            checkBox1.Text = "Enable Automation";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // QuickBookOnlineSettngs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoValidate = AutoValidate.EnableAllowFocusChange;
            ClientSize = new Size(752, 528);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "QuickBookOnlineSettngs";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "QbOnlineSettngs";
            Load += QbOnlineSettngs_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Label label3;
        private TextBox textBox2;
        private Button button3;
        private Label label2;
        private TextBox textBox1;
        private Button button2;
        private Label label1;
        private Button button1;
        private Button testConnectionBtn;
        private Label label4;
        private TextBox textBox3;
        private Button button4;
        private TextBox textBox5;
        private Label label6;
        private TextBox textBox4;
        private Label label5;
        private TextBox textBox6;
        private Label label7;
        private Panel panel2;
        private CheckBox checkBox1;
    }
}