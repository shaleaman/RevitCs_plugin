using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using Autodesk.Revit;
using System.IO;

[TransactionAttribute(TransactionMode.Manual)]
[RegenerationAttribute(RegenerationOption.Manual)]
public class Backup_File : IExternalCommand
{
//    public void ReadAllFiles(string directory)
//    {
//        var fileNames = Directory.GetFiles(directory);
//        List<string> datedNames = new List<string>;

//    }
//    public string ReadDate(string filename)
//        {
//        if (filename.Length > 8)
//            {
//                var dateTest = filename.Substring(0,8);
//            }
//        if (int.TryParse(dateTest, out fileDate))
//            {
//               return DateTime.ParseExact(dateStub, "yyyyMMdd", null);
//            }
//        else { return DateTime.Today;}
//    }       

    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {

        Autodesk.Revit.UI.UIApplication m_app;
        m_app = commandData.Application;
        Autodesk.Revit.ApplicationServices.Application app = m_app.Application;

        // Check worksharing mode of each document
        // Open Revit projects
        OpenFileDialog theDialogRevit = new OpenFileDialog();
        theDialogRevit.Title = "Select Revit Project Files";
        theDialogRevit.Filter = "RVT files|*.rvt";
        theDialogRevit.FilterIndex = 1;
        theDialogRevit.InitialDirectory = @"D:\";
        theDialogRevit.Multiselect = true;

        if (theDialogRevit.ShowDialog() == DialogResult.OK)
        {
            DateTime todaysDate = DateTime.Today;
            string dateStamp = string.Format("{0}", todaysDate.ToString("yyyyMMdd"));
            string mpath = "";
            string mpathOnlyFilename = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            string currentFolder = Path.GetDirectoryName(theDialogRevit.FileName);
            

            folderBrowserDialog1.Description = "Select Folder Where Revit Projects to be Saved in Local";
            //folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowserDialog1.SelectedPath = currentFolder;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                mpath = folderBrowserDialog1.SelectedPath;
                foreach (String projectPath in theDialogRevit.FileNames)
                {
                    // convert input string to directory info. 
                    FileInfo filePath = new FileInfo(projectPath);
                    ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath.FullName);

                    OpenOptions opt = new OpenOptions();
                    opt.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                    mpathOnlyFilename = string.Format("{0}_{1}", dateStamp, filePath.Name);
                    Document openedDoc = app.OpenDocumentFile(mp, opt);
                    SaveAsOptions options = new SaveAsOptions();
                    WorksharingSaveAsOptions wsOptions = new WorksharingSaveAsOptions();
                    options.OverwriteExistingFile = true;
                    wsOptions.SaveAsCentral = true;
                    options.SetWorksharingOptions(wsOptions);
                    ModelPath modelPathout = ModelPathUtils.ConvertUserVisiblePathToModelPath(mpath + "\\" + mpathOnlyFilename);
                    openedDoc.SaveAs(modelPathout, options);
                    openedDoc.Close(false);
                 
                }
            }

            // Scan backup folder and retrieve date prefixes
            //List<string> filesToDelete =  ReadAllFiles(mpath);
        }
        return Result.Succeeded;
    }
}