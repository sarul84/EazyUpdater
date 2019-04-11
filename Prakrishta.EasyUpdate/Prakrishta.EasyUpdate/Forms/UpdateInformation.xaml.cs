//----------------------------------------------------------------------------------
// <copyright file="UpdateInformation.xaml.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/19/2019</date>
// <summary>UpdateInformation.xaml.cs</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Interaction logic for UpdateInformation.xaml
    /// </summary>
    public partial class UpdateInformation : Window
    {
        /// <summary>
        /// Constant that has IE root key
        /// </summary>
        private const string InternetExplorerRootKey = @"Software\Microsoft\Internet Explorer";

        /// <summary>
        /// Constant that tells registry location to write emulation key
        /// </summary>
        private const string BrowserEmulationKey = InternetExplorerRootKey + @"\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

        /// <summary>
        /// Holds the value of easy update manager
        /// </summary>
        private readonly EazyUpdateManager _eazyUpdate;

        /// <summary>
        /// Initializes a new instances of <see cref="UpdateInformation"/> class
        /// </summary>
        /// <param name="eazyUpdate">The easy update manager</param>
        public UpdateInformation(EazyUpdateManager eazyUpdate)
        {
            this.SetLanguageDictionary();

            InitializeComponent();

            this._eazyUpdate = eazyUpdate;

            this.SetControlText();

            this.UseLatestIE();

            if (!string.IsNullOrEmpty(this._eazyUpdate.UpdaterInformation.ChangelogURL))
                webBrowser.Navigate(this._eazyUpdate.UpdaterInformation.ChangelogURL);

            if(this._eazyUpdate.UpdaterInformation.IsMandatoryUpdate 
                && this._eazyUpdate.UpdaterInformation.UpdateMode == EazyUpdateMode.ForcedDownload)
            {
                this.WindowStyle = WindowStyle.None;
            }
        }

        /// <summary>
        /// Sets all control texts from source file
        /// </summary>
        private void SetControlText()
        {
            this.Title = string.Format(ResourceEnabler.GetResourceText("UpdateFormTitle"), this._eazyUpdate.UpdaterOptions.ApplicationTitle,
                this._eazyUpdate.UpdaterInformation.CurrentVersion);

            lblNewVersionTitle.Text = string.Format(ResourceEnabler.GetResourceText("UpdateFormNewVersionTitle"), this._eazyUpdate.UpdaterOptions.ApplicationTitle);

            lblDownloadQuestion.Text = string.Format(ResourceEnabler.GetResourceText("UpdateFormDownloadQuestion"), this._eazyUpdate.UpdaterOptions.ApplicationTitle,
                this._eazyUpdate.UpdaterInformation.CurrentVersion, this._eazyUpdate.UpdaterInformation.InstalledVersion);

            lblReleaseNote.Text = ResourceEnabler.GetResourceText("UpdateFormReleaseNoteText");

            lblSkip.Text = ResourceEnabler.GetResourceText("SkipVersionButtonText");

            lblRemindLater.Text = ResourceEnabler.GetResourceText("RemindLaterButtonText");

            lblUpdate.Text = ResourceEnabler.GetResourceText("UpdateButtonText");

            btnRemindLater.Visibility = (this._eazyUpdate.UpdaterInformation.UpdateMode == EazyUpdateMode.Forced ||
                                            this._eazyUpdate.UpdaterInformation.UpdateMode == EazyUpdateMode.ForcedDownload) ? Visibility.Hidden : Visibility.Visible;

            btnSkip.Visibility = this._eazyUpdate.UpdaterInformation.UpdateMode == EazyUpdateMode.NoSkip ? Visibility.Hidden : Visibility.Visible;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (this._eazyUpdate.DownloadUpdate())
            {
                DialogResult = true;
            }
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            this._eazyUpdate.EazyUpdateReadWriter.UpdateRegistry(this._eazyUpdate.RegistryLocation,
                new Dictionary<string, object>
                {
                    { RegistryKeys.Version, this._eazyUpdate.UpdaterInformation.CurrentVersion.ToString() },
                    { RegistryKeys.Skip, 1 }
                });
        }

        private void BtnRemindLater_Click(object sender, RoutedEventArgs e)
        {
            var remindLaterForm = new RemindLaterForm();

            var dialogResult = remindLaterForm.ShowDialog();

            if (dialogResult == true)
            {
                this._eazyUpdate.RemindLaterTimeSpan = remindLaterForm.RemindLaterFormat;
                this._eazyUpdate.RemindLaterAt = remindLaterForm.RemindLaterAt;
            }
            else if (dialogResult == false)
            {
                this.BtnUpdate_Click(sender, e);
                return;
            }
            else
            {
                return;
            }

            DateTime remindLaterDateTime = DateTime.Now;
            switch (this._eazyUpdate.RemindLaterTimeSpan)
            {
                case RemindLater.Days:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromDays(this._eazyUpdate.RemindLaterAt);
                    break;
                case RemindLater.Hours:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromHours(this._eazyUpdate.RemindLaterAt);
                    break;
                case RemindLater.Minutes:
                    remindLaterDateTime = DateTime.Now + TimeSpan.FromMinutes(this._eazyUpdate.RemindLaterAt);
                    break;
            }

            this._eazyUpdate.EazyUpdateReadWriter.UpdateRegistry(this._eazyUpdate.RegistryLocation,
               new Dictionary<string, object>
               {
                    { RegistryKeys.Version, this._eazyUpdate.UpdaterInformation.CurrentVersion.ToString() },
                    { RegistryKeys.Skip, 0 },
                    { RegistryKeys.RemindLater, remindLaterDateTime
                                                    .ToString(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat) }
               });

            this._eazyUpdate.SetTimer(remindLaterDateTime);
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._eazyUpdate.IsUpdaterRunning = false;
        }

        private void UseLatestIE()
        {
            int ieValue = this.GetInternetExplorerMajorVersion();
            BrowserEmulationVersion result = BrowserEmulationVersion.Default;

            switch (ieValue)
            {
                case 11:
                    result = BrowserEmulationVersion.Version11;
                    break;
                case 10:
                    result = BrowserEmulationVersion.Version10;
                    break;
                case 9:
                    result = BrowserEmulationVersion.Version9;
                    break;
                case 8:
                    result = BrowserEmulationVersion.Version8;
                    break;
                default:
                    result = BrowserEmulationVersion.Version7;
                    break;
            }

            if (ieValue != 0)
            {
                using (RegistryKey registryKey =
                    Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true))
                {
                    registryKey?.SetValue(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName), (int)result,
                        RegistryValueKind.DWord);
                }
            }
        }

        private int GetInternetExplorerMajorVersion()
        {
            int result = 0;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey);

            if (key != null)
            {
                object value;

                value = key.GetValue(RegistryKeys.LatestBrowserVersion, null) ?? key.GetValue(RegistryKeys.BrowserVersion, null);

                if (value != null)
                {
                    string version = value.ToString();
                    int separator = version.IndexOf('.');

                    if (separator != -1)
                    {
                        int.TryParse(version.Substring(0, separator), out result);
                    }
                }
            }

            return result;
        }
    }
}
