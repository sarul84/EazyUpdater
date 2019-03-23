//----------------------------------------------------------------------------------
// <copyright file="ParseUpdateInfoEventArgs.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>Event arg to parse update info different way than current implementation in
//  Eazy update</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;

    /// <summary>
    /// An object of this class contains the AppCast file received from server.
    /// </summary>
    public sealed class ParseUpdateInfoEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instances of <see cref="ParseUpdateInfoEventArgs"/> class
        /// </summary>
        /// <param name="parsedData">The parsed text from appcast file</param>
        public ParseUpdateInfoEventArgs(string  parsedData)
        {
            this.ParseText = parsedData;
        }

        /// <summary>
        /// Gets parsed text from appcast file
        /// </summary>
        public string ParseText { get; }

        /// <summary>
        /// Gets or sets updater information
        /// </summary>
        public UpdaterInformation UpdateInfo { get; set; }
    }
}
