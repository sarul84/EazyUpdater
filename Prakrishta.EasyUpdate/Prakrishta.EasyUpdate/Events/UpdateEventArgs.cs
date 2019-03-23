//----------------------------------------------------------------------------------
// <copyright file="UpdateEventArgs.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>Custom event arg for notifying update information</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Custom class that contain update event data, and provides a value to use it in update manager
    /// </summary>
    public sealed class UpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets update information received from appcast file or database or any other persistance
        /// </summary>
        public UpdaterInformation UpdateInfo { get; set; }

        /// <summary>
        /// Gets or sets updater options
        /// </summary>
        public EazyUpdaterOptions Options { get; set; }

        /// <summary>
        /// Gets or sets current assembly information
        /// </summary>
        public Assembly AssemblyInfo { get; set; }
    }
}
