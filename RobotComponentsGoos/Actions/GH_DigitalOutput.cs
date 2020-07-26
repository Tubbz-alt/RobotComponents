﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/RobotComponents/RobotComponents>.

// Grasshopper Libs
using Grasshopper.Kernel.Types;
// RobotComponents Libs
using RobotComponents.Actions;

namespace RobotComponents.Gh.Goos.Actions
{
    /// <summary>
    /// Digital Output Goo wrapper class, makes sure the Digital Output class can be used in Grasshopper.
    /// </summary>
    public class GH_DigitalOutput : GH_Goo<DigitalOutput>
    {
        #region constructors
        /// <summary>
        /// Blank constructor
        /// </summary>
        public GH_DigitalOutput()
        {
            this.Value = null;
        }

        /// <summary>
        /// Data constructor: Creates a Digital Output Goo instance from Digital Ouput instance.
        /// </summary>
        /// <param name="digitalOutput"> Digital Output Value to store inside this Goo instance. </param>
        public GH_DigitalOutput(DigitalOutput digitalOutput)
        {
            this.Value = digitalOutput;
        }

        /// <summary>
        /// Data constructor: Creates a Digital Output Goo instance from another Digital Output Goo instance.
        /// This creates a shallow copy of the passed Digital Output Goo instance. 
        /// </summary>
        /// <param name="digitalOutputGoo"> Digital Output Goo instance to copy. </param>
        public GH_DigitalOutput(GH_DigitalOutput digitalOutputGoo)
        {
            if (digitalOutputGoo == null)
            {
                digitalOutputGoo = new GH_DigitalOutput();
            }

            this.Value = digitalOutputGoo.Value;
        }

        /// <summary>
        /// Make a complete duplicate of this Goo instance. No shallow copies.
        /// </summary>
        /// <returns> A duplicate of the Digital Output Goo. </returns>
        public override IGH_Goo Duplicate()
        {
            return new GH_DigitalOutput(Value == null ? new DigitalOutput() : Value.Duplicate());
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
        /// Gets a string describing the state of "invalidness". 
        /// If the instance is valid, then this property should return Nothing or String.Empty.
        /// </summary>
        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null) { return "No internal Digital Output instance"; }
                if (Value.IsValid) { return string.Empty; }
                return "Invalid Digital Output instance: Did you define the digital output name and state?";
            }
        }

        /// <summary>
        /// Creates a string description of the current instance value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Value == null) { return "Null Digital Output"; }
            else { return Value.ToString(); }
        }

        /// <summary>
        /// Gets the name of the type of the implementation.
        /// </summary>
        public override string TypeName
        {
            get { return "Digital Output"; }
        }

        /// <summary>
        /// Gets a description of the type of the implementation.
        /// </summary>
        public override string TypeDescription
        {
            get { return "Defines a Digital Output"; }
        }
        #endregion

        #region casting methods
        /// <summary>
        /// Attempt a cast to type Q.
        /// </summary>
        /// <typeparam name="Q"> Type to cast to.  </typeparam>
        /// <param name="target"> Pointer to target of cast. </param>
        /// <returns> True on success, false on failure. </returns>
        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to Digital Output
            if (typeof(Q).IsAssignableFrom(typeof(DigitalOutput)))
            {
                if (Value == null) { target = default(Q); }
                else { target = (Q)(object)Value; }
                return true;
            }

            //Cast to Digital Output Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_DigitalOutput)))
            {
                if (Value == null) { target = default(Q); }
                else { target = (Q)(object)new GH_DigitalOutput(Value); }
                return true;
            }

            //Cast to Action
            if (typeof(Q).IsAssignableFrom(typeof(Action)))
            {
                if (Value == null) { target = default(Q); }
                else { target = (Q)(object)Value; }
                return true;
            }

            //Cast to Action Goo
            if (typeof(Q).IsAssignableFrom(typeof(GH_Action)))
            {
                if (Value == null) { target = default(Q); }
                else { target = (Q)(object)new GH_Action(Value); }
                return true;
            }

            //Cast to Boolean
            if (typeof(Q).IsAssignableFrom(typeof(GH_Boolean)))
            {
                if (Value == null) { target = default(Q); }
                else { target = (Q)(object)new GH_Boolean(Value.IsActive); }
                return true;
            }

            target = default(Q);
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

            //Cast from Digital Output
            if (typeof(DigitalOutput).IsAssignableFrom(source.GetType()))
            {
                Value = source as DigitalOutput;
                return true;
            }

            //Cast from Action
            if (typeof(Action).IsAssignableFrom(source.GetType()))
            {
                if (source is DigitalOutput action)
                {
                    Value = action;
                    return true;
                }
            }

            //Cast from Action Goo
            if (typeof(GH_Action).IsAssignableFrom(source.GetType()))
            {
                GH_Action actionGoo = source as GH_Action;
                if (actionGoo.Value is DigitalOutput action)
                {
                    Value = action;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
