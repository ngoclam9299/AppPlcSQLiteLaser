using EasyModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppPlcSQLiteLaser
{
    public partial class frMain : Form
    {
        //<Struct Data Product>
        struct ProductData
        {
            public int ProductId;
            public string ProductCode;
            public string TestedByUserName;
            public string TesterName;
            public string TypeOfTest;
            public string FixtureName;
            public string PCBA_PartNumber;
            public string CommercialReference;
            public string CavityLaserMark;
            public int TestedResult;
            public DateTime TestedDate;
            public int LaserMarkedResult;
            public DateTime LaserMarkedDate;
            public string EUI64;
            public string InstallCode;
            public string EUI64WithSpace;
            public string InstallCodeWithSpace;
        }
        //<Struct ProductMarkResult>
        struct ProductMarkResult
        {
            public string ProductCode;
            public int TestedResult;
            public DateTime TestedDate;
            public int LaserMarkedResult;
            public DateTime LaserMarkedDate;
        }
        // <Variable system>
        CultureInfo culture; //ngôn ngữ
        SQLiteConn Csqlite = new SQLiteConn(); // khai báo Class SQLite
        ConnScanner _Scanner = new ConnScanner(); // khai báo Class Scanner
        // <Variable frMain>
        DataTable dt = new DataTable(); // bang chứa dữ liệu từ sqlite về để hiện thị lên datagridview
        OpenFileDialog FileDB = new OpenFileDialog(); //mở file trong máy tính
        // <Variable SQLite>
        string NameDb = "";// tên của database
        string tableDB;// tên bảng chứa dữ liệu trong database
        //<Variable FolderText>
        private bool _bFolderAvailable = false; //Ping Folder
        ProductData PDTest;
        private BackgroundWorker _folderBackgroundWorker = null;
        //<Scanner>
        private BackgroundWorker ScannerResult;
        int iCount = 0;
        //<Search Data>
        private List<object> ProductList = new List<object>();
        public int TotalRecords = 0;
        public int PageSize = 30;
        //<Modbus Client>
        private BackgroundWorker  ModBus;
        ModbusClient mPLCM340 = new ModbusClient();
        ProductMarkResult PdM1;
        ProductMarkResult PdM2;
        private bool _bServerAvailable;
        private bool _MBResiAvailable;
        int[] MarkResult;
        int[] iPLCMode;
        int[] iMraking;
        private string Ip = "";
        string RegisPing = "";
        string RegisSttPLC = "";
        string RegisModeMark = "";
        string RegisMarking = "";
        string RegisMarkingResult = "";
        string RegisProductCode1 = "";
        string RegisEUI1 = "";
        string RegisInstallCode1 = "";
        string RegisProductCode2 = "";
        string RegisEUI2 = "";
        string RegisInstallCode2 = "";
        public frMain()
        {
            InitializeComponent();
            this.FormClosing += FrMain_FormClosing;
        }
        //<<<Form Main>>>
        private void FrMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ScannerResult.CancelAsync();
            RegistryHelper.SaveAllSettings(Application.ProductName, this);
            Application.Exit();
            _Scanner.Disconnect();
        }
        private void frMain_Load(object sender, EventArgs e)
        {
            dataGridView1.Size = new System.Drawing.Size(473, 197);
            dataGridView1.Location = new Point(0, 40);
            RegistryHelper.LoadAllSettings(Application.ProductName, this); // load seting registry
            if (tableDB == null) // nếu chưa điền tên bảng cần truy cập đến trong databasae thì gán mặc định
            {
                tableDB = "Product";
            }
            else
            {
                tableDB = txtSQLiteTable.Text;
            }
            GetListComPort();// get com scanner
            ReadSysConfig();
            rdbVN.Checked = true;
            if (txtSqliteExe.Text != null)
                Csqlite.createConection(txtSqliteExe.Text); // tạo kết nối với sqlite
            ScannerResult = new BackgroundWorker();
            ScannerResult.WorkerReportsProgress = true; // ho tro bao cao tien do
            ScannerResult.WorkerSupportsCancellation = true; // cho phep dung tien trinh
            ScannerResult.DoWork += ScannerResult_DoWork;
            ScannerResult.ProgressChanged += ScannerResult_ProgressChanged;
            ScannerResult.RunWorkerCompleted += ScannerResult_RunWorkerCompleted;
            ScannerResult.RunWorkerAsync();
            PingFolder();
            StartFolderBackgroundWorker();
            checkRegister();
            mPLCM340.IPAddress = Ip;
            mPLCM340.Port = Convert.ToInt32(502);
            mPLCM340.ConnectionTimeout = Convert.ToInt32(50000);
            mPLCM340.Connect();
            if (mPLCM340.Connected)
            {
                int[] val = mPLCM340.ReadHoldingRegisters(406, 4);
                Console.WriteLine(val[0].ToString());
                Console.WriteLine(val[1].ToString());
                ModBus = new BackgroundWorker();
                ModBus.WorkerReportsProgress = true; // ho tro bao cao tien do
                ModBus.WorkerSupportsCancellation = true; // cho phep dung tien trinh
                ModBus.DoWork += ModBus_DoWork;
                ModBus.ProgressChanged += ModBus_ProgressChanged;
                ModBus.RunWorkerCompleted += ModBus_RunWorkerCompleted;
                ModBus.RunWorkerAsync();
            }
            else
                MessageBox.Show("Không Khởi tạo được Modbus, vui lòng kiểm tra kết nối","Error",MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
        }
        /*------------------------------------------------------------------------------------------------------*/
        //<<<Folder_BackgroundWorker>>>
        public void StartFolderBackgroundWorker()
        {
            if (_folderBackgroundWorker == null)
            {
                _folderBackgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                _folderBackgroundWorker.DoWork += _folderBackgroundWorker_DoWork;
                _folderBackgroundWorker.ProgressChanged += _folderBackgroundWorker_ProgressChanged;
            }
            _folderBackgroundWorker.RunWorkerAsync();
        }
        public void StopFolderBackgroundWorker()
        {
            if (_folderBackgroundWorker != null)
            {
                _folderBackgroundWorker.CancelAsync();
            }
        }
        public void _folderBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string applicationPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Data";

            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                Thread.Sleep(500);
                PingFolder();

                //if folder is not available, set ProgressBar = 100 to indicate that it stoped
                //show to UI
                if (!_bFolderAvailable)
                {
                    worker.ReportProgress(1, null);
                    continue;
                }
                try
                {
                    var files = Directory.EnumerateFiles(FolderHelper.Folder, "*.txt")
                        .Select(fn => new
                        {
                            Path = fn,
                            Split = AnalyzeFileName(Path.GetFileNameWithoutExtension(fn))
                        })
                        .Where(c => c.Split.Count >= 2)
                        .Select(x => new
                        {
                            Path = x.Path,
                            ProductCode = x.Split[0],
                            TestedDate = new DateTime(
                                x.Split.Count >= 2 ? int.Parse(x.Split[1]) : DateTime.MinValue.Year,
                                x.Split.Count >= 3 ? int.Parse(x.Split[2]) : DateTime.MinValue.Month,
                                x.Split.Count >= 4 ? int.Parse(x.Split[3]) : DateTime.MinValue.Day,
                                x.Split.Count >= 5 ? int.Parse(x.Split[4]) : DateTime.MinValue.Hour,
                                x.Split.Count >= 6 ? int.Parse(x.Split[5]) : DateTime.MinValue.Minute,
                                x.Split.Count >= 7 ? int.Parse(x.Split[6]) : DateTime.MinValue.Second)
                        })
                        //.Where(c => c.CreatedDate >= DateTime.Parse("2016.01.01"))
                        .OrderByDescending(x => x.TestedDate).Skip(0).Take(100);
                    if (files != null && files.Count() > 0)
                    {
                        foreach (var file in files)
                        {
                            if (!_bFolderAvailable) break;
                            string filePath = file.Path;
                            PDTest.TestedDate = file.TestedDate;
                            //read content file
                            if (File.Exists(filePath))
                            {
                                string content = File.ReadAllText(filePath);
                                if (content != null)
                                {
                                    string[] arrLines = EliminateEmptyLine(content.Split('\n'));
                                    //first item is product code, always start with 'S' => remove it
                                    if (arrLines != null && arrLines.Length > 0)
                                    {
                                        if (arrLines[0].StartsWith("S"))
                                        {
                                            PDTest.ProductCode = arrLines[0].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.ProductCode = arrLines[0];
                                        }
                                    }
                                    //first item is product code, always start with 'S' => remove it
                                    if (arrLines != null && arrLines.Length > 2)
                                    {
                                        if (arrLines[2].StartsWith("O"))
                                        {
                                            PDTest.TestedByUserName = arrLines[2].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.TestedByUserName = arrLines[2];
                                        }
                                    }
                                    if (arrLines != null && arrLines.Length > 3)
                                    {
                                        if (arrLines[3].StartsWith("N"))
                                        {
                                            PDTest.TesterName = arrLines[3].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.TesterName = arrLines[3];
                                        }
                                    }
                                    if (arrLines != null && arrLines.Length > 4)
                                    {
                                        if (arrLines[4].StartsWith("P"))
                                        {
                                            PDTest.TypeOfTest = arrLines[4].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.TypeOfTest = arrLines[4];
                                        }
                                    }
                                    if (arrLines != null && arrLines.Length > 5)
                                    {
                                        if (arrLines[5].StartsWith("f"))
                                        {
                                            PDTest.FixtureName = arrLines[5].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.FixtureName = arrLines[5];
                                        }
                                    }
                                    if (arrLines != null && arrLines.Length > 6)
                                    {
                                        if (arrLines[6].StartsWith("n"))
                                        {
                                            PDTest.PCBA_PartNumber = arrLines[6].Substring(1);
                                        }
                                        else
                                        {
                                            PDTest.PCBA_PartNumber = arrLines[6];
                                        }
                                    }
                                    if (arrLines != null && arrLines.Length > 8)
                                    {
                                        if (string.Compare(arrLines[8], "TP", true) == 0)
                                        {
                                            PDTest.TestedResult = 1;
                                        }
                                        else
                                        {
                                            PDTest.TestedResult = 0;
                                        }
                                    }

                                    //check if has EUI64 and install code or not
                                    if (arrLines != null && arrLines.Length > 12)
                                    {
                                        if (arrLines != null && arrLines.Length > 11)
                                        {
                                            PDTest.EUI64 = arrLines[11];
                                        }
                                        else
                                        {
                                            PDTest.EUI64 = "";
                                        }
                                        if (arrLines != null && arrLines.Length > 12 && arrLines[12] != "Install Code:")
                                        {
                                            PDTest.EUI64WithSpace = arrLines[12];
                                        }
                                        else
                                        {
                                            PDTest.EUI64WithSpace = "";
                                        }
                                        if (arrLines != null && arrLines.Length > 14 && arrLines[12] != "Install Code:")
                                        {
                                            PDTest.InstallCode = arrLines[14];
                                        }
                                        else if (arrLines != null && arrLines.Length > 14 && arrLines[12] == "Install Code:")
                                        {
                                            PDTest.InstallCode = arrLines[13];
                                        }
                                        else
                                        {
                                            PDTest.InstallCode = "";
                                        }
                                        if (arrLines != null && arrLines.Length > 15 && arrLines[12] != "Install Code:" && arrLines[13] == "Install Code:")
                                        {
                                            PDTest.InstallCodeWithSpace = arrLines[15];
                                        }
                                        else
                                        {
                                            PDTest.InstallCodeWithSpace = "";
                                        }
                                    }
                                }
                                //move file to SchneiderApp
                                if (PDTest.ProductId >= 0)
                                {
                                    string folderPath = applicationPath.TrimEnd('\\') + "\\" + PDTest.TestedDate.Year + "\\" + PDTest.TestedDate.Month + "\\" + PDTest.TestedDate.Day;
                                    if (!Directory.Exists(folderPath))
                                    {
                                        Directory.CreateDirectory(folderPath);
                                    }
                                    if (!File.Exists(folderPath + "\\" + Path.GetFileName(filePath)))
                                    {
                                        File.Move(filePath, folderPath + "\\" + Path.GetFileName(filePath));
                                        //Console.WriteLine("Move");
                                        if (PDTest.TestedResult == 1)
                                        {
                                            Csqlite.InsertDataBD(tableDB, PDTest.ProductCode, PDTest.TestedByUserName, PDTest.TesterName, PDTest.TypeOfTest, PDTest.FixtureName, PDTest.PCBA_PartNumber, PDTest.CommercialReference, PDTest.CavityLaserMark, PDTest.TestedResult, PDTest.TestedDate.ToString("yyyy-MM-dd HH:mm:ss tt"), PDTest.LaserMarkedResult, "", PDTest.EUI64, PDTest.InstallCode, PDTest.EUI64WithSpace, PDTest.InstallCodeWithSpace);
                                            Console.WriteLine("Inser");
                                        }
                                    }
                                    else
                                    {
                                        File.Delete(filePath);
                                        //Console.WriteLine("Delete");
                                    }
                                    //show to UI
                                    worker.ReportProgress(0, PDTest);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //log4net here
                }
            }
        }
        private void PingFolder()
        {
            _bFolderAvailable = false;
            if (!string.IsNullOrEmpty(FolderHelper.Folder))
            {
                Task<bool> taskA = Task.Factory.StartNew(() => Directory.Exists(FolderHelper.Folder));
                if (taskA.Wait(3000) && taskA.Result)
                {
                    _bFolderAvailable = taskA.Result;
                }
                if (!_bFolderAvailable)
                    lbFolderStatus.Text = "Stop";
                else
                    lbFolderStatus.Text = "Running";
            }
        } // ping đến vị trí thư mục chứa dữ liệu test
        public void _folderBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percentage = e.ProgressPercentage;
            if (percentage == 100)
            {
                Console.WriteLine(percentage);
                txtLatestTestedProduceCode.Text = "";
                txtLatestTestedStatus.Text = "";
            }
            else
            {
                Console.WriteLine(PDTest.ProductCode);
                txtLatestTestedProduceCode.Text = PDTest.ProductCode;
                txtLatestTestedStatus.Text = PDTest.TestedResult.ToString();
            }
        }
        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            // Show the FolderBrowserDialog.
            DialogResult result = folderOpenDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFolder.Text = folderOpenDlg.SelectedPath;
            }
        }
        /*------------------------------------------------------------------------------------------------------*/
        //<ModBus_BackgroundWorker>
        void ModBus_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Modbus Hoan thanh");
        }
        void ModBus_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Application.DoEvents();
        }
        void ModBus_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // Code cần xử lý
                DateTime aDateTime = DateTime.Now;
                Thread.Sleep(300);
                Application.DoEvents();
                mPLCM340.WriteSingleRegister(int.Parse(RegisPing), 1);
                iPLCMode = mPLCM340.ReadHoldingRegisters(int.Parse(RegisModeMark), 1); //1: khắc 1sp bên 1, 2:khắc 1sp bên 2, 3:khắc 2sp.
                MarkResult = mPLCM340.ReadHoldingRegisters(int.Parse(RegisMarkingResult), 1);  //1:đã khắc sp1, 2:đã khắc sp2,3:đã khắc 2 sp.
                iMraking = mPLCM340.ReadHoldingRegisters(int.Parse(RegisMarking), 1); //bằng 0 thì làm chu trình quét mới, bằng 1 PLC chưa nhận đc kq
                if (MarkResult[0] == 1 && iPLCMode[0] == 1)
                {
                    Csqlite.UpdateDataBD(tableDB, PdM1.ProductCode, PdM1.TestedDate.ToString("yyyy-MM-dd HH:mm:ss tt"), aDateTime.ToString("yyyy-MM-dd HH:mm:ss tt"), "1");
                    Console.WriteLine("Update Mark Result P1");
                    PdM1 = default;
                    mPLCM340.WriteSingleRegister(int.Parse(RegisMarkingResult), 0);
                }
                else if (MarkResult[0] == 2 && iPLCMode[0] == 2)
                {
                    Csqlite.UpdateDataBD(tableDB, PdM2.ProductCode, PdM2.TestedDate.ToString("yyyy-MM-dd HH:mm:ss tt"), aDateTime.ToString("yyyy-MM-dd HH:mm:ss tt"), "2");
                    Console.WriteLine("Update Mark Result P2");
                    PdM2 = default;
                    mPLCM340.WriteSingleRegister(int.Parse(RegisMarkingResult), 0);
                }
                else if (MarkResult[0] == 3 && iPLCMode[0] == 3)
                {
                    Csqlite.UpdateDataBD(tableDB, PdM1.ProductCode, PdM1.TestedDate.ToString("yyyy-MM-dd HH:mm:ss tt"), aDateTime.ToString("yyyy-MM-dd HH:mm:ss tt"), "1");
                    Console.WriteLine("Update Mark Result P1");
                    Csqlite.UpdateDataBD(tableDB, PdM2.ProductCode, PdM2.TestedDate.ToString("yyyy-MM-dd HH:mm:ss tt"), aDateTime.ToString("yyyy-MM-dd HH:mm:ss tt"), "2");
                    Console.WriteLine("Update Mark Result P2");
                    PdM1 = default;
                    PdM2 = default;
                    mPLCM340.WriteSingleRegister(int.Parse(RegisMarkingResult), 0);
                }
            }
        } 
        /*------------------------------------------------------------------------------------------------------*/
        //<<<ScannerResult_BackgroundWorker>>>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            _Scanner.ScannerConn(cbPort.Text);
        }
        public void btnCheck_Click(object sender, EventArgs e)
        {
            if (chbDisScanner.Checked == false)
            {
                txtWriteProduct.Enabled = false;
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
            else if (chbDisScanner.Checked == true)
            {
                txtWriteProduct.Enabled = true;
                _Scanner.Disconnect();
                if (_MBResiAvailable == true)
                {
                    if (iPLCMode[0] == 1) // display gb Product
                    {
                        gbProduct1.Enabled = true;
                        gbProduct2.Enabled = false;
                        lbDateTestP2.Text = "";
                        lbResultP2.Text = "";
                    }
                    else if (iPLCMode[0] == 2)
                    {
                        gbProduct2.Enabled = true;
                        gbProduct1.Enabled = false;
                        lbDateTestP1.Text = "";
                        lbResultP1.Text = "";
                    }
                    else if (iPLCMode[0] == 3)
                    {
                        gbProduct2.Enabled = true;
                        gbProduct1.Enabled = true;
                    }
                    DateTime aDateTime = DateTime.Now;
                    List<string> Split = new List<string>();
                    Split = AnalyzeFileName(txtWriteProduct.Text);
                    string ProductCode = Split[0];
                    string[] Rec = Csqlite.SelecteDataDb(tableDB, ProductCode); // theo ProductCode lấy dữ liệu từ database
                    if (Rec[3] == "1")
                    {
                        MessageBox.Show("Sản phẩm đã được khắc thành công", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        iCount++;
                        if (iCount == 1 && iPLCMode[0] == 1 && iMraking[0] == 0) // đọc trạng thái cấu hình hiện tại
                        {
                            //lưu kết quả sp đã quét 1
                            PdM1.ProductCode = Rec[0];
                            PdM1.TestedResult = int.Parse(Rec[1]);
                            PdM1.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP1.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP1.Text = "Pass";
                            }
                            else
                            {
                                lbResultP1.Text = "Fail";
                            }
                            lbDateTestP1.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI1), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode1), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 1" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else if (iCount == 1 && iPLCMode[0] == 2 && iMraking[0] == 0) // quét lần 1, mode chạy 2, đã khắc xong.
                        {
                            //lưu kết quả sp đã quét 2
                            PdM2.ProductCode = Rec[0];
                            PdM2.TestedResult = int.Parse(Rec[1]);
                            PdM2.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP2.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP2.Text = "Pass";
                            }
                            else
                            {
                                lbResultP2.Text = "Fail";
                            }
                            lbDateTestP2.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI2), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode2), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 2" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else if (iCount == 1 && iPLCMode[0] == 3 && iMraking[0] == 0)
                        {
                            //lưu kết quả sp đã quét 1
                            PdM1.ProductCode = Rec[0];
                            PdM1.TestedResult = int.Parse(Rec[1]);
                            PdM1.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP1.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP1.Text = "Pass";
                            }
                            else
                            {
                                lbResultP1.Text = "Fail";
                            }
                            lbDateTestP1.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI1), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode1), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 3" + iCount.ToString());
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 0);
                        }
                        else if (iCount == 2 && iPLCMode[0] == 3 && iMraking[0] == 0)
                        {
                            //lưu kết quả sp đã quét 2
                            PdM2.ProductCode = Rec[0];
                            PdM2.TestedResult = int.Parse(Rec[1]);
                            PdM2.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP2.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP2.Text = "Pass";
                            }
                            else
                            {
                                lbResultP2.Text = "Fail";
                            }
                            lbDateTestP2.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI2), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode2), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 4" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else
                        {
                            Console.WriteLine(" PLCMode:{0}\n Count:{1}\n Marking:{2}", iPLCMode[0], iCount, iMraking[0]);
                            MessageBox.Show("Lỗi bảo trì hệ thống 1, Vui lòng liên hệ đơn vị cung cấp", "LỖI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Lỗi bảo trì hệ thống 2, Vui lòng liên hệ đơn vị cung cấp", "LỖI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void GetListComPort()
        {
            cbPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                cbPort.Items.Add(port);
            }
            if (ports.Length > 0)
            {
                cbPort.SelectedIndex = 0;
            }
        } // đọc các cổng com về
        delegate void SetTextCallback(string text);
        private async void SetText(string text)
        {
            if (this.txtScannerResult.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (_MBResiAvailable == true)
                {
                    if(iPLCMode[0]==1) // display gb Product
                    {
                        gbProduct1.Enabled=true;
                        gbProduct2.Enabled = false;
                        lbDateTestP2.Text = "";
                        lbResultP2.Text = "";
                    }  
                    else if(iPLCMode[0] == 2)
                    {
                        gbProduct2.Enabled = true;
                        gbProduct1.Enabled = false;
                        lbDateTestP1.Text = "";
                        lbResultP1.Text = "";
                    }
                    else if (iPLCMode[0] == 3)
                    {
                        gbProduct2.Enabled = true;
                        gbProduct1.Enabled = true;
                    }
                    DateTime aDateTime = DateTime.Now;
                    this.txtScannerResult.Text = text;
                    List<string> Split = new List<string>();
                    Split = AnalyzeFileName(text); // phân tích ProductCode từ Scanner
                    string ProductCode = Split[0];
                    /*
                    DateTime TestedDate = new DateTime(
                                    Split.Count >= 2 ? int.Parse(Split[1]) : DateTime.MinValue.Year,
                                    Split.Count >= 3 ? int.Parse(Split[2]) : DateTime.MinValue.Month,
                                    Split.Count >= 4 ? int.Parse(Split[3]) : DateTime.MinValue.Day,
                                    Split.Count >= 5 ? int.Parse(Split[4]) : DateTime.MinValue.Hour,
                                    Split.Count >= 6 ? int.Parse(Split[5]) : DateTime.MinValue.Minute,
                                    Split.Count >= 7 ? int.Parse(Split[6]) : DateTime.MinValue.Second);
                    //Csqlite.UpdateDataBD(tableDB, ProductCode, TestedDate, aDateTime); // cập nhật đã khắc và thời gian hiện tại lên database
                    */
                    string[] Rec = Csqlite.SelecteDataDb(tableDB, ProductCode); // theo ProductCode lấy dữ liệu từ database
                    if (Rec[3] == "1")
                    {
                        MessageBox.Show("Sản phẩm đã được khắc thành công", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        iCount++;
                        if (iCount == 1 && iPLCMode[0] == 1 && iMraking[0] == 0) // đọc trạng thái cấu hình hiện tại
                        {
                            //lưu kết quả sp đã quét 1
                            PdM1.ProductCode = Rec[0];
                            PdM1.TestedResult = int.Parse(Rec[1]);
                            PdM1.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP1.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP1.Text = "Pass";
                            }
                            else
                            {
                                lbResultP1.Text = "Fail";
                            }
                            lbDateTestP1.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI1), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode1), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 1" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else if (iCount == 1 && iPLCMode[0] == 2 && iMraking[0] == 0) // quét lần 1, mode chạy 2, đã khắc xong.
                        {
                            //lưu kết quả sp đã quét 2
                            PdM2.ProductCode = Rec[0];
                            PdM2.TestedResult = int.Parse(Rec[1]);
                            PdM2.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP2.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP2.Text = "Pass";
                            }
                            else
                            {
                                lbResultP2.Text = "Fail";
                            }
                            lbDateTestP2.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI2), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode2), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 2" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else if (iCount == 1 && iPLCMode[0] == 3 && iMraking[0] == 0)
                        {
                            //lưu kết quả sp đã quét 1
                            PdM1.ProductCode = Rec[0];
                            PdM1.TestedResult = int.Parse(Rec[1]);
                            PdM1.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP1.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP1.Text = "Pass";
                            }
                            else
                            {
                                lbResultP1.Text = "Fail";
                            }
                            lbDateTestP1.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI1), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode1), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 3" + iCount.ToString());
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 0);
                        }
                        else if (iCount == 2 && iPLCMode[0] == 3 && iMraking[0] == 0)
                        {
                            //lưu kết quả sp đã quét 2
                            PdM2.ProductCode = Rec[0];
                            PdM2.TestedResult = int.Parse(Rec[1]);
                            PdM2.TestedDate = Convert.ToDateTime(Rec[2]);
                            //hiện thị và gửi dữ liệu
                            txtResultP2.Text = Rec[0].ToString();
                            if (Rec[1] == "1")
                            {
                                lbResultP2.Text = "Pass";
                            }
                            else
                            {
                                lbResultP2.Text = "Fail";
                            }
                            lbDateTestP2.Text = Rec[2].ToString();
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisEUI2), StringToAscii(Rec[4]));
                            mPLCM340.WriteMultipleRegisters(int.Parse(RegisInstallCode2), StringToAscii(Rec[5]));
                            Console.WriteLine("Mode 4" + iCount.ToString());
                            iCount = 0;
                            mPLCM340.WriteSingleRegister(int.Parse(RegisMarking), 1);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi bảo trì hệ thống, Vui lòng liên hệ đơn vị cung cấp", "LỖI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(_MBResiAvailable);
                }
            }
        } // đọc kết quả từ Scanner ở luồn khác lấy kq xử lý và hiện lên form
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
                if (_Scanner.ReadStr() != null && _Scanner.ReadStr() != txtScannerResult.Text)
                {
                    SetText(_Scanner.ReadStr());
                }
                else
                {
                    /*
                    Stopwatch swObj = new Stopwatch();
                    swObj.Start();
                    Thread.Sleep(800);
                    Application.DoEvents();
                    mPLCM340.WriteSingleRegister(int.Parse(RegisPing), 1);
                    swObj.Stop();
                    Console.WriteLine("Total:=" + swObj.ElapsedMilliseconds);
                    */
                }
            }
        } // kết quả Scanner trả về
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        } // resize ảnh (hiện tại không dùng)
        private void chbDisScanner_CheckedChanged(object sender, EventArgs e)
        {
            if (chbDisScanner.Checked == false)
                txtWriteProduct.Enabled = false;
            if (chbDisScanner.Checked == true)
                txtWriteProduct.Enabled = true;
        }
        /*------------------------------------------------------------------------------------------------------*/
        //<<<SQLite>>>
        private void btnBrowseSqliteExe_Click(object sender, EventArgs e)
        {
            if (FileDB.ShowDialog() == DialogResult.OK)
            {
                NameDb = FileDB.SafeFileName;
                //PathDB = FileDB.FileName.Remove((FileDB.FileName.LastIndexOf(NameDb)));
                txtSqliteExe.Text = FileDB.FileName;
                Csqlite.createConection(FileDB.FileName);
            }
        }
        private void btnOpenSqlite_Click(object sender, EventArgs e)
        {
            tableDB = txtSQLiteTable.Text;
            Console.WriteLine(tableDB);
            Csqlite.loadDataToGrid(dataGridView1, tableDB);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFolder.Text))
            {
                bool isValid = Directory.Exists(txtFolder.Text);
                if (isValid)
                {
                    try
                    {
                        //attemp to wriate a text file
                        using (var myFile = File.Create(txtFolder.Text.TrimEnd('\\') + "\\" + "test.txt"))
                        {
                            // interact with myFile here, it will be disposed automatically                                                        
                        }
                        File.Delete(txtFolder.Text.TrimEnd('\\') + "\\" + "test.txt");
                    }
                    catch (Exception ex)
                    {
                        isValid = false;
                        MessageBox.Show(ex.Message);
                    }
                }
                if (isValid)
                {
                    FolderHelper.Folder = txtFolder.Text.TrimEnd('\\') + "\\";
                }
            }
            else
            {
                FolderHelper.Folder = "";
            }
        }
        /*------------------------------------------------------------------------------------------------------*/
        //<<<Search>>>
        public void DoSearch()
        {
            ProductList.Clear();
            bindingSource1.DataSource = null;
            dataGridView1.DataSource = null;
            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.AppendLine(@"  
                                        SELECT *                                                 
                                        FROM Product   
                                        WHERE 1=1
                                ");
            if (!string.IsNullOrEmpty(txtProductCode.Text)) sqlQuery.AppendLine(" AND ProductCode LIKE '%" + txtProductCode.Text + "%'");
            if (!string.IsNullOrEmpty(txtTestedByUserName.Text)) sqlQuery.AppendLine(" AND TestedByUserName LIKE '%" + txtTestedByUserName.Text + "%'");
            if (!string.IsNullOrEmpty(txtTesterName.Text)) sqlQuery.AppendLine(" AND TesterName LIKE '%" + txtTesterName.Text + "%'");
            if (!string.IsNullOrEmpty(txtTypeOfTest.Text)) sqlQuery.AppendLine(" AND TypeOfTest LIKE '%" + txtTypeOfTest.Text + "%'");
            if (!string.IsNullOrEmpty(txtFixtureName.Text)) sqlQuery.AppendLine(" AND FixtureName LIKE '%" + txtFixtureName.Text + "%'");
            if (!string.IsNullOrEmpty(txtCommercialRef.Text)) sqlQuery.AppendLine(" AND CommercialReference LIKE '%" + txtCommercialRef.Text + "%'");
            if (!string.IsNullOrEmpty(txtPCBA.Text)) sqlQuery.AppendLine(" AND PCBA_PartNumber LIKE '%" + txtPCBA.Text + "%'");
            if (chkFromDateTestedDate.Checked)
            {
                sqlQuery.AppendLine(" AND TestedDate >= '" + txtFromDateTestedDate.Value.ToString("yyyy-MM-dd") + "'");
                Console.WriteLine(sqlQuery);
            }
            if (chkToDateTestedDate.Checked)
            {
                sqlQuery.AppendLine(" AND TestedDate <= '" + txtToDateTestedDate.Value.ToString("yyyy-MM-dd") + "'");
            }
            if (chkFromDateMarkedDate.Checked)
            {
                sqlQuery.AppendLine(" AND LaserMarkedDate >= '" + txtFromDateMarkedDate.Value.ToString("yyyy-MM-dd") + "'");
            }
            if (chkToDateMarkedDate.Checked)
            {
                sqlQuery.AppendLine(" AND LaserMarkedDate <= '" + txtToDateMarkedDate.Value.ToString("yyyy-MM-dd") + "'");
            }
            try
            {
                DataTable dt = Csqlite.filldb(sqlQuery.ToString());
                BindingSource bs = new BindingSource();
                bs.DataSource = dt;
                bindingNavigator1.BindingSource = bs;
                dataGridView1.DataSource = bs;
                dataGridView1.Enabled = true;
            }
            catch (Exception)
            {

            }
            TotalRecords = ProductList.Count;
            //calculate pages
            List<int> pages = new List<int>();
            int totalPage = TotalRecords / PageSize + 1;
            for (int i = 0; i < totalPage; i++)
            {
                pages.Add(i + 1);
            }
            bindingSource1.DataSource = null;
            bindingSource1.DataSource = pages;
        } // search data cần tìm trong SQLite
        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoSearch();
        }
        private void chkFromDateTestedDate_CheckedChanged(object sender, EventArgs e)
        {
            txtFromDateTestedDate.Enabled = chkFromDateTestedDate.Checked;
        }
        private void chkToDateTestedDate_CheckedChanged(object sender, EventArgs e)
        {
            txtToDateTestedDate.Enabled = chkToDateTestedDate.Checked;
        }
        private void chkFromDateMarkedDate_CheckedChanged(object sender, EventArgs e)
        {
            txtFromDateMarkedDate.Enabled = chkFromDateMarkedDate.Checked;
        }
        private void chkToDateMarkedDate_CheckedChanged(object sender, EventArgs e)
        {
            txtToDateMarkedDate.Enabled = chkToDateMarkedDate.Checked;
        }
        /*------------------------------------------------------------------------------------------------------*/
        //<<<Language>>>
        private void rdbVN_CheckedChanged(object sender, EventArgs e) // lua chon ngon ngu
        {
            if (rdbEN.Checked)
                SetLanguage("en");
            else if (rdbVN.Checked)
                SetLanguage("vi");
            else
                SetLanguage("vi");
        }
        private void SetLanguage(string cultureName)
        {
            culture = CultureInfo.CreateSpecificCulture(cultureName);
            ResourceManager rm = new
                ResourceManager("AppPlcSQLiteLaser.Translation.Caption", typeof(frMain).Assembly);
            gbSQLiteSearch.Text = rm.GetString("gbSQLiteSearch", culture);
            lbProductCode.Text = rm.GetString("lbProductCode", culture);
            lbTestedByUserName.Text = rm.GetString("lbTestedByUserName", culture);
            lbTesterName.Text = rm.GetString("lbTesterName", culture);
            lbTypeOfTest.Text = rm.GetString("lbTypeOfTest", culture);
            lbFixtureName.Text = rm.GetString("lbFixtureName", culture);
            lbCommercialRef.Text = rm.GetString("lbCommercialRef", culture);
            lbPCBAParNumber.Text = rm.GetString("lbPCBAParNumber", culture);
            lbTestedResult.Text = rm.GetString("lbTestedResult", culture);
            lbLaserMarkResult.Text = rm.GetString("lbLaserMarkResult", culture);
            btnSearch.Text = rm.GetString("btnSearch", culture);
            gbData.Text = rm.GetString("gbData", culture);
            gbSysConfig.Text = rm.GetString("gbSysConfig", culture);
            gbSqliteBrowser.Text = rm.GetString("gbSqliteBrowser", culture);
            lbDatabase.Text = rm.GetString("lbDatabase", culture);
            lbTableData.Text = rm.GetString("lbTableData", culture);
            lbSttDB.Text = rm.GetString("lbSttDB", culture);
            btnBrowseSqliteExe.Text = rm.GetString("btnBrowseSqliteExe", culture);
            btnOpenSqlite.Text = rm.GetString("btnOpenSqlite", culture);
            gbProcess.Text = rm.GetString("gbProcess", culture);
            gbProduct1.Text = rm.GetString("gbProduct1", culture);
            gbProduct2.Text = rm.GetString("gbProduct2", culture);
            gbTest.Text = rm.GetString("gbTest", culture);
            lblFolder.Text = rm.GetString("lblFolder", culture);
            lblLatestTestedProduceCode.Text = rm.GetString("lblLatestTestedProduceCode", culture);
            lblLatestTestedStatus.Text = rm.GetString("lblLatestTestedStatus", culture);
            lbPing.Text = rm.GetString("lbPing", culture);
            lbSttPLC.Text = rm.GetString("lbSttPLC", culture);
            lbMarkMode.Text = rm.GetString("lbMarkMode", culture);
            lbMarking.Text = rm.GetString("lbMarking", culture);
            lbRProductCode1.Text = rm.GetString("lbRProductCode1", culture);
            lbREUI1.Text = rm.GetString("lbREUI1", culture);
            lbRInstallCode1.Text = rm.GetString("lbRInstallCode1", culture);
            lbRProductCode2.Text = rm.GetString("lbRProductCode2", culture);
            lbREUI2.Text = rm.GetString("lbREUI2", culture);
            lbRInstallCode2.Text = rm.GetString("lbRInstallCode2", culture);
            gbFolder.Text = rm.GetString("gbFolder", culture);
            btnSave.Text = rm.GetString("btnSave", culture);
            gbScan.Text = rm.GetString("gbScan", culture);
            lbComPort.Text = rm.GetString("lbComPort", culture);
            lbScanProductCode.Text = rm.GetString("lbScanProductCode", culture);
            lbWriteProductCode.Text = rm.GetString("lbWriteProductCode", culture);
            chbDisScanner.Text = rm.GetString("chbDisScanner", culture);
            btnCheck.Text = rm.GetString("btnCheck", culture);
            btnRefresh.Text = rm.GetString("btnRefresh", culture);        
        } // set ngôn ngữ
        /*------------------------------------------------------------------------------------------------------*/
        //<<<Modbus TCP/IP>>>
        private void btnUpdateRegister_Click(object sender, EventArgs e)
        {
            if (txtRegisterPing.Text == "" || txtRegisterSttPLC.Text == "" || txtRegisterModeMark.Text == "" || txtRegisterMark.Text == "" ||txtRegisterMarkResult.Text==""||
               txtRegisterProCode1.Text == "" || txtRegisterEUI1.Text == "" || txtRegisterInstallCode1.Text == "" ||
               txtRegisterProCode2.Text == "" || txtRegisterEUI2.Text == "" || txtRegisterInstallCode2.Text == "")
            {
                MessageBox.Show("Chưa điền thông tin thanh ghi Modbus", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                _MBResiAvailable = false;
            }
            else
            {
                RegisPing = txtRegisterPing.Text;           //_Conten[1].Substring(5);
                RegisSttPLC = txtRegisterSttPLC.Text;         //_Conten[2].Substring(7);
                RegisModeMark = txtRegisterModeMark.Text;       //_Conten[3].Substring(9);
                RegisMarking = txtRegisterModeMark.Text;            //_Conten[4].Substring(13);
                RegisMarkingResult = txtRegisterMarkResult.Text;       //_Conten[5].Substring(13);
                RegisProductCode1 = txtRegisterProCode1.Text;       //_Conten[6].Substring(13);
                RegisEUI1 = txtRegisterEUI1.Text;           //_Conten[7].Substring(5);
                RegisInstallCode1 = txtRegisterInstallCode1.Text;   //_Conten[8].Substring(13);
                RegisProductCode2 = txtRegisterProCode2.Text;       //_Conten[9].Substring(13);
                RegisEUI2 = txtRegisterEUI2.Text;           //_Conten[10].Substring(5);
                RegisInstallCode2 = txtRegisterInstallCode2.Text;   //_Conten[11].Substring(13);
                string[] _Conten = new string[12];
                _Conten[0] = "Ip=" + Ip;
                _Conten[1] = "Ping=" + RegisPing;
                _Conten[2] = "SttPLC=" + RegisSttPLC;
                _Conten[3] = "ModeMark=" + RegisModeMark;
                _Conten[4] = "Marking=" + RegisModeMark;
                _Conten[5] = "MarkResult=" + RegisMarkingResult;
                _Conten[6] = "ProductCode1=" + RegisProductCode1;
                _Conten[7] = "EUI1=" + RegisEUI1;
                _Conten[8] = "InstallCode1=" + RegisInstallCode1;
                _Conten[9] = "ProductCode2=" + RegisProductCode2;
                _Conten[10] = "EUI2=" + RegisEUI2;
                _Conten[11] = "InstallCode2=" + RegisInstallCode2;
                MessageBox.Show("Điền thành công thanh ghi Modbus", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                File.WriteAllLines(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "/config.ini", _Conten);
                _MBResiAvailable = true;
            }
        }
        public void ReadSysConfig()
        {
            string[] _Conten;
            string directoryPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                MessageBox.Show("Không tìm Thấy file Config.ini", "Cảnh báo", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else
            {
                _Conten = File.ReadAllLines(directoryPath + "/config.ini");
                // Read Register IP
                if (_Conten[0].StartsWith("Ip="))
                {
                    Ip = _Conten[0].Substring(3);
                    Console.WriteLine(Ip);
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin IP", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register Ping PLC to APP
                if (_Conten[1].StartsWith("Ping="))
                {
                    RegisPing = _Conten[1].Substring(5);
                    txtRegisterPing.Text = RegisPing;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register STT PLC
                if (_Conten[2].StartsWith("SttPLC="))
                {
                    RegisSttPLC = _Conten[2].Substring(7);
                    txtRegisterSttPLC.Text = RegisSttPLC;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register Mode Marking
                if (_Conten[3].StartsWith("ModeMark="))
                {
                    RegisModeMark = _Conten[3].Substring(9);
                    txtRegisterModeMark.Text = RegisModeMark;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register Marking
                if (_Conten[4].StartsWith("Marking="))
                {
                    RegisMarking = _Conten[4].Substring(8);
                    txtRegisterMark.Text = RegisMarking;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register ProductCode1
                if (_Conten[5].StartsWith("MarkResult="))
                {
                    RegisMarkingResult = _Conten[5].Substring(11);
                    txtRegisterMarkResult.Text = RegisMarkingResult;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register ProductCode1
                if (_Conten[6].StartsWith("ProductCode1="))
                {
                    RegisProductCode1 = _Conten[6].Substring(13);
                    txtRegisterProCode1.Text = RegisProductCode1;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register EUI1
                if (_Conten[7].StartsWith("EUI1="))
                {
                    RegisEUI1 = _Conten[7].Substring(5);
                    txtRegisterEUI1.Text = RegisEUI1;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register InstallCode
                if (_Conten[8].StartsWith("InstallCode1="))
                {
                    RegisInstallCode1 = _Conten[8].Substring(13);
                    txtRegisterInstallCode1.Text = RegisInstallCode1;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register ProductCode2
                if (_Conten[9].StartsWith("ProductCode2="))
                {
                    RegisProductCode2 = _Conten[9].Substring(13);
                    txtRegisterProCode2.Text = RegisProductCode2;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register EUI1
                if (_Conten[10].StartsWith("EUI2="))
                {
                    RegisEUI2 = _Conten[10].Substring(5);
                    txtRegisterEUI2.Text = RegisEUI2;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Read Register InstallCode
                if (_Conten[11].StartsWith("InstallCode2="))
                {
                    RegisInstallCode2 = _Conten[11].Substring(13);
                    txtRegisterInstallCode2.Text = RegisInstallCode2;
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                }
            }
        } // đọc thành ghi modbus từ file cấu hình Config.ini
        private void checkRegister()
        {
            if (txtRegisterPing.Text == "" || txtRegisterSttPLC.Text == "" || txtRegisterModeMark.Text == "" || txtRegisterMark.Text == "" ||
                txtRegisterProCode1.Text == "" || txtRegisterEUI1.Text == "" || txtRegisterInstallCode1.Text == "" ||
                txtRegisterProCode2.Text == "" || txtRegisterEUI2.Text == "" || txtRegisterInstallCode2.Text == "")
            {
                MessageBox.Show("Chưa điền thông tin thanh ghi modbus", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _MBResiAvailable = false;
            }
            else
            {
                _MBResiAvailable = true;
            }
        } // kiểm tra đã cấu hình thành ghi modbus
        public static int[] StringToAscii(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            return value.Select(System.Convert.ToInt32).ToArray();
        } //  chuyển dổi String sang mãng Ascii Int[]
        /*------------------------------------------------------------------------------------------------------*/
        //<<<Sub Function system>>>
        public List<string> AnalyzeFileName(string fileName)
        {
            List<string> result = new List<string>();
            string[] arrPart = fileName.Split('_');

            //first part is ProductCode
            if (arrPart.Length >= 1)
            {
                result.Add(arrPart[0]);
            }
            //second part is date
            if (arrPart.Length >= 2)
            {
                string sDate = arrPart[1];
                if (sDate.Length >= 4)
                {
                    result.Add(sDate.Substring(0, 4));
                    sDate = sDate.Substring(4);
                }
                if (sDate.Length >= 2)
                {
                    result.Add(sDate.Substring(0, 2));
                    sDate = sDate.Substring(2);
                }
                if (sDate.Length >= 2)
                {
                    result.Add(sDate.Substring(0, 2));
                    sDate = sDate.Substring(2);
                }
            }
            //third part is time
            if (arrPart.Length >= 3)
            {
                string sTime = arrPart[2];
                if (sTime.Length >= 2)
                {
                    result.Add(sTime.Substring(0, 2));
                    sTime = sTime.Substring(2);
                }
                if (sTime.Length >= 2)
                {
                    result.Add(sTime.Substring(0, 2));
                    sTime = sTime.Substring(2);
                }
                if (sTime.Length >= 2)
                {
                    result.Add(sTime.Substring(0, 2));
                    sTime = sTime.Substring(2);
                }
            }
            return result;
        } // Phân tích tên file text đê lấy ProductId và ngày/tháng/năm
        private string[] EliminateEmptyLine(string[] lines)
        {
            List<string> result = new List<string>();
            foreach (var line in lines)
            {
                if (line != null && !string.IsNullOrEmpty(line.Replace("\n", "").Replace("\r", "")))
                {
                    result.Add(line.Replace("\n", "").Replace("\r", ""));
                }
            }
            return result.ToArray();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetListComPort();
        }
    }
}