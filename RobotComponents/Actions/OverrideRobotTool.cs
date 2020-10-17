﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/RobotComponents/RobotComponents>.

// System Libs
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
// RobotComponents Libs
using RobotComponents.Definitions;
using RobotComponents.Utils;

namespace RobotComponents.Actions
{
    /// <summary>
    /// Represents the Override Robot Tool action.
    /// This action is used to set a new default Robot Tool from this action. 
    /// </summary>
    [Serializable()]
    public class OverrideRobotTool : Action, ISerializable
    {
        #region fields
        private RobotTool _robotTool; // The robot that should be used
        #endregion

        #region (de)serialization
        /// <summary>
        /// Protected constructor needed for deserialization of the object.  
        /// </summary>
        /// <param name="info"> The SerializationInfo to extract the data from. </param>
        /// <param name="context"> The context of this deserialization. </param>
        protected OverrideRobotTool(SerializationInfo info, StreamingContext context)
        {
            // int version = (int)info.GetValue("Version", typeof(int)); // <-- use this if the (de)serialization changes
            _robotTool = (RobotTool)info.GetValue("Robot Tool", typeof(RobotTool));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the object.
        /// </summary>
        /// <param name="info"> The SerializationInfo to populate with data. </param>
        /// <param name="context"> The destination for this serialization. </param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", VersionNumbering.CurrentVersionAsInt, typeof(int));
            info.AddValue("Robot Tool", _robotTool, typeof(RobotTool));
        }
        #endregion

        #region constructors
        /// <summary>
        /// Defines an empty Override Robot Tool object. 
        /// </summary>
        public OverrideRobotTool()
        {
        }

        /// <summary>
        /// Creates and Override Robot Tool object.
        /// </summary>
        /// <param name="robotTool"> The Robot Tool that should be set. </param>
        public OverrideRobotTool(RobotTool robotTool)
        {
            _robotTool = robotTool;
        }

        /// <summary>
        /// Creates a new override robot tool by duplicating an existing override robot tool. 
        /// This creates a deep copy of the existing override robot tool. 
        /// </summary>
        /// <param name="overrideRobotTool"> The override robot tool that should be duplicated. </param>
        public OverrideRobotTool(OverrideRobotTool overrideRobotTool)
        {
            _robotTool = overrideRobotTool.RobotTool.Duplicate();
        }

        /// <summary>
        /// Method to duplicate the Override Robot Tool object.
        /// </summary>
        /// <returns> Returns a deep copy of the Override Robot Tool object.</returns>
        public OverrideRobotTool Duplicate()
        {
            return new OverrideRobotTool(this);
        }

        /// <summary>
        /// A method to duplicate the OverrideRobotTool object to an Action object. 
        /// </summary>
        /// <returns> Returns a deep copy of the OverrideRobotTool object as an Action object. </returns>
        public override Action DuplicateAction()
        {
            return new OverrideRobotTool(this) as Action;
        }
        #endregion

        #region method
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
        {
            if (!this.IsValid)
            {
                return "Invalid Override Robot Tool";
            }
            else
            {
                return "Override Robot Tool (" + this.ToolName + ")";
            }
        }

        /// <summary>
        /// Used to create variable definition code of this action. 
        /// </summary>
        /// <param name="robot"> Defines the Robot were the code is generated for. </param>
        /// <returns> Returns the RAPID code line as a string. </returns>
        public override string ToRAPIDDeclaration(Robot robot)
        {
            robot.Tool = _robotTool;
            return "! " + "Default Robot Tool changed to " + robot.Tool.Name + ".";
        }

        /// <summary>
        /// Used to create action instruction code line. 
        /// </summary>
        /// <param name="robot"> Defines the Robot were the code is generated for. </param>
        /// <returns> Returns the RAPID code line as a string. </returns>
        public override string ToRAPIDInstruction(Robot robot)
        {
            robot.Tool = _robotTool;
            return "! " + "Default Robot Tool changed to " + robot.Tool.Name + ".";
        }

        /// <summary>
        /// Used to create variable definitions in the RAPID Code. It is typically called inside the CreateRAPIDCode() method of the RAPIDGenerator class.
        /// </summary>
        /// <param name="RAPIDGenerator"> Defines the RAPIDGenerator. </param>
        public override void ToRAPIDDeclaration(RAPIDGenerator RAPIDGenerator)
        {
            // We don't write a comment between our variable names
            this.ToRAPIDDeclaration(RAPIDGenerator.Robot);
        }

        /// <summary>
        /// Used to create action instructions in the RAPID Code. It is typically called inside the CreateRAPIDCode() method of the RAPIDGenerator class.
        /// </summary>
        /// <param name="RAPIDGenerator"> Defines the RAPIDGenerator. </param>
        public override void ToRAPIDInstruction(RAPIDGenerator RAPIDGenerator)
        {
            RAPIDGenerator.StringBuilder.Append(Environment.NewLine + "\t\t" + this.ToRAPIDInstruction(RAPIDGenerator.Robot));
        }

        /// <summary>
        /// Get the name of the set Robot Tool.
        /// </summary>
        /// <returns> The name of the set Robot Tool. </returns>
        public string GetToolName()
        {
            return _robotTool.Name;
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether the object is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (ToolName == null) { return false; }
                if (ToolName == "") { return false; }
                if (RobotTool == null) { return false; }
                if (RobotTool.IsValid == false) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the Robot Tool.
        /// </summary>
        public RobotTool RobotTool
        {
            get { return _robotTool; }
            set { _robotTool = value; }
        }

        /// <summary>
        /// Gets the name of the Robot Tool.
        /// </summary>
        public string ToolName
        {
            get { return _robotTool.Name; }
        }
        #endregion
    }
}
