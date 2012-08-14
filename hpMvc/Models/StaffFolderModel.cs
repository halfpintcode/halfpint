using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Telerik.Web.Mvc.UI;

namespace hpMvc.Models
{
    public static class DynamicFolderFile
    {
        public static List<TreeViewItemModel> GetFileFolderModel(string path, string topFolder)
        {
            var list = new List<TreeViewItemModel>();
            var tvParent = new TreeViewItemModel();
            tvParent.Text = topFolder;
            tvParent.ImageUrl = "~/Content/Images/folder.png";

            list.Add(tvParent);
            foreach (string f in Directory.GetFiles(path))
            {
                var tvChild = new TreeViewItemModel();
                tvChild.Text = Path.GetFileName(f);
                tvChild.ImageUrl = "~/Content/Images/file.png"; 
                tvChild.NavigateUrl = "~/Download/Getfile/" + tvChild.Text + "/" + topFolder;
                tvParent.Items.Add(tvChild);
            }

            GetFolderFiles(path, topFolder, tvParent);
            return list;
        }

        public static void GetFolderFiles(string path, string topFolder, TreeViewItemModel parentTvi)
        {
                        
            foreach (string d in Directory.GetDirectories(path))
            {
                string fileFolder = topFolder;
            
                var tvFolder = new TreeViewItemModel();
                tvFolder.ImageUrl = "~/Content/Images/folder.png";
                tvFolder.Text = Path.GetFileName(d);
                parentTvi.Items.Add(tvFolder);

                string[] parts = d.Split(new string[] { "\\" }, StringSplitOptions.None);
                int iStart = 100;
                for (int i = 3; i < parts.Length; i++)
                {
                    if (parts[i] == topFolder)
                    {
                        iStart = i;
                    }
                    if (i > iStart)
                        fileFolder += "~" + parts[i];
                }
                                
                foreach (string f in Directory.GetFiles(d))
                {
                    var tvChild = new TreeViewItemModel();
                    tvChild.Text = Path.GetFileName(f);
                    tvChild.ImageUrl = "~/Content/Images/file.png";
                    tvChild.NavigateUrl = "~/Download/Getfile/" + tvChild.Text + "/" + fileFolder;
                    tvFolder.Items.Add(tvChild);
                } 

                GetFolderFiles(d, topFolder, tvFolder);


            }
            
            

            

            
            
        }
    }

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