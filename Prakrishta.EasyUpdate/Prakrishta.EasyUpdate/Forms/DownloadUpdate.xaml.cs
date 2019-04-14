//----------------------------------------------------------------------------------
// <copyright file="DownloadUpdate.xaml.cs" company="Prakrishta Technologies">
//     Copyright (c) 2019 Prakrishta Technologies. All rights reserved.
// </copyright>
// <author>Arul Sengottaiyan</author>
// <date>3/19/2019</date>
// <summary>DownloadUpdate.xaml.cs</summary>
//-----------------------------------------------------------------------------------

namespace Prakrishta.EasyUpdate
{
    using Prakrishta.EasyUpdate.Controls;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Security;
    using System.Security.Cryptography;
    using System.Windows;

    /// <summary>
    /// Interaction logic for DownloadUpdate.xaml
    /// </summary>
    public partial class DownloadUpdate : Window
    {
        /// <summary>
        /// Holds the value of eazy update manager object
        /// </summary>
        private readonly EazyUpdateManager _eazyUpdate;

        /// <summary>
        /// Holds the value of temp file name
        /// </summary>
        private string _tempFile;

        /// <summary>
        /// Holds the value of web client object
        /// </summary>
        private EasyUpdaterWebClient _webClient;

        /// <summary>
        /// Holds the value of download start time
        /// </summary>
        private DateTime _startedAt;

        /// <summary>
        /// Intializes a new instances of <see cref="DownloadUpdate"/> class
        /// </summary>
        /// <param name="eazyUpdate">The eazy update manager object</param>
        public DownloadUpdate(EazyUpdateManager eazyUpdate)
        {
            this.SetLanguageDictionary();

            InitializeComponent();

            this._eazyUpdate = eazyUpdate;

            if (this._eazyUpdate.UpdaterInformation.IsMandatoryUpdate && 
                this._eazyUpdate.UpdaterInformation.UpdateMode == EazyUpdateMode.ForcedDownload)
            {
                this.WindowStyle = WindowStyle.None; 
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _webClient = new EasyUpdaterWebClient
            {
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore)
            };

            var uri = new Uri(this._eazyUpdate.UpdaterInformation.DownloadURL);

            if (string.IsNullOrEmpty(this._eazyUpdate.UpdaterOptions.DownloadPath))
            {
                _tempFile = Path.GetTempFileName();
            }
            else
            {
                _tempFile = Path.Combine(this._eazyUpdate.UpdaterOptions.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");
                if (!Directory.Exists(this._eazyUpdate.UpdaterOptions.DownloadPath))
                {
                    Directory.CreateDirectory(this._eazyUpdate.UpdaterOptions.DownloadPath);
                }
            }

            if (this._eazyUpdate.UpdaterOptions.DownloadAuthentication != null)
            {
                _webClient.Headers[HttpRequestHeader.Authorization] = this._eazyUpdate.UpdaterOptions.DownloadAuthentication.ToString();
            }

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;

            _webClient.DownloadFileAsync(uri, _tempFile);
        }

        /// <summary>
        /// Web client file download completed event
        /// </summary>
        /// <param name="sender">The sender object (webclient)</param>
        /// <param name="e">The async completed event arg</param>
        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, e.Error.GetType().ToString(), MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _webClient = null;
                Close();
                return;
            }

            if (!string.IsNullOrEmpty(_eazyUpdate.UpdaterInformation.Checksum))
            {
                if (!CompareChecksum(_tempFile, _eazyUpdate.UpdaterInformation.Checksum))
                {
                    _webClient = null;
                    Close();
                    return;
                }
            }

            string fileName;
            string contentDisposition = _webClient.ResponseHeaders["Content-Disposition"] ?? string.Empty;
            if (string.IsNullOrEmpty(contentDisposition))
            {
                fileName = Path.GetFileName(_webClient.ResponseUri.LocalPath);
            }
            else
            {
                fileName = TryToFindFileName(contentDisposition, "filename=");
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = TryToFindFileName(contentDisposition, "filename*=UTF-8''");
                }
            }

