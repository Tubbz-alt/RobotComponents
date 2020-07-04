﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/RobotComponents/RobotComponents>.

// System Libs
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
// Rhino Libs
using Rhino.Geometry;
// Grasshopper Libs
using Grasshopper.Kernel;
using GH_IO.Serialization;
// RobotComponents Libs
using RobotComponents.Actions;
using RobotComponents.Kinematics;
using RobotComponents.Definitions;
using RobotComponentsABB.Parameters.Definitions;
using RobotComponentsABB.Parameters.Actions;
using RobotComponentsABB.Utils;

namespace RobotComponentsABB.Components.Simulation
{
    /// <summary>
    /// RobotComponents Path Generator component. An inherent from the GH_Component Class.
    /// </summary>
    public class PathGeneratorComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, Subcategory the panel. 
        /// If you use non-existing tab or panel names, new tabs/panels will automatically be created.
        /// </summary>
        public PathGeneratorComponent()
          : base("Path Generator", "PG",
              "EXPERIMENTAL: Generates and displays an approximation of the movement path for a defined ABB robot based on a list of Actions."
                + System.Environment.NewLine + System.Environment.NewLine +
                "Robot Components: v" + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Simulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RobotParameter(), "Robot", "R", "Robot as Robot", GH_ParamAccess.item);
            pManager.AddParameter(new ActionParameter(), "Actions", "A", "Actions as a list with Actions", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Interpolations", "I", "Interpolations as Int", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Animation Slider", "AS", "Animation Slider as number (0.0 - 1.0)", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Update", "U", "If set to true, path will be constantly recalculated.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PlaneParam("Plane", "EP", "Current End Plane as a Plane");
            pManager.Register_PlaneParam("External Axis Planes", "EAP", "Current Exernal Axis Planes as a list with Planes");
            pManager.RegisterParam(new RobotJointPositionParameter(), "Robot Joint Position", "RJ", "The current Robot Joint Position");
            pManager.RegisterParam(new ExternalJointPositionParameter(), "External Joint Position", "EJ", "The current External Joint Position");
            pManager.Register_CurveParam("Tool Path", "TP", "Tool Path as a list with Curves");
        }

        // Fields
        private Robot _robotInfo;
        private PathGenerator _pathGenerator = new PathGenerator();
        private ForwardKinematics _forwardKinematics = new ForwardKinematics();
        private List<Plane> _planes = new List<Plane>();
        private List<Curve> _paths = new List<Curve>();
        private List<List<double>> _internalAxisValues = new List<List<double>>();
        private List<List<double>> _externalAxisValues = new List<List<double>>();
        private int _lastInterpolations = 0;
        private bool _raiseWarnings = false;
        private bool _previewMesh = true;
        private bool _previewCurve = true;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            List<RobotComponents.Actions.Action> actions = new List<RobotComponents.Actions.Action>();
            int interpolations = 0;
            double interpolationSlider = 0;
            bool update = false;

            // Catch the input data
            if (!DA.GetData(0, ref _robotInfo)) { return; }
            if (!DA.GetDataList(1, actions)) { return; }
            if (!DA.GetData(2, ref interpolations)) { return; }
            if (!DA.GetData(3, ref interpolationSlider)) { return; }
            if (!DA.GetData(4, ref update)) { return; }

            // Update the path
            if (update == true || _lastInterpolations != interpolations)
            {
                // Create forward kinematics for mesh display
                _forwardKinematics = new ForwardKinematics(_robotInfo);

                // Create the path generator
                _pathGenerator = new PathGenerator(_robotInfo);

                // Re-calculate the path
                _pathGenerator.Calculate(actions, interpolations);

                // Get all the targets
                _planes.Clear();
                _planes = _pathGenerator.Planes;

                // Get the new path curve
                _paths.Clear();
                _paths = _pathGenerator.Paths;

                // Clear the lists with the internal and external axis values
                ClearAxisValuesLists();
                _internalAxisValues = _pathGenerator.InternalAxisValues;
                _externalAxisValues = _pathGenerator.ExternalAxisValues;

                // Store the number of interpolations that are used, to check if this value is changed. 
                _lastInterpolations = interpolations;

                // Raise warnings
                if (_pathGenerator.ErrorText.Count != 0)
                {
                    _raiseWarnings = true;
                }
                else
                {
                    _raiseWarnings = false;
                }
            }

            // Get the index number of the current target
            int index = (int)(((_planes.Count - 1) * interpolationSlider));

            // Calcualte foward kinematics
            _forwardKinematics.InternalAxisValues = _internalAxisValues[index];
            _forwardKinematics.ExternalAxisValues = _externalAxisValues[index];
            _forwardKinematics.HideMesh = !_previewMesh;
            _forwardKinematics.Calculate();

            // Show error messages
            if (_raiseWarnings == true)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only axis values of absolute joint movements are checked.");

