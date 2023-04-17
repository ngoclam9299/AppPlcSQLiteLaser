using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppPlcSQLiteLaser
{
    class FolderHelper
    {
        public static string Folder
        {
            get
            {
                return Properties.Settings.Default.Folder;
            }
            set
            {
                Properties.Settings.Default.Folder = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
