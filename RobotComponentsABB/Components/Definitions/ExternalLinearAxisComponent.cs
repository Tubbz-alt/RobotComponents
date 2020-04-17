﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/EDEK-UniKassel/RobotComponents>.

// System Libs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
// Grasshopper Libs
using Grasshopper.Kernel;
// Rhino Libs
using Rhino.Geometry;
// RobotComponents Libs
using RobotComponentsABB.Parameters.Definitions;
using RobotComponents.BaseClasses.Definitions;
using RobotComponentsABB.Utils;

namespace RobotComponentsABB.Components.Definitions
{
    /// <summary>
    /// RobotComponents External Linear Axis component. An inherent from the GH_Component Class.
    /// </summary>
    public class ExternalLinearAxisComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public ExternalLinearAxisComponent()
          : base("External Linear Axis", "External Linear Axis",
              "Defines an External Linear Axis for any Robot."
                + System.Environment.NewLine +
                "RobotComponents : v" + RobotComponents.Utils.VersionNumbering.CurrentVersion,
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
            pManager.AddTextParameter("Name", "N", "Axis Name as a Text", GH_ParamAccess.item, "default_ela");
            pManager.AddPlaneParameter("Attachment plane", "AP", "Attachement plane of robot. Overrides robot position plane.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Axis", "A", "Axis as Vector", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Axis Limits", "AL", "Axis Limits as Domain", GH_ParamAccess.item);
            pManager.AddMeshParameter("Base Mesh", "BM", "Base Mesh as Mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Link Mesh", "LM", "Link Mesh as Mesh", GH_ParamAccess.list);

            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new ExternalLinearAxisParameter(), "External Linear Axis", "ELA", "Resulting External Linear Axis");  //Todo: beef this up to be more informative.
        }

        // Fields
        private string _axisName = String.Empty;
        private string _lastName = "";
        private bool _nameUnique;
        private ObjectManager _objectManager;
        private ExternalLinearAxis _externalLinearAxis;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            string name = "";
            Plane attachmentPlane = Plane.WorldXY;
            Vector3d axis = new Vector3d(0,0,0);
            Interval limits = new Interval(0, 0);
            List<Mesh> baseMeshes = new List<Mesh>();
            List<Mesh> linkMeshes = new List<Mesh>();

            // Catch the input data
            if (!DA.GetData(0, ref name)) { return; }
            if (!DA.GetData(1, ref attachmentPlane)) { return; }
            if (!DA.GetData(2, ref axis)) { return; }
            if (!DA.GetData(3, ref limits)) { return; }
            if (!DA.GetDataList(4, baseMeshes)) { baseMeshes = new List<Mesh>() { new Mesh() }; }
            if (!DA.GetDataList(5, linkMeshes)) { linkMeshes = new List<Mesh>() { new Mesh() }; }

            // Create the external linear axis
            _externalLinearAxis = new ExternalLinearAxis(name, attachmentPlane, axis, limits, baseMeshes, linkMeshes);

            // Output
            DA.SetData(0, _externalLinearAxis);

            #region Object manager
            // Gets ObjectManager of this document
            _objectManager = DocumentManager.GetDocumentObjectManager(this.OnPingDocument());

            // Clears ExternalAxisNames
            _objectManager.ExternalAxisNames.Remove(_axisName);
            _axisName = String.Empty;

            // Removes lastName from ExternalAxisNames List
            if (_objectManager.ExternalAxisNames.Contains(_lastName))
            {
                _objectManager.ExternalAxisNames.Remove(_lastName);
            }

            // Adds Component to ExternalLinarAxesByGuid Dictionary
            if (!_objectManager.ExternalLinearAxesByGuid.ContainsKey(this.InstanceGuid))
            {
                _objectManager.ExternalLinearAxesByGuid.Add(this.InstanceGuid, this);
            }

            // Checks if axis name is already in use and counts duplicates
            #region Check name in object manager
            if (_objectManager.ExternalAxisNames.Contains(_externalLinearAxis.Name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "External Axis Name already in use.");
                _nameUnique = false;
                _lastName = "";
            }
            else
            {
                // Adds Robot Axis Name to list
                _axisName = _externalLinearAxis.Name;
                _objectManager.ExternalAxisNames.Add(_externalLinearAxis.Name);

                // Run SolveInstance on other External Axes with no unique Name to check if their name is now available
                foreach (KeyValuePair<Guid, ExternalLinearAxisComponent> entry in _objectManager.ExternalLinearAxesByGuid)
                {
                    if (entry.Value.LastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }
                foreach (KeyValuePair<Guid, ExternalRotationalAxisComponent> entry in _objectManager.ExternalRotationalAxesByGuid)
                {
                    if (entry.Value.LastName == "")
                    {
                        entry.Value.ExpireSolution(true);
                    }
                }

                _lastName = _externalLinearAxis.Name;
                _nameUnique = true;
            }
            #endregion

            // Recognizes if Component is Deleted and removes it from Object Managers axis and name list
            GH_Document doc = this.OnPingDocument();
            if (doc != null)
            {
                doc.ObjectsDeleted += DocumentObjectsDeleted;
            }
            #endregion
        }

        /// <summary>
        /// This method detects if the user deletes the component from the Grasshopper canvas. 
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void DocumentObjectsDeleted(object sender, GH_DocObjectEventArgs e)
        {
            if (e.Objects.Contains(this))
            {
                if (_nameUnique == true)
                {
                    _objectManager.ExternalAxisNames.Remove(_axisName);
                }
                _objectManager.ExternalLinearAxesByGuid.Remove(this.InstanceGuid);

                // Run SolveInstance on other External Axes with no unique Name to check if their name is now available
                foreach (KeyValuePair<Guid, ExternalLinearAxisComponent> entry in _objectManager.ExternalLinearAxesByGuid)
                {
                    entry.Value.ExpireSolution(true);
                }
                foreach (KeyValuePair<Guid, ExternalRotationalAxisComponent> entry in _objectManager.ExternalRotationalAxesByGuid)
                {
                    entry.Value.ExpireSolution(true);
                }
            }
        }

        /// <summary>
        /// The external linear axis created by this component
        /// </summary>
        public ExternalLinearAxis ExternalLinearAxis
        {
            get { return _externalLinearAxis; }
        }

        /// <summary>
        /// The external linear axis created by this component as External Axis
        /// </summary>
        public ExternalAxis ExternalAxis
        {
            get { return _externalLinearAxis as ExternalAxis; }
        }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
        }

        #region menu item
        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Documentation", MenuItemClickComponentDoc, Properties.Resources.WikiPage_MenuItem_Icon);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Documentation" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickComponentDoc(object sender, EventArgs e)
        {
            string url = Documentation.ComponentWeblinks[this.GetType()];
            System.Diagnostics.Process.Start(url);
        }
        #endregion

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ExternalLinearAxis_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B438238D-FF4C-48BC-ADE5-1772C99BE599"); }
        }
    }
}
