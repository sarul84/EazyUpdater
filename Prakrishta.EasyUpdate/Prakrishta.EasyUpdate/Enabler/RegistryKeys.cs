//----------------------------------------------------------------------------------
// <copyright file="RegistryKeys.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/22/2019</date>
// <summary>Class that has registry key constants</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    /// <summary>
    /// Registry key constants
    /// </summary>
    public sealed class RegistryKeys
    {
        /// <summary>
        /// Skip Registry key Constant
        /// </summary>
        public const string Skip = "skip";

        /// <summary>
        /// Version registry key constant
        /// </summary>
        public const string Version = "version";

        /// <summary>
        /// Remind later registry key constant
        /// </summary>
        public const string RemindLater = "remindlater";

        /// <summary>
        /// Latest browser version key
        /// </summary>
        public const string LatestBrowserVersion = "svcVersion";

        /// <summary>
        /// Normal browser version key
        /// </summary>
        public const string BrowserVersion = "Version";
    }
}
