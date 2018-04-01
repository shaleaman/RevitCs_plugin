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
public class CopyGroupToRoom : IExternalCommand
{
    public XYZ GetElementCenter(Element elem)
    {
        BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
        XYZ center = (bounding.Max + bounding.Min) * 0.5;
        return center;
    }

    Room GetRoomOfGroup(Document doc, XYZ point)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfCategory(BuiltInCategory.OST_Rooms);
        Room room = null;
        foreach (Element elem in collector)
        {
            room = elem as Room;
            if (room != null)
            {
                //Decide if this point is in the picked room
                if (room.IsPointInRoom(point))
                {
                    break;
                }
            }
        }
        return room;
    }

    public XYZ GetRoomCenter(Room room)
    {
        // Get the room center point
        XYZ boundCenter = GetElementCenter(room);
        LocationPoint locPt = (LocationPoint)room.Location;
        XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
        return roomCenter;
    }

    public void PlaceFunitureInRooms(
       Document doc,
       IList<Reference> rooms,
       XYZ sourceCenter,
       GroupType gt,
       XYZ groupOrigin)
    {
        XYZ offset = groupOrigin - sourceCenter;
        XYZ offsetXY = new XYZ(offset.X, offset.Y, 0);

        foreach (Reference r in rooms)
        {
            Room roomTarget = doc.GetElement(r) as Room;
            if (roomTarget != null)
            {
                XYZ roomCenter = GetRoomCenter(roomTarget);
                Group group = doc.Create.PlaceGroup(roomCenter + offsetXY, gt);
            }
        }
    }

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

            //Get the groups center point
            XYZ origin = GetElementCenter(group);

            //Get the room that the picked group is located in
            Room room = GetRoomOfGroup(doc, origin);

            // Get the room's center point
            XYZ sourceCenter = GetRoomCenter(room);

            //Ask the user to pick target rooms
            RoomPickFilter roomPickFilter = new RoomPickFilter();
            IList<Reference> rooms =
                sel.PickObjects(ObjectType.Element, roomPickFilter, "Select target rooms for duplicate furniture group");

            //Pick a point
            //XYZ point = sel.PickPoint("PLease pick a point to place group");

            //Place the group
            Transaction trans = new Transaction(doc);
            trans.Start("CopyGroupToRoom");
            //doc.Create.PlaceGroup(point, group.GroupType);

            //Calculate the new group's position
            PlaceFunitureInRooms(doc, rooms, sourceCenter, group.GroupType, origin);

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

public class RoomPickFilter : ISelectionFilter
{
    public bool AllowElement(Element e)
    {
        return (e.Category.Id.IntegerValue.Equals(
            (int)BuiltInCategory.OST_Rooms));
    }
    public bool AllowReference (Reference r, XYZ p)
    {
        return false;
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