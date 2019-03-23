//----------------------------------------------------------------------------------
// <copyright file="EazyUpdaterOptions.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>The eazy updater options class</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System.Net;

    public class EazyUpdaterOptions
    {
        /// <summary>
        /// Gets or sets path where update packge will be Download
        /// </summary>
        public string DownloadPath { get; set; }

        /// <summary>
        /// Gets or sets application title. Although Easy updater 
        /// will get it automatically, can set this property if like to give custom Title.
        /// </summary>
        public string ApplicationTitle { get; set; }

        /// <summary>
        /// Gets or sets flag to indicate if the update process should be started as admin
        /// </summary>
        public bool RunUpdateAsAdmin { get; set; }

        /// <summary>
        /// Gets or sets flag to indicate if the update process should be started as another user
        /// </summary>
        public bool RunUpdateAsAnotherUser { get; set; }

        /// <summary>
        /// Gets or sets athentication to download file if there is any
        /// </summary>
        public Authentication DownloadAuthentication { get; set; }

        /// <summary>
        /// Gets or sets authentication to download App cast file
        /// </summary>
        public Authentication AppcastFileAuthentication { get; set; }

        /// <summary>
        /// Gets or sets account credentials to run update as impersonate user
        /// </summary>
        public NetworkCredential ServiceAccount { get; set; }

        /// <summary>
        /// Gets or sets Report errors flag
        /// </summary>
        public bool ReportErrors { get; set; }
    }
}
