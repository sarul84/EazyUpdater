//----------------------------------------------------------------------------------
// <copyright file="Authentication.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/18/2019</date>
// <summary>Class that has basic authentication header for web request.</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Text;

    /// <summary>
    /// Provides Basic Authentication header for web request.
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// Holds the value of user name variable
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Holds the value of password
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of <see cref="Authentication.cs"/> class
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Authentication(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            return $"Basic {token}";
        }
    }
}
