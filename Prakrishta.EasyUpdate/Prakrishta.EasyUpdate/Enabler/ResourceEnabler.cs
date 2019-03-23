//----------------------------------------------------------------------------------
// <copyright file="ResourceEnabler.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/23/2019</date>
// <summary>
//  Helper class for pointing and reading resource file based on current application culture
// </summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Enabler class to handle resource dictionary for different cultures
    /// </summary>
    public static class ResourceEnabler
    {
        /// <summary>
        /// Load and set resource dictionary based on current culture language
        /// </summary>
        /// <param name="window">The system window</param>
        public static void SetLanguageDictionary(this Window window)
        {
            SetLanguageDictionary();
        }

        /// <summary>
        /// Load and set resource dictionary based on current culture language
        /// </summary>
        public static void SetLanguageDictionary()
        {
            ResourceDictionary dictionary = new ResourceDictionary();
            var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
            string resourceFileName = "StringResources.xaml";

            if (currentCulture != "en-US")
            {
                resourceFileName = $"StringResources.{currentCulture}.xaml";
            }

            dictionary.Source = new Uri($"pack://application:,,,/Prakrishta.EasyUpdate;component/Resources/{resourceFileName}");
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

        /// <summary>
        /// Get resource string from resource dictionary for the given key
        /// </summary>
        /// <param name="key">The resource key</param>
        /// <returns>The resource string for the key</returns>
        public static string GetResourceText(string key)
        {
            var resource = Application.Current.FindResource(key);

            return resource == null ? "?" : (string)resource;
        }
    }
}
