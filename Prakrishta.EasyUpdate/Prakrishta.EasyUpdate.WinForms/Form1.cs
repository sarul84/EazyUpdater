using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prakrishta.EasyUpdate.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
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
