using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class StaffFolder
    {
        public StaffFolder()
        {
            
        }
        public StaffFolder(string foldername)
        {
            FolderName = foldername;
        }
        public string FolderName { get; set; }
        public IEnumerable<StaffFile> Files { get; set; }
    }

    
    public class StaffFile
    {
        public StaffFile()
        {
         
        }
        public StaffFile(string filename)
        {
            FileName = filename;
        }
        public string FileName { get; set; }
        public string FolderName { get; set; }
    }
}