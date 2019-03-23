using System.Linq;
using System.Windows;

namespace Prakrishta.EasyUpdate
{
    /// <summary>
    /// Interaction logic for RemindLaterForm.xaml
    /// </summary>
    public partial class RemindLaterForm : Window
    {
        /// <summary>
        /// Initializes a new instances of <see cref="RemindLaterForm"/> class
        /// </summary>
        public RemindLaterForm()
        {
            this.SetLanguageDictionary();
            InitializeComponent();

            this.SetControlText();

            var remindLaterList = ResourceEnabler.GetResourceText("RemindLaterList");
            var list = remindLaterList.Split(new char[] { ',' }).ToList();
            cbxRemindLater.ItemsSource = list;

            cbxRemindLater.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets or sets Remind later format
        /// </summary>
        public RemindLater RemindLaterFormat { get; private set; }

        /// <summary>
        /// Gets or sets Remind later at
        /// </summary>
        public int RemindLaterAt { get; private set; }

        /// <summary>
        /// Sets all control texts from source file
        /// </summary>
        private void SetControlText()
        {
            this.Title = ResourceEnabler.GetResourceText("RemindLaterFormTitle");
            this.lblUpdateQuestion.Text = ResourceEnabler.GetResourceText("RemindLaterFormUpdateQuestion");
            this.lblWarning.Text= ResourceEnabler.GetResourceText("UpdateRecommendationWarning");
            this.lblOkButton.Text = ResourceEnabler.GetResourceText("OkButtonText");
            this.rdoYesRemindLater.Content = ResourceEnabler.GetResourceText("UpdateLaterRadioButtonContent");
            this.rdoNoDownload.Content = ResourceEnabler.GetResourceText("DownloadNowRadioButtonContent");
        }

        /// <summary>
        /// Ok button click event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The routed event args</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (rdoYesRemindLater.IsChecked == true)
            {
                switch (cbxRemindLater.SelectedIndex)
                {
                    case 0:
                        RemindLaterFormat = RemindLater.Minutes;
                        RemindLaterAt = 30;
                        break;
                    case 1:
                        RemindLaterFormat = RemindLater.Hours;
                        RemindLaterAt = 6;
                        break;
                    case 2:
                        RemindLaterFormat = RemindLater.Hours;
                        RemindLaterAt = 12;
                        break;
                    case 3:
                        RemindLaterFormat = RemindLater.Days;
                        RemindLaterAt = 1;
                        break;
                    case 4:
                        RemindLaterFormat = RemindLater.Days;
                        RemindLaterAt = 2;
                        break;
                    case 5:
                        RemindLaterFormat = RemindLater.Days;
                        RemindLaterAt = 4;
                        break;
                    case 6:
                        RemindLaterFormat = RemindLater.Days;
                        RemindLaterAt = 8;
                        break;
                }

                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
        }
    }
}
