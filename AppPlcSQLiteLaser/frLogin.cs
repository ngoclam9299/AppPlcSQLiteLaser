using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.IO;

namespace AppPlcSQLiteLaser
{
    public partial class frLogin : Form
    {
        frMain frMain = new frMain();
        private bool _bServerAvailable;
        string Ip;
        public frLogin()
        {
            InitializeComponent();
            this.FormClosing += frLogin_FormClosing;
        }
        private void frLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if(chbSaveLogin.Checked == true)
               RegistryHelper.SaveAllSettings(Application.ProductName, this);
            */
        }
        private void frLogin_Load(object sender, EventArgs e)
        {
            
            //RegistryHelper.LoadAllSettings(Application.ProductName, this);
            this.Size = new System.Drawing.Size(422, 197);
            ReadSysConfig();
            PingServerMd();
        }
        private void btnLogon_Click(object sender, EventArgs e)
        {
            string User = AppPlcSQLiteLaser.Properties.Settings.Default.User;
            string Pass = AppPlcSQLiteLaser.Properties.Settings.Default.Pass;
            if (txtUserName.Text == User & txtPassword.Text == Pass)
            {
                if (_bServerAvailable == true)
                {
                    frMain.Show();
                    frMain.Size = new System.Drawing.Size(861, 535);
                    this.Hide();
                }
                else
                    MessageBox.Show("méo kết nối được đâu bro", "Kết nối", MessageBoxButtons.RetryCancel);
            }

            else if (txtUserName.Text == "" || txtPassword.Text == "")
                MessageBox.Show("Chu Dien Ten Tai Khoang Hoac Mat Khau", "Thong Bao", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            else
            {
                MessageBox.Show("Ten Tai Khoang Hoac Mat Khau Sai", "Thong Bao", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                txtPassword.Text = "";
                txtUserName.Text = "";
            }
        }
        private void chbShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if(chbShowPass.Checked == true)
            {
                txtPassword.UseSystemPasswordChar = false;
            }
            else
                txtPassword.UseSystemPasswordChar = true;
        }
        private async void PingServerMd()
        {
            _bServerAvailable = false;
            while (_bServerAvailable == false)
            {
                Ping pingSender = new Ping();
                string host = Ip;
                await Task.Run(() => {
                    PingReply reply = pingSender.Send(host);
                    if (reply.Status == IPStatus.Success)
                    {
                        _bServerAvailable = true;  
                    }
                    else
                    {
                        Console.WriteLine("Address: {0}", reply.Status);
                    }
                });
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
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin IP", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        } // đọc thành ghi modbus từ file cấu hình Config.ini

    }
}
