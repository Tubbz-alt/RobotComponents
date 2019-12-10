﻿using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using RobotComponents.BaseClasses;

namespace RobotComponents.Goos
{
    /// <summary>
    /// RobotInfo Goo wrapper class, makes sure RobotInfo can be used in Grasshopper.
    /// </summary>
    public class RobotInfoGoo : GH_GeometricGoo<RobotInfo>, IGH_PreviewData
    {
        #region constructors
        /// <summary>
        /// Blank constructor
        /// </summary>
        public RobotInfoGoo()
        {
            this.Value = new RobotInfo();
        }

        /// <summary>
        /// Data constructor, m_value will be set to internal_data.
        /// </summary>
        /// <param name="robotInfo"> RobotInfo Value to store inside this Goo instance. </param>
        public RobotInfoGoo(RobotInfo robotInfo)
        {
            if (robotInfo == null)
                robotInfo = new RobotInfo();
            this.Value = robotInfo;
        }

        /// <summary>
        /// Make a complete duplicate of this geometry. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the RobotInfoGoo. </returns>
        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return DuplicateRobotInfoGoo();
        }

        /// <summary>
        /// Make a complete duplicate of this geometry. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the RobotInfoGoo. </returns>
        public RobotInfoGoo DuplicateRobotInfoGoo()
        {
            return new RobotInfoGoo(Value == null ? new RobotInfo() : Value.Duplicate());
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets a value indicating whether or not the current value is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                return Value.IsValid;
            }
        }

        /// <summary>
        /// ets a string describing the state of "invalidness". 
        /// If the instance is valid, then this property should return Nothing or String.Empty.
        /// </summary>
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) { return "No internal RobotInfo instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid RobotInfo instance: Did you define an AxisValues, AxisLimits, BasePlane and MountingFrame?"; //Todo: beef this up to be more informative.
            }
        }

        /// <summary>
        /// Creates a string description of the current instance value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Value == null)
                return "Null RobotInfo";
            else
                return "Robot Info";
        }

        /// <summary>
        /// Gets the name of the type of the implementation.
        /// </summary>
        public override string TypeName
        {
            get { return ("RobotInfo"); }
        }

        /// <summary>
        /// Gets a description of the type of the implementation.
        /// </summary>
        public override string TypeDescription
        {
            get { return ("Defines a single RobotInfo"); }
        }

        /// <summary>
        /// Gets the boundingbox for this geometry.
        /// </summary>
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                if (Value.Meshes == null) { return BoundingBox.Empty; }
                else
                {
                    // Make the bounding box at the base plane
                    BoundingBox MeshBoundingBox = BoundingBox.Empty;
                    for (int i = 0; i != Value.Meshes.Count; i++)
                    {
                        MeshBoundingBox.Union(Value.Meshes[i].GetBoundingBox(true));
                    }
                    
                    // Orient the bounding box to the position plane
                    Transform orientNow;
                    orientNow = Rhino.Geometry.Transform.ChangeBasis(Value.BasePlane, Plane.WorldXY);
                    MeshBoundingBox.Transform(orientNow);

                    return MeshBoundingBox;
                }
            }
        }

        /// <summary>
        /// Compute an aligned boundingbox.
        /// </summary>
        /// <param name="xform"> Transformation to apply to geometry for BoundingBox computation. </param>
        /// <returns> The world aligned boundingbox of the transformed geometry. </returns>
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return Boundingbox;
        }
        #endregion

        #region casting methods
        /// <summary>
        /// Attempt a cast to type Q.
        /// </summary>
        /// <typeparam name="Q"> Type to cast to.  </typeparam>
        /// <param name="robotInfo"> Pointer to target of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastTo<Q>(out Q robotInfo)
        {
            //Cast to RobotInfo.
            if (typeof(Q).IsAssignableFrom(typeof(RobotInfo)))
            {
                if (Value == null)
                    robotInfo = default(Q);
                else
                    robotInfo = (Q)(object)Value;
                return true;
            }

            //Cast to Mesh.
            if (typeof(Q).IsAssignableFrom(typeof(List<Mesh>)))
            {
                if (Value == null)
                    robotInfo = default(Q);
                else if (Value.Meshes == null)
                    robotInfo = default(Q);
                else
                    robotInfo = (Q)(object)Value.Meshes;
                return true;
            }

            //Todo: cast to point, number, mesh, curve?

            robotInfo = default(Q);
            return false;
        }

        /// <summary>
        /// Attempt a cast from generic object.
        /// </summary>
        /// <param name="source"> Reference to source of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }

            //Cast from RobotInfo
            if (typeof(RobotInfo).IsAssignableFrom(source.GetType()))
            {
                Value = (RobotInfo)source;
                return true;
            }

            return false;
        }
        #endregion

        #region transformation methods
        /// <summary>
        /// Transforms the object or a deformable representation of the object.
        /// </summary>
        /// <param name="xform"> Transformation matrix. </param>
        /// <returns> Transformed geometry. If the local geometry can be transformed accurately, 
        /// then the returned instance equals this instance. Not all geometry types can be accurately 
        /// transformed under all circumstances though, if this is the case, this function will 
        /// return an instance of another IGH_GeometricGoo derived type which can be transformed.</returns>
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return null;
        }

        /// <summary>
        /// Morph the object or a deformable representation of the object.
        /// </summary>
        /// <param name="xmorph"> Spatial deform. </param>
        /// <returns> Deformed geometry. If the local geometry can be deformed accurately, then the returned 
        /// instance equals this instance. Not all geometry types can be accurately deformed though, if 
        /// this is the case, this function will return an instance of another IGH_GeometricGoo derived 
        /// type which can be deformed.</returns>
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return null;
        }
        #endregion

        #region drawing methods
        /// <summary>
        /// Gets the clipping box for this data. The clipping box is typically the same as the boundingbox.
        /// </summary>
        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }

        /// <summary>
        /// Implement this function to draw all shaded meshes. 
        /// If the viewport does not support shading, this function will not be called.
        /// </summary>
        /// <param name="args"> Drawing arguments. </param>
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null) { return; }
            if (Value.Meshes != null)
            {
                List<double> internalAxisValues = new List<double> { 0, 0, 0, 0, 0, 0 };
                List<double> externalAxisValues = new List<double> { 0, 0, 0, 0, 0, 0 };

                ForwardKinematics forwardKinematics = new ForwardKinematics(this.Value, internalAxisValues, externalAxisValues);
                forwardKinematics.Calculate();

                if (forwardKinematics.PosedMeshes != null)
                {
                    for (int i = 0; i != forwardKinematics.PosedMeshes.Count; i++)
                    {
                        args.Pipeline.DrawMeshShaded(forwardKinematics.PosedMeshes[i], new Rhino.Display.DisplayMaterial(System.Drawing.Color.FromArgb(225, 225, 225), 0));
                    }
                }
            }
        }

        /// <summary>
        /// Implement this function to draw all wire and point previews.
        /// </summary>
        /// <param name="args"> Drawing arguments. </param>
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null) { return; }

            //Draw hull shape.
            if (Value.Meshes != null)
            {
                List<double> internalAxisValues = new List<double> { 0, 0, 0, 0, 0, 0 };
                List<double> externalAxisValues = new List<double> { 0, 0, 0, 0, 0, 0 };

                ForwardKinematics forwardKinematics = new ForwardKinematics(this.Value, internalAxisValues, externalAxisValues);
                forwardKinematics.Calculate();

                if (forwardKinematics.PosedMeshes != null)
                {
                    for (int i = 0; i != forwardKinematics.PosedMeshes.Count; i++)
                    {
                        args.Pipeline.DrawMeshWires(forwardKinematics.PosedMeshes[i], args.Color, -1);
                    }
                }
            }
        }
        #endregion
    }
}
