using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppPlcSQLiteLaser
{
    class FolderHelper
    {
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
            public string TestedResult;
            public DateTime TestedDate;
	        public int LaserMarkedResult;
            public DateTime LaserMarkedDate;
	        public string EUI64;
            public string InstallCode;
            public string EUI64WithSpace;
            public string InstallCodeWithSpace;
        }
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
