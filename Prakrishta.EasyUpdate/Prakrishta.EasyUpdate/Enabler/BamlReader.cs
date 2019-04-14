//----------------------------------------------------------------------------------
// <copyright file="BamlReader.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/19/2019</date>
// <summary>Helper class to read xaml resource file</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Baml2006;
    using System.Windows.Markup;
    using System.Windows.Resources;
    using System.Xaml;

    /// <summary>
    /// Helper class to read xaml resource texts
    /// Credits to: https://www.wpftutorial.net/ReadWPFResourcesFromWinForms.html
    /// </summary>
    public static class BamlReader
    {
        /// <summary>
        /// Load xaml resource stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static object Load(Stream stream)
        {
            var reader = new Baml2006Reader(stream);
            var writer = new XamlObjectWriter(reader.SchemaContext);
            while (reader.Read())
            {
                writer.WriteNode(reader);
            }
            return writer.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceFileName"></param>
        /// <returns></returns>
        public static ResourceDictionary GetResourceDictionary(string resourceFileName)
        {
            try
            {
                StreamResourceInfo steamResourceInfo = System.Windows.Application.GetResourceStream(new Uri($"/Prakrishta.EasyUpdate;component/Resources/{resourceFileName}", UriKind.Relative));
                ResourceDictionary resources = (ResourceDictionary)BamlReader.Load(steamResourceInfo.Stream);
                return resources;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
