using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using System.Windows.Forms;
using Autodesk.Revit;
using System.IO;

public class JA_PLUGIN
{
    public void Execute(
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
            string mpath = "";
            string mpathOnlyFilename = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

            folderBrowserDialog1.Description = "Select Folder Where Revit Projects to be Saved in Local";
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                mpath = folderBrowserDialog1.SelectedPath;
                foreach (String projectPath in theDialogRevit.FileNames)
                {
                    FileInfo filePath = new FileInfo(projectPath);
                    ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath.FullName);
                    OpenOptions opt = new OpenOptions();
                    opt.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                    mpathOnlyFilename = filePath.Name;
                    Document openedDoc = app.OpenDocumentFile(mp, opt);
                    SaveAsOptions options = new SaveAsOptions();
                    options.OverwriteExistingFile = true;
                    ModelPath modelPathout = ModelPathUtils.ConvertUserVisiblePathToModelPath(mpath + "\\" + mpathOnlyFilename);
                    openedDoc.SaveAs(modelPathout, options);
                    openedDoc.Close(false);
                }
            }
        }
    }
}