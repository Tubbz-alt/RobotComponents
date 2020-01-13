﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using RobotComponents.BaseClasses;
using RobotComponents.Parameters;
using RobotComponents.Utils;

namespace RobotComponents.Components
{
    public class RobotToolFromPlanesComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear,  Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public RobotToolFromPlanesComponent()
          : base("Robot Tool From Planes", "RobToool",
              "Generates a robot tool based on attachment and effector planes."
                + System.Environment.NewLine +
                "RobotComponent V : " + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Definitions")
        {
        }

        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary, dropdown and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Robot Tool Name as String", GH_ParamAccess.item, "default_tool");
            pManager.AddMeshParameter("Mesh", "M", "Robot Tool Mesh as Mesh", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Attachment Plane", "AP", "Robot Tool Attachment Plane as Plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Tool Plane", "TP", "Robot Tool Plane as Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new RobotToolParameter(), "Robot Tool", "RT", "Resulting Robot Tool");  //Todo: beef this up to be more informative.
            pManager.Register_StringParam("Robot Tool Code", "RTC", "Robot Tool Code as a string");
        }

        // Global component variables
        public string lastName = "";
        public bool nameUnique;
        public RobotTool robTool = new RobotTool();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Gets Document ID
            Guid documentID = this.OnPingDocument().DocumentID;

            // Checks if ObjectManager for this document already exists. If not it creates a new one
            if (!DocumentManager.ObjectManagers.ContainsKey(documentID))
            {
                DocumentManager.ObjectManagers.Add(documentID, new ObjectManager());
            }

            // Gets ObjectManager of this document
            ObjectManager objectManager = DocumentManager.ObjectManagers[documentID];

            // Adds Component to ToolsByGuid Dictionary
            if (!objectManager.ToolsPlanesByGuid.ContainsKey(this.InstanceGuid))
            {
                objectManager.ToolsPlanesByGuid.Add(this.InstanceGuid, this);
            }

            // Removes lastName from toolNameList
            if (objectManager.ToolNames.Contains(lastName))
            {
                objectManager.ToolNames.Remove(lastName);
            }

            // Input variables
            string name = "default_tool";
            List<Mesh> meshes = new List<Mesh>();
            Plane attachmentPlane = Plane.Unset;
            Plane toolPlane = Plane.Unset;

            // Catch the input data
            if (!DA.GetData(0, ref name)) { return; }
            if (!DA.GetDataList(1,  meshes)) { return; }
            if (!DA.GetData(2, ref attachmentPlane)) { return; }
            if (!DA.GetData(3, ref toolPlane)) { return; };

            // Robot Tool mesh
            Mesh mesh = new Mesh();

            // Join the Robot Tool mesh to one single mesh
            for (int i = 0; i < meshes.Count; i++)
            {
                mesh.Append(meshes[i]);
            }

            // Create the Robot Tool
            RobotTool robotTool = new RobotTool(name, mesh, attachmentPlane, toolPlane);

            // Checks if the tool name is already in use and counts duplicates
            #region NameCheck
            if (objectManager.ToolNames.Contains(robotTool.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Tool Name already in use.");
                nameUnique = false;
                lastName = "";
            }
            else
            {
                // Adds Robot Tool Name to list
                objectManager.ToolNames.Add(robotTool.Name);

                // Run SolveInstance on other Tools with no unique Name to check if their name is now available
                foreach (KeyValuePair<Guid, RobotToolFromDataEulerComponent> entry in objectManager.ToolsEulerByGuid)
                {
                    if (entry.Value.lastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }
                foreach (KeyValuePair<Guid, RobotToolFromPlanesComponent> entry in objectManager.ToolsPlanesByGuid)
                {
                    if (entry.Value.lastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }

                lastName = robotTool.Name;
                nameUnique = true;
            }

            // Checks if variable name exceeds max character limit for RAPID Code
            if (HelperMethods.VariableExeedsCharacterLimit32(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Robot Tool Name exceeds character limit of 32 characters.");
            }

            // Checks if variable name starts with a number
            if (HelperMethods.VariableStartsWithNumber(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Robot Tool Name starts with a number which is not allowed in RAPID Code.");
            }
            #endregion

            // Outputs
            robTool = robotTool;
            DA.SetData(0, robotTool);
            DA.SetData(1, robotTool.GetRSToolData());
        }

        /// <summary>
        /// This method detects if the user deletes the component from the Grasshopper canvas. 
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void DocumentObjectsDeleted(object sender, GH_DocObjectEventArgs e)
        {
            // Gets Document ID
            Guid documentID = this.OnPingDocument().DocumentID;

            // Checks if ObjectManager for this document already exists. If not it creates a new one
            if (!DocumentManager.ObjectManagers.ContainsKey(documentID))
            {
                DocumentManager.ObjectManagers.Add(documentID, new ObjectManager());
            }

            // Gets ObjectManager of this document
            ObjectManager objectManager = DocumentManager.ObjectManagers[documentID];

            if (e.Objects.Contains(this))
            {
                if (nameUnique == true)
                {
                    objectManager.ToolNames.Remove(lastName);
                }
                objectManager.ToolsPlanesByGuid.Remove(this.InstanceGuid);

                // Run SolveInstance on other Tools with no unique Name to check if their name is now available
                foreach (KeyValuePair<Guid, RobotToolFromDataEulerComponent> entry in objectManager.ToolsEulerByGuid)
                {
                    if (entry.Value.lastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }
                foreach (KeyValuePair<Guid, RobotToolFromPlanesComponent> entry in objectManager.ToolsPlanesByGuid)
                {
                    if (entry.Value.lastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ToolPlane_Icon; }
        }
 
        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6d430719-5c45-4b66-a676-71be54f6ee93"); }
        }
    }

}