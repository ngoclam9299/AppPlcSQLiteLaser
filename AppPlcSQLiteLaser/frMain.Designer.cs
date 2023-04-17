namespace AppPlcSQLiteLaser
{
    partial class frMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frMain));
            this.txtSqliteExe = new System.Windows.Forms.TextBox();
            this.btnBrowseSqliteExe = new System.Windows.Forms.Button();
            this.btnOpenSqlite = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.gbSqliteBrowser = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSQLiteTable = new System.Windows.Forms.TextBox();
            this.gbFolder = new System.Windows.Forms.GroupBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.rdvEN = new System.Windows.Forms.RadioButton();
            this.rdbVN = new System.Windows.Forms.RadioButton();
            this.btnCheck = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cbPort = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.gbSqliteBrowser.SuspendLayout();
            this.gbFolder.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSqliteExe
            // 
            resources.ApplyResources(this.txtSqliteExe, "txtSqliteExe");
            this.txtSqliteExe.Name = "txtSqliteExe";
            // 
            // btnBrowseSqliteExe
            // 
            resources.ApplyResources(this.btnBrowseSqliteExe, "btnBrowseSqliteExe");
            this.btnBrowseSqliteExe.Name = "btnBrowseSqliteExe";
            this.btnBrowseSqliteExe.UseVisualStyleBackColor = true;
            this.btnBrowseSqliteExe.Click += new System.EventHandler(this.btnBrowseSqliteExe_Click);
            // 
            // btnOpenSqlite
            // 
            resources.ApplyResources(this.btnOpenSqlite, "btnOpenSqlite");
            this.btnOpenSqlite.Name = "btnOpenSqlite";
            this.btnOpenSqlite.UseVisualStyleBackColor = true;
            this.btnOpenSqlite.Click += new System.EventHandler(this.btnOpenSqlite_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            // 
            // gbSqliteBrowser
            // 
            this.gbSqliteBrowser.Controls.Add(this.label2);
            this.gbSqliteBrowser.Controls.Add(this.label1);
            this.gbSqliteBrowser.Controls.Add(this.txtSQLiteTable);
            this.gbSqliteBrowser.Controls.Add(this.txtSqliteExe);
            this.gbSqliteBrowser.Controls.Add(this.dataGridView1);
            this.gbSqliteBrowser.Controls.Add(this.btnBrowseSqliteExe);
            this.gbSqliteBrowser.Controls.Add(this.btnOpenSqlite);
            resources.ApplyResources(this.gbSqliteBrowser, "gbSqliteBrowser");
            this.gbSqliteBrowser.Name = "gbSqliteBrowser";
            this.gbSqliteBrowser.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtSQLiteTable
            // 
            resources.ApplyResources(this.txtSQLiteTable, "txtSQLiteTable");
            this.txtSQLiteTable.Name = "txtSQLiteTable";
            // 
            // gbFolder
            // 
            this.gbFolder.Controls.Add(this.txtFolder);
            this.gbFolder.Controls.Add(this.btnBrowseFolder);
            this.gbFolder.Controls.Add(this.btnSave);
            resources.ApplyResources(this.gbFolder, "gbFolder");
            this.gbFolder.Name = "gbFolder";
            this.gbFolder.TabStop = false;
            // 
            // txtFolder
            // 
            resources.ApplyResources(this.txtFolder, "txtFolder");
            this.txtFolder.Name = "txtFolder";
            // 
            // btnBrowseFolder
            // 
            resources.ApplyResources(this.btnBrowseFolder, "btnBrowseFolder");
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // rdvEN
            // 
            resources.ApplyResources(this.rdvEN, "rdvEN");
            this.rdvEN.Name = "rdvEN";
            this.rdvEN.TabStop = true;
            this.rdvEN.UseVisualStyleBackColor = true;
            // 
            // rdbVN
            // 
            resources.ApplyResources(this.rdbVN, "rdbVN");
            this.rdbVN.Name = "rdbVN";
            this.rdbVN.TabStop = true;
            this.rdbVN.UseVisualStyleBackColor = true;
            this.rdbVN.CheckedChanged += new System.EventHandler(this.rdbVN_CheckedChanged);
            // 
            // btnCheck
            // 
            resources.ApplyResources(this.btnCheck, "btnCheck");
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            // 
            // textBox2
            // 
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.Name = "textBox2";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbPort);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnCheck);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.textBox2);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btnConnect
            // 
            resources.ApplyResources(this.btnConnect, "btnConnect");
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // cbPort
            // 
            this.cbPort.FormattingEnabled = true;
            resources.ApplyResources(this.cbPort, "cbPort");
            this.cbPort.Name = "cbPort";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // frMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rdbVN);
            this.Controls.Add(this.rdvEN);
            this.Controls.Add(this.gbFolder);
            this.Controls.Add(this.gbSqliteBrowser);
            this.Name = "frMain";
            this.Load += new System.EventHandler(this.frMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.gbSqliteBrowser.ResumeLayout(false);
            this.gbSqliteBrowser.PerformLayout();
            this.gbFolder.ResumeLayout(false);
            this.gbFolder.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtSqliteExe;
        private System.Windows.Forms.Button btnBrowseSqliteExe;
        private System.Windows.Forms.Button btnOpenSqlite;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox gbSqliteBrowser;
        private System.Windows.Forms.GroupBox gbFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.RadioButton rdvEN;
        private System.Windows.Forms.RadioButton rdbVN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSQLiteTable;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbPort;
        private System.Windows.Forms.Button btnConnect;
    }
}

