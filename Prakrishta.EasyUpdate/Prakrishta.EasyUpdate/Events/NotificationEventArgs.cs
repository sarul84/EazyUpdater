//----------------------------------------------------------------------------------
// <copyright file="NotificationEventArgs.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>Custom event arg for notifying (custom notification) update information 
//  to application user</summary>
//-----------------------------------------------------------------------------------


namespace Prakrishta.EasyUpdate
{
    using System;

    /// <summary>
    /// Custom class that contain update event data, and provides a value to use it in update manager
    /// </summary>
    public sealed class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instances of <see cref="NotificationEventArgs"/> class
        /// </summary>
        /// <param name="updater">The application update details</param>
        public NotificationEventArgs(UpdaterInformation updater)
        {
            this.UpdateInfo = updater;
        }

        /// <summary>
        /// Gets update information received from appcast file or database or any other persistance
        /// </summary>
        public UpdaterInformation UpdateInfo { get; }
    }
}
