//----------------------------------------------------------------------------------
// <copyright file="EazyUpdater.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/18/2019</date>
// <summary>
// Main class that lets user auto update applications by setting some mandatory fields 
// and executing its Start method.
// </summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Xml;

    /// <summary>
    /// Easy update manager
    /// </summary>
    public class EazyUpdateManager : IEazyUpdateManager, IDisposable
    {
        #region |Private Fields|

        /// <summary>
        /// Holds the value of the remind later timer object
        /// </summary>
        private System.Timers.Timer _remindLaterTimer;

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool disposedValue = false;
        #endregion

        #region |Constructor|
        /// <summary>
        /// Initializes a new instances of <see cref="EazyUpdateManager"/> class
        /// </summary>
        public EazyUpdateManager() : this(new EazyUpdateRegistryReadWriter())
        {
        }

        /// <summary>
        /// Initializes a new instances of <see cref="EazyUpdateManager"/> class
        /// </summary>
        /// <param name="updateReadWriter">Eazy update read writer object</param>
        public EazyUpdateManager(IEazyUpdateReadWriter updateReadWriter)
        {
            this.EazyUpdateReadWriter = updateReadWriter;
        }
        #endregion

        #region |Public and Internal members|

        /// <inheritdoc />
        public event EventHandler<NotificationEventArgs> CustomNotification;

        /// <inheritdoc />
        public event EventHandler<ParseUpdateInfoEventArgs> ParseUpdateInformation;

        /// <inheritdoc />
        public event EventHandler<UpdateEventArgs> CheckForUpdateInformation;

        /// <inheritdoc />
        public event EventHandler ApplicationExit;

        /// <summary>
        /// Sets read writer object to persist skip and remind later information
        /// </summary>
        public readonly IEazyUpdateReadWriter EazyUpdateReadWriter;

        /// <summary>
        /// Gets or sets updaterinformtation object
        /// </summary>
        internal UpdaterInformation UpdaterInformation { get; private set; }

        /// <summary>
        /// Gets or sets updater options object
        /// </summary>
        internal EazyUpdaterOptions UpdaterOptions { get; private set; }

        /// <summary>
        /// Gets or sets Appcast URL
        /// </summary>
        internal string AppcastUrl { get; private set; }

        /// <summary>
        /// Gets or sets registry location
        /// </summary>
        internal string RegistryLocation { get; private set; }

        /// <summary>
        /// Gets or sets if windows forms application
        /// </summary>
        internal bool IsWinFormsApplication { get; private set; }

        /// <summary>
        /// Gets or sets if the updater is running or not
        /// </summary>
        internal bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets Remind later time span
        /// </summary>
        internal RemindLater RemindLaterTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets Remind later at
        /// </summary>
        internal int RemindLaterAt { get; set; }
        #endregion

        #region |Interface methods implementation|
        /// <inheritdoc />
        public async Task Update(string appcastUrl, Assembly assembly = null)
        {
            await this.Update(appcastUrl, new EazyUpdaterOptions(), assembly);
        }

        /// <inheritdoc />
        public async Task Update(string appcastUrl, EazyUpdaterOptions updaterOptions, Assembly assembly = null)
        {
            this.SetSecurityProtocol();
            this.AppcastUrl = appcastUrl;
            this.UpdaterOptions = updaterOptions;

            if(this.IsRunning && this._remindLaterTimer == null)
            {
                this.IsRunning = true;

                this.IsWinFormsApplication = System.Windows.Forms.Application.MessageLoop;
                ResourceEnabler.SetLanguageDictionary();
            }

            if (!string.IsNullOrEmpty(this.AppcastUrl))
            {
                this.UpdaterInformation = await this.GetUpdateDetilsFromAppCast();
            }
            else if(string.IsNullOrEmpty(this.AppcastUrl) && CheckForUpdateInformation != null)
            {
                #region |Invoke custom implementation|
                var updateArg = new UpdateEventArgs{ Options = this.UpdaterOptions };
                CheckForUpdateInformation?.Invoke(this, updateArg);
                this.UpdaterInformation = updateArg.UpdateInfo;
                this.UpdaterOptions = updateArg.Options ?? new EazyUpdaterOptions();
                assembly = updateArg.AssemblyInfo;
                #endregion
            }
            else
            {
                throw new NotImplementedException("There is no implementation or source to get latest update information");
            }

            this.GetAssemblyInformation(assembly ?? Assembly.GetEntryAssembly());
            var updateTask = this.RunApplicationUpdate();
            updateTask.Wait();
            this.IsRunning = false;
        }

        /// <inheritdoc />
        public bool DownloadUpdate()
        {
            var downloadDialog = new DownloadUpdate(this);

            try
            {
                return downloadDialog.ShowDialog() == true;
            }
            catch (TargetInvocationException)
            {
            }

            return false;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region |Helper methods|

        /// <summary>
        /// Run application update with current update information
        /// </summary>
        /// <returns>Empty Task object that can be awaited</returns>
        private Task RunApplicationUpdate()
        {
            if(this.UpdaterInformation.CurrentVersion == null 
                || string.IsNullOrEmpty(this.UpdaterInformation.DownloadURL))
            {
                this.IsRunning = false;
                throw new InvalidDataException();
            }

            if(!this.UpdaterInformation.IsMandatoryUpdate)
            {
                var registryValues = this.EazyUpdateReadWriter.GetRegistryValues(this.RegistryLocation, 
                    new Collection<string> { RegistryKeys.Skip, RegistryKeys.Version, RegistryKeys.RemindLater });

                if(registryValues?.Any() == true)
                {
                    if (registryValues[RegistryKeys.Skip] != null && registryValues[RegistryKeys.Version] != null)
                    {
                        string skipValue = registryValues[RegistryKeys.Skip].ToString();
                        var skipVersion = new Version(registryValues[RegistryKeys.Version].ToString());
                        if (skipValue.Equals("1") && this.UpdaterInformation.CurrentVersion <= skipVersion)
                        {
                            return null;
                        }

                        if (this.UpdaterInformation.CurrentVersion > skipVersion)
                        {
                            var newRegistryValue = new Dictionary<string, object>
                        {
                            { RegistryKeys.Version, this.UpdaterInformation.CurrentVersion.ToString() },
                            { RegistryKeys.Skip, 0 }
                        };

                            this.EazyUpdateReadWriter.UpdateRegistry(this.RegistryLocation, newRegistryValue);
                        }
                    }

                    var remindLaterTime = registryValues?[RegistryKeys.RemindLater];

                    if (remindLaterTime != null)
                    {
                        DateTime remindLater = Convert.ToDateTime(remindLaterTime.ToString(),
                            CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);

                        int compareResult = DateTime.Compare(DateTime.Now, remindLater);

                        if (compareResult < 0)
                        {
                            this.SetTimer(remindLater);
                            return null;
                        }
                    }
                }                
            }

            if(this.CustomNotification != null)
            {
                this.CustomNotification?.Invoke(this, new NotificationEventArgs(this.UpdaterInformation));
            }
            else
            {
                if(this.UpdaterInformation != null)
                {
                    if(this.UpdaterInformation.IsUpdateAvailable)
                    {
                        if (!this.IsWinFormsApplication)
                        {
                            System.Windows.Forms.Application.EnableVisualStyles();
                        }

                        if(this.UpdaterInformation.IsMandatoryUpdate 
                            && this.UpdaterInformation.UpdateMode == EazyUpdateMode.ForcedDownload)
                        {
                            this.DownloadUpdate();
                            this.Exit();
                        }
                        else
                        {
                            if (Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
                            {
                                this.ShowUpdateForm();
                            }
                            else
                            {
                                Thread thread = new Thread(this.ShowUpdateForm);
                                thread.CurrentCulture = thread.CurrentUICulture = CultureInfo.CurrentCulture;
                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();
                                thread.Join();
                            }
                        }

                        return null;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(ResourceEnabler.GetResourceText("UpdateUnavailableMessage"),
                                    ResourceEnabler.GetResourceText("UpdateUnavailableCaption"),
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(ResourceEnabler.GetResourceText("UpdateCheckFailedMessage"),
                                   ResourceEnabler.GetResourceText("UpdateCheckFailedCaption"),
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            this.IsRunning = false;
            return null;
        }

        /// <summary>
        /// Show update information window
        /// </summary>
        private void ShowUpdateForm()
        {
            var updateForm = new UpdateInformation(this);
            var result = updateForm.ShowDialog();

            if (result == true)
            {
                Exit();
            }
        }

        /// <summary>
        /// Detects and exits all instances of running assembly, including current.
        /// </summary>
        private void Exit()
        {
            if (this.ApplicationExit != null)
            {
                ApplicationExit?.Invoke(this, new EventArgs());
            }
            else
            {
                var currentProcess = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    string processPath;
                    try
                    {
                        processPath = process.MainModule.FileName;
                    }
                    catch (Win32Exception)
                    {
                        continue;
                    }

                    if (process.Id != currentProcess.Id &&
                        currentProcess.MainModule.FileName == processPath
                    )
                    {
                        if (process.CloseMainWindow())
                        {
                            process.WaitForExit((int)TimeSpan.FromSeconds(10)
                                .TotalMilliseconds);
                        }

                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                }

                if (IsWinFormsApplication)
                {
                    MethodInvoker methodInvoker = System.Windows.Forms.Application.Exit;
                    methodInvoker.Invoke();
                }
#if NETWPF
                else if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        System.Windows.Application.Current.Shutdown()));
                }
#endif
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Initiates and starts remind later timer
        /// </summary>
        /// <param name="remindLater"></param>
        internal void SetTimer(DateTime remindLater)
        {
            TimeSpan timeSpan = remindLater - DateTime.Now;

            var context = SynchronizationContext.Current;

            _remindLaterTimer = new System.Timers.Timer
            {
                Interval = (int)timeSpan.TotalMilliseconds,
                AutoReset = false
            };

            _remindLaterTimer.Elapsed += delegate
            {
                _remindLaterTimer = null;

                if (context != null)
                {
                    try
                    {
                        context.Send(state =>
                        {
                            Task.Run(async () => { await this.Update(this.AppcastUrl, new EazyUpdaterOptions()); });

                        }, null);
                    }
                    catch (InvalidAsynchronousStateException)
                    {
                        Task.Run(async () => { await this.Update(this.AppcastUrl, new EazyUpdaterOptions()); });
                    }
                }
                else
                {
                    Task.Run(async () => { await this.Update(this.AppcastUrl, new EazyUpdaterOptions()); });
                }
            };

            _remindLaterTimer.Start();
        }

        /// <summary>
        /// Retrieve update information from appcast file
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<UpdaterInformation> GetUpdateDetilsFromAppCast(CancellationToken token = default(CancellationToken))
        {
            if(token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            var webRequest = WebRequest.Create(this.AppcastUrl);

            if (this.UpdaterOptions.AppcastFileAuthentication != null)
            {
                webRequest.Headers[HttpRequestHeader.Authorization] = this.UpdaterOptions.AppcastFileAuthentication.ToString();
            }

            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            WebResponse webResponse;

            try
            {
                webResponse = await webRequest.GetResponseAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }

            UpdaterInformation updater;

            using (Stream appCastStream = webResponse?.GetResponseStream())
            {
                if(token.IsCancellationRequested)
                {
                    webResponse?.Close();
                    throw new TaskCanceledException();
                }

                if (appCastStream != null)
                {
                    if (this.ParseUpdateInformation != null)
                    {
                        using (StreamReader streamReader = new StreamReader(appCastStream))
                        {
                            string data = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                            ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(data);

                            this.ParseUpdateInformation.Invoke(this, parseArgs);

                            updater = parseArgs.UpdateInfo;
                        }
                    }
                    else
                    {
                        XmlDocument receivedAppCastDocument = new XmlDocument();

                        try
                        {
                            receivedAppCastDocument.Load(appCastStream);

                            XmlNodeList appCastItems = receivedAppCastDocument.SelectNodes("item");

                            updater = new UpdaterInformation();

                            if (appCastItems != null)
                            {
                                foreach (XmlNode item in appCastItems)
                                {
                                    XmlNode appCastVersion = item.SelectSingleNode("version");

                                    try
                                    {
                                        updater.CurrentVersion = new Version(appCastVersion?.InnerText);
                                    }
                                    catch (Exception)
                                    {
                                        updater.CurrentVersion = null;
                                    }

                                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");
                                    updater.ChangelogURL = appCastChangeLog?.InnerText;

                                    XmlNode appCastUrl = item.SelectSingleNode("url");
                                    updater.DownloadURL = appCastUrl?.InnerText;

                                    XmlNode umode = item.SelectSingleNode("updatemode");

                                    var updateMode = (EazyUpdateMode)Enum.Parse(typeof(EazyUpdateMode), umode?.InnerText);
                                    if (!Enum.IsDefined(typeof(EazyUpdateMode), updateMode))
                                    {
                                        throw new InvalidDataException(
                                            $"{updateMode} is not an underlying value of the Mode enumeration.");
                                    }

                                    updater.UpdateMode = updateMode;

                                    XmlNode appArgs = item.SelectSingleNode("args");
                                    updater.InstallerArgs = appArgs?.InnerText;

                                    XmlNode checksum = item.SelectSingleNode("checksum");
                                    updater.HashingAlgorithm = checksum?.Attributes["algorithm"]?.InnerText;
                                    updater.Checksum = checksum?.InnerText;
                                }
                            }
                        }
                        catch (XmlException)
                        {
                            webResponse?.Close();
                            return null;
                        }
                    }
                }
                else
                {
                    webResponse?.Close();
                    return null;
                }
            }

            webResponse?.Close();
            return updater;
        }

        /// <summary>
        /// Set security protocol objects
        /// </summary>
        private void SetSecurityProtocol()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)192 |
                                                        (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            }
            catch (NotSupportedException) { }

            if (this.UpdaterInformation?.IsMandatoryUpdate == true 
                && _remindLaterTimer != null)
            {
                _remindLaterTimer.Stop();
                _remindLaterTimer.Close();
                _remindLaterTimer = null;
            }
        }

        /// <summary>
        /// Get assembly version, application name, company name and compute registry location & installed version details
        /// </summary>
        /// <param name="mainAssembly">The current application assembly details</param>
        private void GetAssemblyInformation(Assembly mainAssembly)
        {
            var companyAttribute =
                (AssemblyCompanyAttribute)GetAttribute(mainAssembly, typeof(AssemblyCompanyAttribute));

            if (string.IsNullOrEmpty(this.UpdaterOptions?.ApplicationTitle))
            {
                var titleAttribute =
                    (AssemblyTitleAttribute)GetAttribute(mainAssembly, typeof(AssemblyTitleAttribute));
                this.UpdaterOptions.ApplicationTitle = titleAttribute != null ? titleAttribute.Title : mainAssembly.GetName().Name;
            }

            string appCompany = companyAttribute != null ? companyAttribute.Company : "";

            RegistryLocation = !string.IsNullOrEmpty(appCompany)
                ? $@"Software\{appCompany}\{this.UpdaterOptions?.ApplicationTitle}\EazyUpdate"
                : $@"Software\{this.UpdaterOptions?.ApplicationTitle}\EazyUpdate";

            this.UpdaterInformation.InstalledVersion = mainAssembly.GetName().Version;
        }

        /// <summary>
        /// Get attribute value from assembly
        /// </summary>
        /// <param name="assembly">The application assembly details</param>
        /// <param name="attributeType">The attribute type to be retrieved</param>
        /// <returns>Attribute object</returns>
        private Attribute GetAttribute(Assembly assembly, Type attributeType)
        {
            object[] attributes = assembly.GetCustomAttributes(attributeType, false);
            if (attributes.Length == 0)
            {
                return null;
            }

            return (Attribute)attributes[0];
        }

        /// <summary>
        /// Dispose of this class
        /// </summary>
        /// <param name="disposing">True if we are disposing</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.UpdaterInformation = null;
                    this.UpdaterOptions = null;
                }

                this.disposedValue = true;
            }
        }
        #endregion
    }
}
