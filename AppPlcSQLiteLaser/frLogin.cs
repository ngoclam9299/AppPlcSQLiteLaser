using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace AppPlcSQLiteLaser
{
    public partial class frLogin : Form
    {
        frMain frMain = new frMain();
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
        private void btnLogon_Click(object sender, EventArgs e)
        {
            string User = AppPlcSQLiteLaser.Properties.Settings.Default.User;
            string Pass = AppPlcSQLiteLaser.Properties.Settings.Default.Pass;
            if (txtUserName.Text == User & txtPassword.Text == Pass)
            {
                //this.Close();
                frMain.Show();
                frMain.Size = new System.Drawing.Size(833, 489);
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

        private void frLogin_Load(object sender, EventArgs e)
        {
            //RegistryHelper.LoadAllSettings(Application.ProductName, this);
            this.Size = new System.Drawing.Size(422, 197);
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
    }
}