            var tempPath =
                Path.Combine(
                    string.IsNullOrEmpty(_eazyUpdate.UpdaterOptions.DownloadPath) ? Path.GetTempPath() : _eazyUpdate.UpdaterOptions.DownloadPath,
                    fileName);

            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                File.Move(_tempFile, tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                _webClient = null;
                Close();
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Arguments = _eazyUpdate.UpdaterInformation.InstallerArgs?.Replace("%path%",
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
            };

            var extension = Path.GetExtension(tempPath);

            if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    UseShellExecute = false,
                    Arguments = $"/i \"{tempPath}\""
                };
            }
            else if(extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath
                };
            }

            if (!string.IsNullOrEmpty(_eazyUpdate.UpdaterInformation.InstallerArgs))
            {
                processStartInfo.Arguments += " " + _eazyUpdate.UpdaterInformation.InstallerArgs;
            }

            if (_eazyUpdate.UpdaterOptions.RunUpdateAsAdmin)
            {
                processStartInfo.Verb = "runas";
            }

            if (_eazyUpdate.UpdaterOptions.RunUpdateAsAnotherUser && _eazyUpdate.UpdaterOptions.ServiceAccount != null)
            {
                SecureString ssPwd = new SecureString();

                Array.ForEach(_eazyUpdate.UpdaterOptions.ServiceAccount.Password.ToCharArray(),
                    (x) => ssPwd.AppendChar(x));

                ssPwd.MakeReadOnly();

                processStartInfo.Domain = _eazyUpdate.UpdaterOptions.ServiceAccount.Domain;
                processStartInfo.UserName = _eazyUpdate.UpdaterOptions.ServiceAccount.UserName;
                processStartInfo.Password = ssPwd;
            }

            try
            {
                Process.Start(processStartInfo);
            }
            catch (Win32Exception exception)
            {
                _webClient = null;
                if (exception.NativeErrorCode != 1223)
                    throw;
            }

            Close();
        }

        /// <summary>
        /// Backgorund worker progress changed event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Download progress event arg</param>
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_startedAt == default(DateTime))
            {
                _startedAt = DateTime.Now;
            }
            else
            {
                var timeSpan = DateTime.Now - _startedAt;
                long totalSeconds = (long)timeSpan.TotalSeconds;
                if (totalSeconds > 0)
                {
                    var bytesPerSecond = e.BytesReceived / totalSeconds;
                    lblDownloadSpeedMessage.Text =
                        string.Format(ResourceEnabler.GetResourceText("DownloadSpeedMessage"), BytesToString(bytesPerSecond));
                }
            }

            lblSize.Text = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            pgbFileDownloadProgress.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Helper method to convert downloading speed bytes to string
        /// </summary>
        /// <param name="byteCount">The download speed byte</param>
        /// <returns>Converted string</returns>
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

        /// <summary>
        /// Get file name from content disposition
        /// </summary>
        /// <param name="contentDisposition">The content disposition</param>
        /// <param name="lookForFileName">The file name</param>
        /// <returns>File name</returns>
        private static string TryToFindFileName(string contentDisposition, string lookForFileName)
        {
            string fileName = string.Empty;
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                var index = contentDisposition.IndexOf(lookForFileName, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                    fileName = contentDisposition.Substring(index + lookForFileName.Length);
                if (fileName.StartsWith("\""))
                {
                    var file = fileName.Substring(1, fileName.Length - 1);
                    var i = file.IndexOf("\"", StringComparison.CurrentCultureIgnoreCase);
                    if (i != -1)
                    {
                        fileName = file.Substring(0, i);
                    }
                }
            }

            return fileName;
        }

        /// <summary>
        /// Compare check sum of the downloaded update file with the algorithm and check sum given
        /// </summary>
        /// <param name="fileName">The downloaded file name</param>
        /// <param name="checksum">The check sum value</param>
        /// <returns>Returns true if it matches otherwise false</returns>
        private bool CompareChecksum(string fileName, string checksum)
        {
            using (var hashAlgorithm = HashAlgorithm.Create(this._eazyUpdate.UpdaterInformation.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

                        if (fileChecksum == checksum.ToLower()) return true;

                        ActionInvokeEnabler.InvokeIfNecessary(() =>
                            MessageBox.Show(ResourceEnabler.GetResourceText("FileIntegrityCheckFailedMessage"),
                            ResourceEnabler.GetResourceText("FileIntegrityCheckFailedCaption"), 
                            MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                    else
                    {
                        if (this._eazyUpdate.UpdaterOptions.ReportErrors)
                        {
                            ActionInvokeEnabler.InvokeIfNecessary(() =>
                                MessageBox.Show(ResourceEnabler.GetResourceText("UnsupportedHashAlgorithmMessage"),
                                ResourceEnabler.GetResourceText("UnsupportedHashAlgorithmCaption"),
                                MessageBoxButton.OK, MessageBoxImage.Error));
                        }
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Download window closing event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The window closing event arg</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_webClient == null)
            {
                DialogResult = false;
            }
            else if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
                DialogResult = false;
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