                for (int i = 0; i < _pathGenerator.ErrorText.Count; i++)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, _pathGenerator.ErrorText[i]);
                    if (i == 30) { break; }
                }
            }

            // Output
            DA.SetData(0, _forwardKinematics.TCPPlane);
            DA.SetDataList(1, _forwardKinematics.PosedExternalAxisPlanes);
            DA.SetData(2, _forwardKinematics.RobotJointPosition);
            DA.SetData(3, _forwardKinematics.ExternalJointPosition);
            if (_previewCurve == true) { DA.SetDataList(4, _paths); }
            else { DA.SetDataList(4, null); }
        }

        /// <summary>
        /// This method clears the two lists with internal and external values
        /// </summary>
        private void ClearAxisValuesLists()
        {
            // Clear the all the individual lists with internal axis values
            for (int i = 0; i < _internalAxisValues.Count; i++)
            {
                _internalAxisValues[i].Clear();
            }

            // Clear the all the individual lists with external axis values
            for (int i = 0; i < _externalAxisValues.Count; i++)
            {
                _externalAxisValues.Clear();
            }

            // Clear both primary lists
            _internalAxisValues.Clear();
            _externalAxisValues.Clear();

        }
        #region menu item
        /// <summary>
        /// Boolean that indicates if the custom menu item for hiding the robot mesh is checked
        /// </summary>
        public bool SetPreviewCurve
        {
            get { return _previewCurve; }
            set { _previewCurve = value; }
        }

        /// <summary>
        /// Boolean that indicates if the custom menu item for hiding the robot mesh is checked
        /// </summary>
        public bool SetPreviewMesh
        {
            get { return _previewMesh; }
            set { _previewMesh = value; }
        }

        /// <summary>
        /// Add our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="writer"> Provides access to a subset of GH_Chunk methods used for writing archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Set Preview Curve", SetPreviewCurve);
            writer.SetBoolean("Set Preview Mesh", SetPreviewMesh);
            return base.Write(writer);
        }


        /// <summary>
        /// Read our own fields. Needed for (de)serialization of the variable input parameters.
        /// </summary>
        /// <param name="reader"> Provides access to a subset of GH_Chunk methods used for reading archives. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool Read(GH_IReader reader)
        {
            SetPreviewCurve = reader.GetBoolean("Set Preview Curve");
            SetPreviewMesh = reader.GetBoolean("Set Preview Mesh");
            return base.Read(reader);
        }

        /// <summary>
        /// Adds the additional items to the context menu of the component. 
        /// </summary>
        /// <param name="menu"> The context menu of the component. </param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Preview Curve", MenuItemClickPreviewCurve, true, SetPreviewCurve);
            Menu_AppendItem(menu, "Preview Mesh", MenuItemClickPreviewMesh, true, SetPreviewMesh);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Documentation", MenuItemClickComponentDoc, Properties.Resources.WikiPage_MenuItem_Icon);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Preview Curve" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickPreviewCurve(object sender, EventArgs e)
        {
            RecordUndoEvent("Set Preview Curve");
            _previewCurve = !_previewCurve;
            ExpireSolution(true);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Preview Mesh" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickPreviewMesh(object sender, EventArgs e)
        {
            RecordUndoEvent("Set Preview Mesh");
            _previewMesh = !_previewMesh;
            ExpireSolution(true);
        }

        /// <summary>
        /// Handles the event when the custom menu item "Documentation" is clicked. 
        /// </summary>
        /// <param name="sender"> The object that raises the event. </param>
        /// <param name="e"> The event data. </param>
        private void MenuItemClickComponentDoc(object sender, EventArgs e)
        {
            string url = Documentation.ComponentWeblinks[this.GetType()];
            Documentation.OpenBrowser(url);
        }
        #endregion

        /// <summary>
        /// This method displays the robot pose for the given axis values. 
        /// </summary>
        /// <param name="args"> Preview display arguments for IGH_PreviewObjects. </param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // Check if there is a mesh available to display and the onlyTCP function not active
            if (_forwardKinematics.PosedInternalAxisMeshes != null && _previewMesh)
            {
                // Initiate the display color and transparancy of the robot mesh
                Color color;
                double trans;

                // Set the display color and transparancy of the robot mesh
                if (_forwardKinematics.InLimits == true)
                {
                    color = Color.FromArgb(225, 225, 225);
                    trans = 0.0;
                }
                else
                {
                    color = Color.FromArgb(150, 0, 0);
                    trans = 0.5;
                }

                // Display the internal axes of the robot
                for (int i = 0; i != _forwardKinematics.PosedInternalAxisMeshes.Count; i++)
                {
                    args.Display.DrawMeshShaded(_forwardKinematics.PosedInternalAxisMeshes[i], new Rhino.Display.DisplayMaterial(color, trans));
                }

                // Display the external axes
                for (int i = 0; i != _forwardKinematics.PosedExternalAxisMeshes.Count; i++)
                {
                    for (int j = 0; j != _forwardKinematics.PosedExternalAxisMeshes[i].Count; j++)
                    {
                        args.Display.DrawMeshShaded(_forwardKinematics.PosedExternalAxisMeshes[i][j], new Rhino.Display.DisplayMaterial(color, trans));
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
            get { return Properties.Resources.PathGen_Icon; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("32B682C9-E301-4372-A341-7AB0A97716CF"); }
        }
    }
}
