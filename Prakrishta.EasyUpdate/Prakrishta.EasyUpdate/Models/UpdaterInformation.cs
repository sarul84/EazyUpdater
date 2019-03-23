//----------------------------------------------------------------------------------
// <copyright file="UpdaterInformation.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/18/2019</date>
// <summary>The class that has details about updater</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;

    /// <summary>
    /// Has updater information
    /// </summary>
    public class UpdaterInformation
    {
        /// <summary>
        ///  Gets if new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable => CurrentVersion > InstalledVersion;

        /// <summary>
        /// Gets or sets download URL of the update file.
        /// </summary>
        public string DownloadURL { get; set; }

        /// <summary>
        /// Gets or sets URL of the webpage specifying changes in the new update.
        /// </summary>
        public string ChangelogURL { get; set; }

        /// <summary>
        /// Gets or sets newest version of the application available to download.
        /// </summary>
        public Version CurrentVersion { get; set; }

        /// <summary>
        /// Gets or setss version of the application currently installed on the user's PC.
        /// </summary>
        public Version InstalledVersion { get; set; }

        /// <summary>
        /// Gets is current update mandatory update.
        /// </summary>
        public bool IsMandatoryUpdate => UpdateMode == EazyUpdateMode.Forced || UpdateMode == EazyUpdateMode.ForcedDownload;

        /// <summary>
        /// Gets or sets defines how the Mandatory flag should work.
        /// </summary>
        public EazyUpdateMode UpdateMode { get; set; }

        /// <summary>
        /// Gets or sets command line arguments used by Installer.
        /// </summary>
        public string InstallerArgs { get; set; }

        /// <summary>
        /// Gets or sets checksum of the update file.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Gets or sets hash algorithm that generated the checksum provided in the XML file.
        /// </summary>
        public string HashingAlgorithm { get; set; }        
    }
}
