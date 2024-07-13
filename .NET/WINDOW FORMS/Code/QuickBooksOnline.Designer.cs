namespace DocFinance_WindowApplication
{
    partial class QuickBooksOnline
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickBooksOnline));
            panel1 = new Panel();
            label1 = new Label();
            panel3 = new Panel();
            OutgoingBox = new GroupBox();
            progressBar2 = new ProgressBar();
            fileLocation = new Label();
            label7 = new Label();
            outgoingButton = new Button();
            OutgoingBoxLabel = new Label();
            OutgoingComboBox = new ComboBox();
            UploadButton = new Button();
            attachedFile = new Label();
            fileName = new Label();
            FilenameText = new Label();
            IncomingBox = new GroupBox();
            progressBar1 = new ProgressBar();
            label4 = new Label();
            label3 = new Label();
            IncomingFilePath = new Label();
            label2 = new Label();
            IncomingBoxLabel = new Label();
            IncomingComboBox = new ComboBox();
            incomingButton = new Button();
            label6 = new Label();
            FilePreviewLabel = new Label();
            FilePreview = new RichTextBox();
            Manual = new Label();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            OutgoingBox.SuspendLayout();
            IncomingBox.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.AppWorkspace;
            panel1.Controls.Add(label1);
            panel1.Controls.Add(panel3);
            panel1.Location = new Point(-11, -4);
            panel1.Name = "panel1";
            panel1.Size = new Size(1405, 609);
            panel1.TabIndex = 6;
            panel1.Paint += panel1_Paint;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold);
            label1.Location = new Point(16, 5);
            label1.Name = "label1";
            label1.Size = new Size(189, 24);
            label1.TabIndex = 19;
            label1.Text = "QuickBooks Online";
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.Control;
            panel3.Controls.Add(OutgoingBox);
            panel3.Controls.Add(IncomingBox);
            panel3.Controls.Add(label6);
            panel3.Controls.Add(FilePreviewLabel);
            panel3.Controls.Add(FilePreview);
            panel3.Controls.Add(Manual);
            panel3.Location = new Point(20, 31);
            panel3.Name = "panel3";
            panel3.Size = new Size(1362, 552);
            panel3.TabIndex = 1;
            // 
            // OutgoingBox
            // 
            OutgoingBox.Controls.Add(progressBar2);
            OutgoingBox.Controls.Add(fileLocation);
            OutgoingBox.Controls.Add(label7);
            OutgoingBox.Controls.Add(outgoingButton);
            OutgoingBox.Controls.Add(OutgoingBoxLabel);
            OutgoingBox.Controls.Add(OutgoingComboBox);
            OutgoingBox.Controls.Add(UploadButton);
            OutgoingBox.Controls.Add(attachedFile);
            OutgoingBox.Controls.Add(fileName);
            OutgoingBox.Controls.Add(FilenameText);
            OutgoingBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            OutgoingBox.Location = new Point(18, 138);
            OutgoingBox.Margin = new Padding(3, 2, 3, 2);
            OutgoingBox.Name = "OutgoingBox";
            OutgoingBox.Padding = new Padding(3, 2, 3, 2);
            OutgoingBox.Size = new Size(1324, 126);
            OutgoingBox.TabIndex = 20;
            OutgoingBox.TabStop = false;
            OutgoingBox.Text = "Push Data";
            // 
            // progressBar2
            // 
            progressBar2.Location = new Point(1125, 75);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new Size(188, 23);
            progressBar2.TabIndex = 14;
            progressBar2.Visible = false;
            // 
            // fileLocation
            // 
            fileLocation.AutoSize = true;
            fileLocation.Font = new Font("Calibri", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fileLocation.ForeColor = Color.Green;
            fileLocation.Location = new Point(150, 99);
            fileLocation.Name = "fileLocation";
            fileLocation.Size = new Size(74, 18);
            fileLocation.TabIndex = 20;
            fileLocation.Text = "Attach file ";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 12F);
            label7.Location = new Point(16, 97);
            label7.Name = "label7";
            label7.Size = new Size(134, 20);
            label7.TabIndex = 19;
            label7.Text = "Default Location :";
            // 
            // outgoingButton
            // 
            outgoingButton.BackColor = Color.Silver;
            outgoingButton.Cursor = Cursors.Hand;
            outgoingButton.FlatStyle = FlatStyle.Flat;
            outgoingButton.Font = new Font("Microsoft Sans Serif", 14.25F);
            outgoingButton.ForeColor = SystemColors.Control;
            outgoingButton.Location = new Point(1125, 21);
            outgoingButton.Name = "outgoingButton";
            outgoingButton.Size = new Size(188, 48);
            outgoingButton.TabIndex = 12;
            outgoingButton.Text = "Run ";
            outgoingButton.UseVisualStyleBackColor = false;
            outgoingButton.Click += outgoingButton_Click;
            // 
            // OutgoingBoxLabel
            // 
            OutgoingBoxLabel.AutoSize = true;
            OutgoingBoxLabel.Font = new Font("Microsoft Sans Serif", 12F);
            OutgoingBoxLabel.Location = new Point(16, 56);
            OutgoingBoxLabel.Name = "OutgoingBoxLabel";
            OutgoingBoxLabel.Size = new Size(34, 20);
            OutgoingBoxLabel.TabIndex = 13;
            OutgoingBoxLabel.Text = "File";
            // 
            // OutgoingComboBox
            // 
            OutgoingComboBox.Cursor = Cursors.Hand;
            OutgoingComboBox.FormattingEnabled = true;
            OutgoingComboBox.Items.AddRange(new object[] { "Prime Entry File" });
            OutgoingComboBox.Location = new Point(122, 49);
            OutgoingComboBox.Name = "OutgoingComboBox";
            OutgoingComboBox.Size = new Size(197, 28);
            OutgoingComboBox.TabIndex = 12;
            OutgoingComboBox.SelectedIndexChanged += OutgoingComboBox_SelectedIndexChanged;
            // 
            // UploadButton
            // 
            UploadButton.BackColor = Color.Green;
            UploadButton.Cursor = Cursors.Hand;
            UploadButton.FlatStyle = FlatStyle.Flat;
            UploadButton.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            UploadButton.ForeColor = Color.White;
            UploadButton.Location = new Point(641, 50);
            UploadButton.Name = "UploadButton";
            UploadButton.Size = new Size(133, 33);
            UploadButton.TabIndex = 15;
            UploadButton.Text = "Upload Document";
            UploadButton.UseVisualStyleBackColor = false;
            // 
            // attachedFile
            // 
            attachedFile.AutoSize = true;
            attachedFile.Font = new Font("Microsoft Sans Serif", 12F);
            attachedFile.Location = new Point(642, 20);
            attachedFile.Name = "attachedFile";
            attachedFile.Size = new Size(89, 20);
            attachedFile.TabIndex = 14;
            attachedFile.Text = "Attach File ";
            // 
            // fileName
            // 
            fileName.AutoSize = true;
            fileName.Cursor = Cursors.IBeam;
            fileName.Font = new Font("Calibri", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fileName.ForeColor = Color.FromArgb(50, 150, 80);
            fileName.Location = new Point(741, 99);
            fileName.MaximumSize = new Size(270, 30);
            fileName.Name = "fileName";
            fileName.Size = new Size(105, 18);
            fileName.TabIndex = 18;
            fileName.Text = "No file selected";
            fileName.Visible = false;
            // 
            // FilenameText
            // 
            FilenameText.AutoSize = true;
            FilenameText.Font = new Font("Segoe UI", 11.25F);
            FilenameText.Location = new Point(643, 97);
            FilenameText.Name = "FilenameText";
            FilenameText.Size = new Size(100, 20);
            FilenameText.TabIndex = 16;
            FilenameText.Text = "Selected File :";
            FilenameText.Visible = false;
            // 
            // IncomingBox
            // 
            IncomingBox.BackColor = SystemColors.Control;
            IncomingBox.Controls.Add(progressBar1);
            IncomingBox.Controls.Add(label4);
            IncomingBox.Controls.Add(label3);
            IncomingBox.Controls.Add(IncomingFilePath);
            IncomingBox.Controls.Add(label2);
            IncomingBox.Controls.Add(IncomingBoxLabel);
            IncomingBox.Controls.Add(IncomingComboBox);
            IncomingBox.Controls.Add(incomingButton);
            IncomingBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IncomingBox.Location = new Point(18, 15);
            IncomingBox.Margin = new Padding(3, 2, 3, 2);
            IncomingBox.Name = "IncomingBox";
            IncomingBox.Padding = new Padding(3, 2, 3, 2);
            IncomingBox.Size = new Size(1324, 119);
            IncomingBox.TabIndex = 19;
            IncomingBox.TabStop = false;
            IncomingBox.Text = "Pull Data";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(1125, 76);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(188, 23);
            progressBar1.TabIndex = 13;
            progressBar1.Visible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Cursor = Cursors.IBeam;
            label4.Font = new Font("Calibri", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.FromArgb(50, 150, 80);
            label4.Location = new Point(741, 40);
            label4.MaximumSize = new Size(330, 30);
            label4.Name = "label4";
            label4.Size = new Size(105, 18);
            label4.TabIndex = 21;
            label4.Text = "No file selected";
            label4.Visible = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 11.25F);
            label3.Location = new Point(631, 38);
            label3.Name = "label3";
            label3.Size = new Size(103, 20);
            label3.TabIndex = 21;
            label3.Text = "Received File :";
            label3.Visible = false;
            // 
            // IncomingFilePath
            // 
            IncomingFilePath.AutoSize = true;
            IncomingFilePath.Font = new Font("Calibri", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IncomingFilePath.ForeColor = Color.Green;
            IncomingFilePath.Location = new Point(126, 90);
            IncomingFilePath.Name = "IncomingFilePath";
            IncomingFilePath.Size = new Size(74, 18);
            IncomingFilePath.TabIndex = 22;
            IncomingFilePath.Text = "Attach file ";
            IncomingFilePath.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 12F);
            label2.Location = new Point(17, 88);
            label2.Name = "label2";
            label2.Size = new Size(106, 20);
            label2.TabIndex = 21;
            label2.Text = "File saved to :";
            label2.Visible = false;
            // 
            // IncomingBoxLabel
            // 
            IncomingBoxLabel.AutoSize = true;
            IncomingBoxLabel.Font = new Font("Microsoft Sans Serif", 12F);
            IncomingBoxLabel.Location = new Point(16, 40);
            IncomingBoxLabel.Name = "IncomingBoxLabel";
            IncomingBoxLabel.Size = new Size(34, 20);
            IncomingBoxLabel.TabIndex = 7;
            IncomingBoxLabel.Text = "File";
            // 
            // IncomingComboBox
            // 
            IncomingComboBox.Cursor = Cursors.Hand;
            IncomingComboBox.FormattingEnabled = true;
            IncomingComboBox.Items.AddRange(new object[] { "Charts of Accounts", "Due Dates" });
            IncomingComboBox.Location = new Point(122, 36);
            IncomingComboBox.Name = "IncomingComboBox";
            IncomingComboBox.Size = new Size(197, 28);
            IncomingComboBox.TabIndex = 6;
            IncomingComboBox.SelectedIndexChanged += IncomingComboBox_SelectedIndexChanged;
            // 
            // incomingButton
            // 
            incomingButton.BackColor = Color.Silver;
            incomingButton.Cursor = Cursors.Hand;
            incomingButton.FlatStyle = FlatStyle.Flat;
            incomingButton.Font = new Font("Microsoft Sans Serif", 14.25F);
            incomingButton.ForeColor = SystemColors.Control;
            incomingButton.Location = new Point(1125, 22);
            incomingButton.Name = "incomingButton";
            incomingButton.Size = new Size(188, 48);
            incomingButton.TabIndex = 4;
            incomingButton.Text = "Run ";
            incomingButton.UseVisualStyleBackColor = false;
            incomingButton.Click += incomingButton_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(256, 459);
            label6.Name = "label6";
            label6.Size = new Size(0, 15);
            label6.TabIndex = 17;
            // 
            // FilePreviewLabel
            // 
            FilePreviewLabel.AutoSize = true;
            FilePreviewLabel.Font = new Font("Microsoft Sans Serif", 12F);
            FilePreviewLabel.Location = new Point(27, 274);
            FilePreviewLabel.Name = "FilePreviewLabel";
            FilePreviewLabel.Size = new Size(91, 20);
            FilePreviewLabel.TabIndex = 13;
            FilePreviewLabel.Text = "File preview";
            // 
            // FilePreview
            // 
            FilePreview.Location = new Point(18, 296);
            FilePreview.Name = "FilePreview";
            FilePreview.Size = new Size(1324, 233);
            FilePreview.TabIndex = 12;
            FilePreview.Text = "";
            FilePreview.TextChanged += FilePreview_TextChanged;
            // 
            // Manual
            // 
            Manual.AutoSize = true;
            Manual.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold);
            Manual.Location = new Point(27, 23);
            Manual.Name = "Manual";
            Manual.Size = new Size(78, 24);
            Manual.TabIndex = 5;
            Manual.Text = "Manual";
            // 
            // QuickBooksOnline
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1383, 600);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "QuickBooksOnline";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quickbook Online";
            Load += Home_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            OutgoingBox.ResumeLayout(false);
            OutgoingBox.PerformLayout();
            IncomingBox.ResumeLayout(false);
            IncomingBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button button2;
        private Panel panel1;
        private Panel panel3;
        private Label Manual;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private Label label2;
        private ComboBox comboBox1;
        private GroupBox RadioButtons;
        private Label label3;
        private RichTextBox richTextBox1;
        private Label label4;
        private Button button1;
        private Label fileName;
        private Label label6;
        private Label FilenameText;
        private Label label1;
        private GroupBox OutgoingBox;
        private ProgressBar progressBar2;
        private Label fileLocation;
        private Label label7;
        private Button outgoingButton;
        private Label OutgoingBoxLabel;
        private ComboBox OutgoingComboBox;
        private Button UploadButton;
        private Label attachedFile;
        private GroupBox IncomingBox;
        private ProgressBar progressBar1;
        private Label IncomingFilePath;
        private Label IncomingBoxLabel;
        private ComboBox IncomingComboBox;
        private Button incomingButton;
        private Label FilePreviewLabel;
        private RichTextBox FilePreview;
    }
}