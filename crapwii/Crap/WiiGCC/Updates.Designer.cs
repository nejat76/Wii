namespace WiiGSC
{
    partial class Updates
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
            this.btnCheckUpdates = new System.Windows.Forms.Button();
            this.updateList = new System.Windows.Forms.ListView();
            this.Action = new System.Windows.Forms.ColumnHeader();
            this.Content = new System.Windows.Forms.ColumnHeader();
            this.ExistingVersion = new System.Windows.Forms.ColumnHeader();
            this.NewVersion = new System.Windows.Forms.ColumnHeader();
            this.Status = new System.Windows.Forms.ColumnHeader();
            this.Description = new System.Windows.Forms.ColumnHeader();
            this.btnProcessUpdate = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.packageProgress = new System.Windows.Forms.ProgressBar();
            this.fileProgress = new System.Windows.Forms.ProgressBar();
            this.packageProgressStatus = new System.Windows.Forms.Label();
            this.fileProgressStatus = new System.Windows.Forms.Label();
            this.outputGroupBox = new System.Windows.Forms.GroupBox();
            this.totalBytesTextBox = new System.Windows.Forms.TextBox();
            this.bytesDownloadedTextBox = new System.Windows.Forms.TextBox();
            this.lblBytesDownloaded = new System.Windows.Forms.Label();
            this.lblDownloadProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblTotalBytes = new System.Windows.Forms.Label();
            this.lblFileProgress = new System.Windows.Forms.Label();
            this.lblPackageProgress = new System.Windows.Forms.Label();
            this.outputGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCheckUpdates
            // 
            this.btnCheckUpdates.Location = new System.Drawing.Point(23, 23);
            this.btnCheckUpdates.Name = "btnCheckUpdates";
            this.btnCheckUpdates.Size = new System.Drawing.Size(165, 23);
            this.btnCheckUpdates.TabIndex = 0;
            this.btnCheckUpdates.Text = "Check for Updates";
            this.btnCheckUpdates.UseVisualStyleBackColor = true;
            this.btnCheckUpdates.Click += new System.EventHandler(this.btnCheckUpdates_Click);
            // 
            // updateList
            // 
            this.updateList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Action,
            this.Content,
            this.ExistingVersion,
            this.NewVersion,
            this.Status,
            this.Description});
            this.updateList.Location = new System.Drawing.Point(23, 52);
            this.updateList.Name = "updateList";
            this.updateList.Size = new System.Drawing.Size(704, 198);
            this.updateList.TabIndex = 2;
            this.updateList.UseCompatibleStateImageBehavior = false;
            this.updateList.View = System.Windows.Forms.View.Details;
            this.updateList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListView1_ItemCheck1);
            // 
            // Action
            // 
            this.Action.Text = "";
            this.Action.Width = 50;
            // 
            // Content
            // 
            this.Content.Text = "Filename";
            this.Content.Width = 150;
            // 
            // ExistingVersion
            // 
            this.ExistingVersion.Text = "Existing Version";
            this.ExistingVersion.Width = 90;
            // 
            // NewVersion
            // 
            this.NewVersion.Text = "New Version";
            this.NewVersion.Width = 90;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 120;
            // 
            // Description
            // 
            this.Description.Text = "Description";
            this.Description.Width = 200;
            // 
            // btnProcessUpdate
            // 
            this.btnProcessUpdate.Location = new System.Drawing.Point(23, 274);
            this.btnProcessUpdate.Name = "btnProcessUpdate";
            this.btnProcessUpdate.Size = new System.Drawing.Size(165, 23);
            this.btnProcessUpdate.TabIndex = 3;
            this.btnProcessUpdate.Text = "Process updates";
            this.btnProcessUpdate.UseVisualStyleBackColor = true;
            this.btnProcessUpdate.Click += new System.EventHandler(this.btnProcessUpdate_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // packageProgress
            // 
            this.packageProgress.Location = new System.Drawing.Point(350, 274);
            this.packageProgress.Name = "packageProgress";
            this.packageProgress.Size = new System.Drawing.Size(134, 23);
            this.packageProgress.TabIndex = 4;
            // 
            // fileProgress
            // 
            this.fileProgress.Location = new System.Drawing.Point(350, 303);
            this.fileProgress.Name = "fileProgress";
            this.fileProgress.Size = new System.Drawing.Size(134, 22);
            this.fileProgress.TabIndex = 5;
            // 
            // packageProgressStatus
            // 
            this.packageProgressStatus.AutoSize = true;
            this.packageProgressStatus.Location = new System.Drawing.Point(491, 278);
            this.packageProgressStatus.Name = "packageProgressStatus";
            this.packageProgressStatus.Size = new System.Drawing.Size(0, 13);
            this.packageProgressStatus.TabIndex = 6;
            // 
            // fileProgressStatus
            // 
            this.fileProgressStatus.AutoSize = true;
            this.fileProgressStatus.Location = new System.Drawing.Point(491, 308);
            this.fileProgressStatus.Name = "fileProgressStatus";
            this.fileProgressStatus.Size = new System.Drawing.Size(0, 13);
            this.fileProgressStatus.TabIndex = 7;
            // 
            // outputGroupBox
            // 
            this.outputGroupBox.Controls.Add(this.totalBytesTextBox);
            this.outputGroupBox.Controls.Add(this.bytesDownloadedTextBox);
            this.outputGroupBox.Controls.Add(this.lblBytesDownloaded);
            this.outputGroupBox.Controls.Add(this.lblDownloadProgress);
            this.outputGroupBox.Controls.Add(this.progressBar);
            this.outputGroupBox.Controls.Add(this.lblTotalBytes);
            this.outputGroupBox.Enabled = false;
            this.outputGroupBox.Location = new System.Drawing.Point(66, 340);
            this.outputGroupBox.Name = "outputGroupBox";
            this.outputGroupBox.Size = new System.Drawing.Size(562, 120);
            this.outputGroupBox.TabIndex = 10;
            this.outputGroupBox.TabStop = false;
            this.outputGroupBox.Text = "Download Status";
            // 
            // totalBytesTextBox
            // 
            this.totalBytesTextBox.Location = new System.Drawing.Point(146, 56);
            this.totalBytesTextBox.Name = "totalBytesTextBox";
            this.totalBytesTextBox.ReadOnly = true;
            this.totalBytesTextBox.Size = new System.Drawing.Size(168, 20);
            this.totalBytesTextBox.TabIndex = 4;
            this.totalBytesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // bytesDownloadedTextBox
            // 
            this.bytesDownloadedTextBox.Location = new System.Drawing.Point(146, 24);
            this.bytesDownloadedTextBox.Name = "bytesDownloadedTextBox";
            this.bytesDownloadedTextBox.ReadOnly = true;
            this.bytesDownloadedTextBox.Size = new System.Drawing.Size(168, 20);
            this.bytesDownloadedTextBox.TabIndex = 3;
            this.bytesDownloadedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblBytesDownloaded
            // 
            this.lblBytesDownloaded.Location = new System.Drawing.Point(16, 28);
            this.lblBytesDownloaded.Name = "lblBytesDownloaded";
            this.lblBytesDownloaded.Size = new System.Drawing.Size(100, 23);
            this.lblBytesDownloaded.TabIndex = 2;
            this.lblBytesDownloaded.Text = "Bytes Downloaded";
            // 
            // lblDownloadProgress
            // 
            this.lblDownloadProgress.Location = new System.Drawing.Point(16, 88);
            this.lblDownloadProgress.Name = "lblDownloadProgress";
            this.lblDownloadProgress.Size = new System.Drawing.Size(104, 23);
            this.lblDownloadProgress.TabIndex = 1;
            this.lblDownloadProgress.Text = "Download Progress";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(146, 88);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(376, 23);
            this.progressBar.TabIndex = 0;
            // 
            // lblTotalBytes
            // 
            this.lblTotalBytes.Location = new System.Drawing.Point(16, 60);
            this.lblTotalBytes.Name = "lblTotalBytes";
            this.lblTotalBytes.Size = new System.Drawing.Size(100, 23);
            this.lblTotalBytes.TabIndex = 2;
            this.lblTotalBytes.Text = "Total Bytes";
            // 
            // lblFileProgress
            // 
            this.lblFileProgress.AutoSize = true;
            this.lblFileProgress.Location = new System.Drawing.Point(204, 308);
            this.lblFileProgress.Name = "lblFileProgress";
            this.lblFileProgress.Size = new System.Drawing.Size(67, 13);
            this.lblFileProgress.TabIndex = 12;
            this.lblFileProgress.Text = "File Progress";
            // 
            // lblPackageProgress
            // 
            this.lblPackageProgress.AutoSize = true;
            this.lblPackageProgress.Location = new System.Drawing.Point(204, 278);
            this.lblPackageProgress.Name = "lblPackageProgress";
            this.lblPackageProgress.Size = new System.Drawing.Size(94, 13);
            this.lblPackageProgress.TabIndex = 11;
            this.lblPackageProgress.Text = "Package Progress";
            // 
            // Updates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 498);
            this.Controls.Add(this.lblFileProgress);
            this.Controls.Add(this.lblPackageProgress);
            this.Controls.Add(this.outputGroupBox);
            this.Controls.Add(this.fileProgressStatus);
            this.Controls.Add(this.packageProgressStatus);
            this.Controls.Add(this.fileProgress);
            this.Controls.Add(this.packageProgress);
            this.Controls.Add(this.btnProcessUpdate);
            this.Controls.Add(this.updateList);
            this.Controls.Add(this.btnCheckUpdates);
            this.Name = "Updates";
            this.Text = "Updates";
            this.Load += new System.EventHandler(this.btnProcessUpdates_Load);
            this.outputGroupBox.ResumeLayout(false);
            this.outputGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheckUpdates;
        private System.Windows.Forms.ListView updateList;
        private System.Windows.Forms.ColumnHeader Action;
        private System.Windows.Forms.ColumnHeader Content;
        private System.Windows.Forms.ColumnHeader ExistingVersion;
        private System.Windows.Forms.ColumnHeader NewVersion;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.ColumnHeader Description;
        private System.Windows.Forms.Button btnProcessUpdate;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar packageProgress;
        private System.Windows.Forms.ProgressBar fileProgress;
        private System.Windows.Forms.Label packageProgressStatus;
        private System.Windows.Forms.Label fileProgressStatus;
        private System.Windows.Forms.GroupBox outputGroupBox;
        private System.Windows.Forms.TextBox totalBytesTextBox;
        private System.Windows.Forms.TextBox bytesDownloadedTextBox;
        private System.Windows.Forms.Label lblBytesDownloaded;
        private System.Windows.Forms.Label lblDownloadProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblTotalBytes;
        private System.Windows.Forms.Label lblFileProgress;
        private System.Windows.Forms.Label lblPackageProgress;
    }
}