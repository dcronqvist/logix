﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LogiX.GLFW
{
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings
    {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GLFW.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to GLFW could not find support for the requested API on the system..
        /// </summary>
        internal static string ApiUnavailable
        {
            get
            {
                return ResourceManager.GetString("ApiUnavailable", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The requested pixel format is not supported or contents of the clipboard could not be converted to the requested format..
        /// </summary>
        internal static string FormatUnavailable
        {
            get
            {
                return ResourceManager.GetString("FormatUnavailable", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to One of the arguments to the function was an invalid enum value..
        /// </summary>
        internal static string InvalidEnum
        {
            get
            {
                return ResourceManager.GetString("InvalidEnum", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to One of the arguments to the function was an invalid value..
        /// </summary>
        internal static string InvalidValue
        {
            get
            {
                return ResourceManager.GetString("InvalidValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to A called that needs and operates on the current OpenGL or OpenGL ES context but no context is current on the calling thread.
        /// </summary>
        internal static string NoCurrentContext
        {
            get
            {
                return ResourceManager.GetString("NoCurrentContext", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to GLFW not yet initialized..
        /// </summary>
        internal static string NotInitialized
        {
            get
            {
                return ResourceManager.GetString("NotInitialized", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to A window that does not have an OpenGL or OpenGL ES context was passed to a function that requires it to have one..
        /// </summary>
        internal static string NoWindowContext
        {
            get
            {
                return ResourceManager.GetString("NoWindowContext", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to A memory allocation failed..
        /// </summary>
        internal static string OutOfMemory
        {
            get
            {
                return ResourceManager.GetString("OutOfMemory", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to A platform-specific error occurred.
        /// </summary>
        internal static string PlatformError
        {
            get
            {
                return ResourceManager.GetString("PlatformError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An unknown error has occurred..
        /// </summary>
        internal static string UnknownError
        {
            get
            {
                return ResourceManager.GetString("UnknownError", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The requested OpenGL or OpenGL ES version (including any requested context or framebuffer hints) is not available on this machine..
        /// </summary>
        internal static string VersionUnavailable
        {
            get
            {
                return ResourceManager.GetString("VersionUnavailable", resourceCulture);
            }
        }
    }
}
