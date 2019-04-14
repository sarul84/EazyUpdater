//----------------------------------------------------------------------------------
// <copyright file="ActionInvokeEnabler.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/19/2019</date>
// <summary>Helper class to execute Actions</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Threading;
    using System.Windows;

    public static class ActionInvokeEnabler
    {
        /// <summary>
        /// Invoke action safely by checking if thread is dispatcher or not
        /// </summary>
        /// <param name="action"></param>
        public static void InvokeIfNecessary(Action action)
        {
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }
    }
}
