using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Resources;
using System.Globalization;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO.Ports;
using System.ComponentModel;



namespace AppPlcSQLiteLaser
{
    public partial class frMain : Form
    {
        // <Variable system>
        CultureInfo culture; // nhiều ngôn ngữ
        SQLiteConn Csqlite = new SQLiteConn();
        FolderText Ft = new FolderText();
        ConnScanner _Scanner = new ConnScanner();

        // <Variable frMain>
        DataTable dt = new DataTable(); // bang chứa dữ liệu từ sqlite về để hiện thị lên datagridview
        OpenFileDialog FileDB = new OpenFileDialog(); //mở file trong máy tính

        // <Variable SQLite>
        SQLiteConnection myConn = new SQLiteConnection(); // khởi tạo kết nối với SQLite
        string sqConnectionString; // đường dẫn tới database xx.db
        string Path = ""; // đường dẫn đến thư mục chứa database
        string NameDb = "";// tên của database
        string tableDB;// tên bảng chứa dữ liệu trong database

        //<Variable FolderText>
        string sPath = "";
        string ResultDB = "";

        //<Scanner>
        private BackgroundWorker ScannerResult;

        public frMain()
        {
            InitializeComponent();
            this.FormClosing += FrMain_FormClosing;
        }

        private void FrMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ScannerResult.CancelAsync();
            RegistryHelper.SaveAllSettings(Application.ProductName, this);
            frLogin frL = new frLogin();
            frL.Close();
        }

        private void frMain_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            RegistryHelper.LoadAllSettings(Application.ProductName, this);
            if (tableDB == null) // nếu chưa điền tên bảng cần truy cập đến trong databasae thì gán mặc định
            {
                tableDB = "schneider";
            }
            if (sPath == null)
            {
                sPath = @"D:\";
            }
            GetListComPort();
            Csqlite.createConection(txtSqliteExe.Text);
            ScannerResult = new BackgroundWorker();
            ScannerResult.WorkerReportsProgress = true; // ho tro bao cao tien do
            ScannerResult.WorkerSupportsCancellation = true; // cho phep dung tien trinh
                                                             // su kien
            ScannerResult.DoWork += ScannerResult_DoWork;
            ScannerResult.ProgressChanged += ScannerResult_ProgressChanged;
            ScannerResult.RunWorkerCompleted += ScannerResult_RunWorkerCompleted;
            ScannerResult.RunWorkerAsync();
        }
        private void btnBrowseSqliteExe_Click(object sender, EventArgs e)
        {
            if (FileDB.ShowDialog() == DialogResult.OK)
            {
                NameDb = FileDB.SafeFileName;
                Path = FileDB.FileName.Remove((FileDB.FileName.LastIndexOf(NameDb)));
                txtSqliteExe.Text = FileDB.FileName;
                Csqlite.createConection(FileDB.FileName);
            }
        }
        private void btnOpenSqlite_Click(object sender, EventArgs e)
        {
            tableDB = txtSQLiteTable.Text;
            Csqlite.loadDataToGrid(dataGridView1, tableDB);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            Ft.CreNewFolder(sPath, ResultDB);
        }
        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog BrowseFolder = new FolderBrowserDialog();
            if (BrowseFolder.ShowDialog() == DialogResult.OK)
            {
                sPath = string.Format(BrowseFolder.SelectedPath);
                txtFolder.Text = BrowseFolder.SelectedPath;
            }
        }
        private void rdbVN_CheckedChanged(object sender, EventArgs e) // lua chon ngon ngu
        {
            if (rdvEN.Checked)
                SetLanguage("en");
            else if (rdbVN.Checked)
                SetLanguage("vi");
        }
        //Sub Function system
        private void SetLanguage(string cultureName)
        {
            culture = CultureInfo.CreateSpecificCulture(cultureName);
            ResourceManager rm = new
                ResourceManager("AppPlcSQLiteLaser.Translation.Caption", typeof(frMain).Assembly);
            gbFolder.Text = rm.GetString("gbFolder", culture);
            btnSave.Text = rm.GetString("btnSave", culture);
        }
        public void GetListComPort()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                cbPort.Items.Add(port);
            }
            if (ports.Length > 0)
            {
                cbPort.SelectedIndex = 0;
            }
        }
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread str of the
            // calling thread to the thread str of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _Scanner.ScannerConn(cbPort.Text);
        }

        public void btnCheck_Click(object sender, EventArgs e)
        {
            _Scanner.Triggrt_on();
            while (true)
            {
                if (_Scanner.ReadStr() != null)
                {
                    SetText(_Scanner.ReadStr());
                    break;
                }
                break;
            }
        }
        void ScannerResult_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Hoan thanh");
        }

        void ScannerResult_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Application.DoEvents();
        }

        void ScannerResult_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                /*
                SetText(_Scanner.ReadStr());
                Image img = _Scanner.ReadImg();
                if (img != null )
                {
                    pictureBox1.Image = resizeImage(img, new Size(320, 164));
                }
                */
                if(_Scanner.ReadStr()!=null)
                {
                    SetText(_Scanner.ReadStr());
                }    
            }
        }
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }
    }
}