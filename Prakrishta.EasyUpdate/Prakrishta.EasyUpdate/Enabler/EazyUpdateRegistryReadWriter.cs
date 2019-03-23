//----------------------------------------------------------------------------------
// <copyright file="EazyUpdateRegistryReadWriter.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/21/2019</date>
// <summary>Enabler class to work with windows registry</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Linq;

    public class EazyUpdateRegistryReadWriter : IEazyUpdateReadWriter
    {
        /// <summary>
        /// Add or update details to windows registry as required
        /// </summary>
        /// <param name="registryLocation">The registry location</param>
        /// <param name="registryEntries">Entries to updated or created in registry</param>
        public void UpdateRegistry(string registryLocation, IDictionary<string, object> registryEntries)
        {
            using (RegistryKey updateKey = Registry.CurrentUser.CreateSubKey(registryLocation))
            {
                if (updateKey != null && registryEntries != null)
                {
                    foreach(var entry in registryEntries)
                    {
                        updateKey.SetValue(entry.Key, entry.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Get registry key values as required
        /// </summary>
        /// <param name="registryLocation">The registry location</param>
        /// <param name="keys">The registry keys to be retrieved</param>
        /// <returns>The registry key and value set</returns>
        public IDictionary<string, object> GetRegistryValues(string registryLocation, IEnumerable<string> keys)
        {
            var returnValue = new Dictionary<string, object>();
            using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(registryLocation))
            {
                if (updateKey != null && keys?.Any() == true)
                {
                    foreach(var key in keys)
                    {
                        returnValue.Add(key, updateKey.GetValue(key));
                    }
                    
                }
            }
            return returnValue;
        }
    }
}
