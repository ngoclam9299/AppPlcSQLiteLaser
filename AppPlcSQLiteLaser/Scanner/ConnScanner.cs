using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.DataMan.SDK;
using System;
using System.Windows.Forms;
using System.Drawing;



namespace AppPlcSQLiteLaser
{
    class ConnScanner
    {
        
        SerSystemConnector myConn;
        DataManSystem _system;
        string str;
        Image img;
        
        public void ScannerConn(string Port)
        {
            try
            {
                myConn = new SerSystemConnector(Port);
                _system = new DataManSystem(myConn);
                ResultTypes requested_result_types = ResultTypes.ReadString | ResultTypes.Image | ResultTypes.ImageGraphics;
                _system.ReadStringArrived += _system_ReadStringArrived;
                _system.ImageArrived += _system_ImageArrived;
                _system.Connect();
                _system.SetResultTypes(requested_result_types);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Disconnect()
        {
            if (_system != null)
                _system.Disconnect();
        }
        public void Connect()
        {
            _system.Connect();
        }
        public void Triggrt_on()
        {
            if (_system != null)
                _system.SendCommand("TRIGGER ON");
        }
        public void Triggrt_off()
        {
            if (_system != null)
                _system.SendCommand("TRIGGER OFF");
        }

        private void _system_ReadStringArrived(object sender, ReadStringArrivedEventArgs e)
        {
            str = e.ReadString;

        }
        public string ReadStr()
        {
            return str;
        }
        private void _system_ImageArrived(object sender, ImageArrivedEventArgs e)
        {
           img = e.Image;
        }
        public Image ReadImg()
        {
            return img;

        }

    }
}
