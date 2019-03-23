//----------------------------------------------------------------------------------
// <copyright file="UpdateMode.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/18/2019</date>
// <summary>Update Mode enum</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    /// <summary>
    ///     Enum representing the effect of Mandatory flag.
    /// </summary>
    public enum EazyUpdateMode
    {
        /// <summary>
        /// In this mode, it shows skip, remind later options
        /// </summary>
        Normal,

        /// <summary>
        /// In this mode, it won't show skip button and show remind later option and update option
        /// </summary>
        NoSkip,

        /// <summary>
        /// In this mode, it won't show close button in addition to Noskip mode behaviour.
        /// </summary>
        Forced,

        /// <summary>
        /// In this mode, it will start downloading and applying update without showing standarad update 
        /// dialog in addition to Forced mode behaviour.
        /// </summary>
        ForcedDownload
    }
}
