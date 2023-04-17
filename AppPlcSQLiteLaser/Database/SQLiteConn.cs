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
        SQLiteConnection _con = new SQLiteConnection();
        public void createConection(string sPath) // tao ket noi den sqlite
        {
            string _strConnect = @"Data Source=" + sPath + ";Version =3;";
            _con.ConnectionString = _strConnect;
        }
        private DataSet loadData(string Table) // lay du lieu lu sqlite
        {
            DataSet ds = new DataSet();
            _con.Open();
            SQLiteDataAdapter da = new SQLiteDataAdapter("select * from " + Table + "", _con);
            da.Fill(ds);
            _con.Close();
            return ds;
        }
        public void loadDataToGrid(System.Windows.Forms.DataGridView DataView, string Table) // dua du lieu da lay tu sqlite vao data view
        {
            DataSet ds = loadData(Table);
            DataView.DataSource = ds.Tables[0];
        }
        public void DeleteDataBD(string Table, string _ProductCode) // Delete dữu liệu trong databasae theo Id
        {
            _con.Open();
            string strDelete = string.Format("DELETE FROM " + Table + " where ProductCode='{0}'", _ProductCode);
            SQLiteCommand cmd = new SQLiteCommand(strDelete, _con);
            cmd.ExecuteNonQuery();
            _con.Close();
        }
        public void InsertDataBD(string Table, string _Id, string _Name, string _Date, string _Time,string _Result) // Insert dữu liệu vào databasae
        {
            _con.Open();
            string strInsert = string.Format("INSERT INTO " + Table + "(ID,Name,Date,Time,Result)VALUES('{0}','{1}','{2}','{3}','{4}')", _Id, _Name, _Date, _Time,_Result);
            SQLiteCommand cmd = new SQLiteCommand(strInsert, _con);
            cmd.ExecuteNonQuery();
            _con.Close();
        }
        public void UpdateDataBD(string Table, string _Id, string _Name, string _Date, string _Time)
        {
            _con.Open();
            string strUpdate = string.Format("UPDATE " + Table +
                " set Name='{1}', Date='{2}', Time='{3}' where id='{0}'", _Id, _Name, _Date, _Time);
            SQLiteCommand cmd = new SQLiteCommand(strUpdate, _con);
            cmd.ExecuteNonQuery();
            _con.Close();
        }
        public string SelecteDataDb(string Table, string _Id)
        {
            string text="";
            _con.Open();
            //Truy vấn
            string sql = "SELECT * FROM "+Table+" where ID="+_Id;
            SQLiteCommand command = new SQLiteCommand(sql, _con);
            SQLiteDataReader reader = command.ExecuteReader();

            //Đọc dữ liệu
            string r = "";
            while (reader.Read())
            {
                string strId = string.Format("{0}", reader[0]);
                string strName = string.Format("{0}", reader[1]);
                string strDate = string.Format("{0}", reader[2]);
                string strTime = string.Format("{0}", reader[3]);
                string strResult = string.Format("{0}", reader[4]);
                text = strId + "/" + strName + "/" + strDate + "/" + strTime + "/"+ strResult;
                _con.Close();
                break;
            }
            MessageBox.Show(text);
            return text;
        }
        public string SelectDataDBNew(string Table)
        {
            string NoMax="";
            string sql = "";
            string text="";
            _con.Open();
            string strSelectMax = "SELECT MAX(No) FROM " + Table;
            SQLiteCommand command = new SQLiteCommand(strSelectMax, _con);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                NoMax = string.Format("{0}", reader[0]);
                sql = "SELECT * FROM " + Table + " where No=" + NoMax;
                break;
            }
            if (sql != "")
            {
                SQLiteCommand command1 = new SQLiteCommand(sql, _con);
                SQLiteDataReader reader1 = command1.ExecuteReader();
                while (reader1.Read())
                {
                    string strId = string.Format("{0}", reader1[0]);//No
                    string strName = string.Format("{0}", reader1[1]);//Id
                    string strDate = string.Format("{0}", reader1[2]);//Name
                    string strTime = string.Format("{0}", reader1[3]);//Date
                    string strResult = string.Format("{0}", reader1[4]);//Time reader[5] result
                    text = strId + "/" + strName + "/" + strDate + "/" + strTime + "/" + strResult;
                    _con.Close();
                    break;
                }
            }
            return text;
        }
    }
    
}
