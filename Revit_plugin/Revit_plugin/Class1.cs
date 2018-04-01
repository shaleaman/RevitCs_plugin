using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

[TransactionAttribute(TransactionMode.Manual)]
[RegenerationAttribute(RegenerationOption.Manual)]
public class JA_PLUGIN : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        //Get application and document objects
        UIApplication uiApp = commandData.Application;
        Document doc = uiApp.ActiveUIDocument.Document;

        try
        {


        //Define a Reference object to accept the pick result
        Reference pickedRef = null;

        //Pick a group
        Selection sel = uiApp.ActiveUIDocument.Selection;
        GroupPickFilter selFilter = new GroupPickFilter();
        pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Please select a group.");
        Element elem = doc.GetElement(pickedRef);
        Group group = elem as Group;

        //Pick a point
        XYZ point = sel.PickPoint("PLease pick a point to place group");

        //Place the group
        Transaction trans = new Transaction(doc);
        trans.Start("Lab");
        doc.Create.PlaceGroup(point, group.GroupType);
        trans.Commit();

        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }
        //Catch other errors
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
        return Result.Succeeded;
    }
}

public class GroupPickFilter : ISelectionFilter
{
    public bool AllowElement(Element e)
    {
        return (e.Category.Id.IntegerValue.Equals(
            (int)BuiltInCategory.OST_IOSModelGroups));
    }
    public bool AllowReference(Reference r, XYZ p)
    {
        return false;
    }
}