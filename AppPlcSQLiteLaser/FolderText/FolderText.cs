using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace AppPlcSQLiteLaser
{
    class FolderText
    {
        DateTime DateTimeNow = DateTime.Now; // lay thoi gian hien tai cua may tinh dang chay
        public void CreNewFolder(string sPath,string tConten)
        {
            string[] _Id = tConten.Split('/');
            // 1. Đường dẫn tới thư mục cần tạo New Directory
            string directoryPath = sPath;
            directoryPath += "/" + DateTimeNow.Year;
            directoryPath += "/" + DateTimeNow.Month;
            directoryPath += "/" + DateTimeNow.Day;
            //directoryPath += "/" + _Id[0] + ".txt";

            // 2.Khai báo một thể hiện của lớp DirectoryInfo
            DirectoryInfo directory = new DirectoryInfo(directoryPath);

            // Kiểm tra thư mục chưa tồn tại mới sử dụng phương thức tạo
            if (!directory.Exists)
            {
                // 3.Sử dụng phương thức Create để tạo thư mục.
                directory.Create();
                File.WriteAllText(directoryPath + "/" + _Id[0] + ".txt", _Id[0]);
                File.WriteAllLines(directoryPath + "/" + _Id[0] + ".txt", _Id);
            }
            else
                File.WriteAllText(directoryPath + "/" + _Id[0] + ".txt", _Id[0]);
                File.WriteAllLines(directoryPath + "/" + _Id[0] + ".txt", _Id);

        }
    }
}
