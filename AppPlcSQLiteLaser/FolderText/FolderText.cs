using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AppPlcSQLiteLaser
{
    class FolderText
    {
        public void ReadSysConfig()
        {
            string[] _Conten;
            string directoryPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            //directoryPath += "/config.txt";
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
            {
                MessageBox.Show("Không tìm Thấy file Config.txt", "Cảnh báo", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else
            {
                _Conten = File.ReadAllLines(directoryPath+ "/config.txt");
                if(_Conten[0].StartsWith("Ip="))
                {
                    string Ip = _Conten[0].Substring(3);
                    Console.WriteLine(Ip);
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin IP", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (_Conten[1].StartsWith("Ping="))
                {
                    string RegisPing = _Conten[1].Substring(5);
                    Console.WriteLine(RegisPing);
                }
                else
                {
                    MessageBox.Show("Chư điền thông tin thanh ghi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
