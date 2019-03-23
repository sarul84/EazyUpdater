//----------------------------------------------------------------------------------
// <copyright file="IEazyUpdateReadWriter.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/22/2019</date>
// <summary>Contract that defines way to persist skip and remind later details</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System.Collections.Generic;

    /// <summary>
    /// Intrface that declares contracts to persist skip and remind later details
    /// </summary>
    public interface IEazyUpdateReadWriter
    {
        /// <summary>
        /// Add or update details to registry as required
        /// </summary>
        /// <param name="registryLocation">The registry location</param>
        /// <param name="registryEntries">Entries to updated or created in registry</param>
        void UpdateRegistry(string registryLocation, IDictionary<string, object> registryEntries);

        /// <summary>
        /// Get registry key values as required
        /// </summary>
        /// <param name="registryLocation">The registry location</param>
        /// <param name="keys">The registry keys to be retrieved</param>
        /// <returns>The registry key and value set</returns>
        IDictionary<string, object> GetRegistryValues(string registryLocation, IEnumerable<string> keys);
    }
}
