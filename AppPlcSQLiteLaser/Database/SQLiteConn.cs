using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace AppPlcSQLiteLaser
{
    class SQLiteConn
    {
        SQLiteConnection SQLiteCon = new SQLiteConnection();
        public void createConection(string sPath) // tao ket noi den sqlite
        {
            string _strConnect = @"Data Source=" + sPath + ";Version =3;";
            SQLiteCon.ConnectionString = _strConnect;
        }

        public DataTable filldb(string sQuey)
        {
            DataTable dt = new DataTable();
            SQLiteCon.Open();
            SQLiteDataAdapter sda = new SQLiteDataAdapter(sQuey, SQLiteCon);
            sda.Fill(dt);
            if (SQLiteCon.State == ConnectionState.Open) SQLiteCon.Close();
            return dt;
        }
        private DataSet loadData(string Table) // lay du lieu lu sqlite
        {
            DataSet ds = new DataSet();
            SQLiteCon.Open();
            SQLiteDataAdapter da = new SQLiteDataAdapter("select * from " + Table, SQLiteCon);
            da.Fill(ds);
            SQLiteCon.Close();
            return ds;
        }
        public void loadDataToGrid(System.Windows.Forms.DataGridView DataView, string Table) // dua du lieu da lay tu sqlite vao data view
        {
            DataSet ds = loadData(Table);
            DataView.DataSource = ds.Tables;
        }
        public void DeleteDataBD(string Table, string _ProductCode) // Delete dữu liệu trong databasae theo Id
        {
            SQLiteCon.Open();
            string strDelete = string.Format("DELETE FROM " + Table + " where ProductCode='{0}'", _ProductCode);
            SQLiteCommand cmd = new SQLiteCommand(strDelete, SQLiteCon);
            cmd.ExecuteNonQuery();
            SQLiteCon.Close();
        }

        public void InsertDataBD(string Table, string ProductCode, string TestedByUserName, string TesterName, string TypeOfTest, string FixtureName, string PCBA_PartNumber, string CommercialReference, string CavityLaserMark, int TestedResult, string TestedDate, int LaserMarkedResult, string LaserMarkedDate, string EUI64, string InstallCode, string EUI64WithSpace, string InstallCodeWithSpace) // Insert dữu liệu vào databasae
        {
            SQLiteCon.Open();
            string strInsert = string.Format("INSERT INTO " + Table + "(ProductCode,TestedByUserName,TesterName,TypeOfTest,FixtureName,PCBA_PartNumber,CommercialReference,CavityLaserMark,TestedResult,TestedDate,LaserMarkedResult,LaserMarkedDate,EUI64,InstallCode,EUI64WithSpace,InstallCodeWithSpace)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')",ProductCode, TestedByUserName, TesterName, TypeOfTest, FixtureName, PCBA_PartNumber, CommercialReference, CavityLaserMark, TestedResult.ToString(), TestedDate.ToString(), LaserMarkedResult.ToString(), LaserMarkedDate.ToString(), EUI64, InstallCode, EUI64WithSpace, InstallCodeWithSpace);
            SQLiteCommand cmd = new SQLiteCommand(strInsert, SQLiteCon);
            cmd.ExecuteNonQuery();
            SQLiteCon.Close();
        }
        public void UpdateDataBD(string Table, string ProductCode, string TestedDate,string LaserMarkedDate,string CavityLaserMark)
        {
            SQLiteCon.Open();
            string strUpdate = string.Format("UPDATE " + Table +
                " set LaserMarkedResult='{2}', LaserMarkedDate='{3}', CavityLaserMark='{4}' where ProductCode='{0}'and TestedDate='{1}'", ProductCode, TestedDate, "1", LaserMarkedDate, CavityLaserMark);
            SQLiteCommand cmd = new SQLiteCommand(strUpdate, SQLiteCon);
            cmd.ExecuteNonQuery();
            SQLiteCon.Close();
        }
        public string[] SelecteDataDb(string Table, string _ProductCode)
        {
            //string text="";
            string[] RequestDb = new string[8];
            SQLiteCon.Open();
            //Truy vấn
            string sql = "SELECT ProductCode,TestedResult,TestedDate,LaserMarkedResult,EUI64,InstallCode,EUI64WithSpace,InstallCodeWithSpace FROM  " + Table+ " where ProductCode=" +"'"+ _ProductCode+"'";
            SQLiteCommand command = new SQLiteCommand(sql, SQLiteCon);
            SQLiteDataReader reader = command.ExecuteReader();

            //Đọc dữ liệu
            while (reader.Read())
            {
                string strProductCode = string.Format("{0}", reader["ProductCode"]);
                string strTestedResult = string.Format("{0}", reader["TestedResult"]);
                string strTestedDate = string.Format("{0}", reader["TestedDate"]);
                string strLaserMarkedResult = string.Format("{0}", reader["LaserMarkedResult"]);
                string strEUI64 = string.Format("{0}", reader["EUI64"]);
                string strIntallCode = string.Format("{0}", reader["InstallCode"]);
                string strEUI64WhiteSpace = string.Format("{0}", reader["EUI64WithSpace"]);
                string strIntallCodeWhiteSpace = string.Format("{0}", reader["InstallCodeWithSpace"]);
                //text = strProductCode + "/" + strLaserMarkedResult + "/" + strLaserMarkedDate + "/" + strEUI64 + "/" + strIntallCode + "/" + strEUI64WhiteSpace + "/"+ strIntallCodeWhiteSpace;
                RequestDb[0]= strProductCode;
                RequestDb[1] = strTestedResult;
                RequestDb[2] = strTestedDate;
                RequestDb[3] = strLaserMarkedResult;
                RequestDb[4] = strEUI64;
                RequestDb[5] = strIntallCode;;
                RequestDb[6] = strEUI64WhiteSpace;
                RequestDb[7] = strIntallCodeWhiteSpace;
                SQLiteCon.Close();
                break;
            }
            return RequestDb;
        }
        public string SelectDataDBNew(string Table)
        {
            string NoMax="";
            string sql = "";
            string text="";
            SQLiteCon.Open();
            string strSelectMax = "SELECT MAX(No) FROM " + Table;
            SQLiteCommand command = new SQLiteCommand(strSelectMax, SQLiteCon);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                NoMax = string.Format("{0}", reader[0]);
                sql = "SELECT * FROM " + Table + " where No=" + NoMax;
                break;
            }
            if (sql != "")
            {
                SQLiteCommand command1 = new SQLiteCommand(sql, SQLiteCon);
                SQLiteDataReader reader1 = command1.ExecuteReader();
                while (reader1.Read())
                {
                    string strId = string.Format("{0}", reader1[0]);//No
                    string strName = string.Format("{0}", reader1[1]);//Id
                    string strDate = string.Format("{0}", reader1[2]);//Name
                    string strTime = string.Format("{0}", reader1[3]);//Date
                    string strResult = string.Format("{0}", reader1[4]);//Time reader[5] result
                    text = strId + "/" + strName + "/" + strDate + "/" + strTime + "/" + strResult;
                    SQLiteCon.Close();
                    break;
                }
            }
            return text;
        }
    }
}
