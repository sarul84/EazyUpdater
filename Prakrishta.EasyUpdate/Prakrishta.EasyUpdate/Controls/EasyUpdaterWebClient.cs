//----------------------------------------------------------------------------------
// <copyright file="EasyUpdaterWebClient.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/20/2019</date>
// <summary>Custom web Client</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate.Controls
{
    using System;
    using System.Net;

    public class EasyUpdaterWebClient : WebClient
    {
        /// <summary>
        ///  Gets or sets  Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri { get; set; }

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }
    }
}
