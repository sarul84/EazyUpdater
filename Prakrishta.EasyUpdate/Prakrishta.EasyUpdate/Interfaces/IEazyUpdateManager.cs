//----------------------------------------------------------------------------------
// <copyright file="IEazyUpdateManager.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>Contract that defines auto downloader behavior</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface that has contract details of eazy update manager
    /// </summary>
    public interface IEazyUpdateManager
    {
        /// <summary>
        /// Event handler if want to show custom messages to user instead of 
        /// eazy updater's standard messages
        /// </summary>
        event EventHandler<NotificationEventArgs> CustomNotification;

        /// <summary>
        /// Event handler to handle a situation if there is different file type than
        /// the one supported by easy updater
        /// </summary>
        event EventHandler<ParseUpdateInfoEventArgs> ParseUpdateInformation;

        /// <summary>
        /// Event handler to handle a situation if the update information is not persisted in a file (xml or text)
        /// Developer has to provide their own implementation for getting updater information
        /// </summary>
        event EventHandler<UpdateEventArgs> CheckForUpdateInformation;

        /// <summary>
        /// An event that developers can use to exit the application gracefully.
        /// </summary>
        event EventHandler ApplicationExit;

        /// <summary>
        /// Start update operation based on details from file in given url
        /// </summary>
        /// <param name="appcastUrl">The location where appcast xml file is available</param>
        /// <param name="assembly">The current application assembly detail</param>
        Task Update(string appcastUrl, Assembly assembly = null);

        /// <summary>
        /// Start update operation based on details from file in given url
        /// </summary>
        /// <param name="appcastUrl">The location where appcast xml file is available</param>
        /// <param name="updaterOptions">The updater options</param>
        /// <param name="assembly">The current application assembly detail</param>
        Task Update(string appcastUrl, EazyUpdaterOptions updaterOptions, Assembly assembly = null);

        /// <summary>
        /// Opens the Download window that download the update and execute the installer when download completes.
        /// </summary>
        /// <returns>Returns flag if download is successful</returns>
        bool DownloadUpdate();
    }
}
