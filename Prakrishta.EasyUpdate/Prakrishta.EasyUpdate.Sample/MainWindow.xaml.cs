using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prakrishta.EasyUpdate.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetEntryAssembly();
            
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EazyUpdateManager updateManager = new EazyUpdateManager();
            updateManager.CheckForUpdateInformation += UpdateManager_CheckForUpdateInformation;
            Task.Run(async () =>
                await updateManager.Update(string.Empty, null));
        }

        private void UpdateManager_CheckForUpdateInformation(object sender, UpdateEventArgs e)
        {
            var update = new UpdaterInformation
            {
                CurrentVersion = new Version("1.0.0.0"),
                DownloadURL = "http://rbsoft.org/downloads/AutoUpdaterTestWPF.zip",
                ChangelogURL = "https://github.com/ravibpatel/AutoUpdater.NET/releases",
                UpdateMode = EazyUpdateMode.Normal
            };
            e.UpdateInfo = update;
            e.Options = new EazyUpdaterOptions();
            e.AssemblyInfo = Assembly.GetExecutingAssembly();
        }
    }
}
