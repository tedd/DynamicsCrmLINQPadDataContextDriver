using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using LINQPad.Extensibility.DataContext;
using Tedd.DynamicsCrmLINQPadDataContextDriver.DataConnection;
using Tedd.DynamicsCrmLINQPadDataContextDriver.LINQPad.Astoria;
using Tedd.DynamicsCrmLINQPadDataContextDriver.Utils;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.Views
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        IConnectionInfo _cxInfo;

        public ConnectionDialog(IConnectionInfo cxInfo)
        {
            _cxInfo = cxInfo;
            DataContext = cxInfo.CustomTypeInfo;
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = ConnectButton.IsEnabled = false;
            var progress = new ProgressIndicatorHost(Dispatcher, 3, true);
            try
            {

                string whoami = null;
                var connectionData = ((ViewModels.ConnectionDialogViewModel)DataContext).ConnectionData;
                // Attempt connect (non-blocking)
                Exception failException = null;
                await Task.Factory.StartNew(() =>
                {
                    using (var connection = new XrmConnection())
                    {
                        try
                        {
                            progress.SetStatus(1, "Connecting to CRM-server...");
                            connection.Connect(connectionData);
                            progress.SetStatus(2, "Getting WhoAmI...");
                            whoami = connection.WhoAmI();
                            progress.SetStatus(3, "Done!");
                        }
                        catch (Exception exception)
                        {
                            failException = exception;
                        }
                    }
                });

                if (failException != null)
                {
                    MessageBox.Show("Error connecting to CRM-server:\r\n" + failException.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Handled = true;
                    return;
                }

                //
                if (whoami == null)
                    whoami = "Something went wrong. Unable to retrieve WhoAmI fullname. We'll try to pretend everything is ok.";

                // Display result
                MessageBox.Show(whoami);

                // Done
                DialogResult = true;
            }
            finally
            {
                CancelButton.IsEnabled = ConnectButton.IsEnabled = true;
                progress.Dispose();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void GoToSupportPage_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void DeleteCache_Click(object sender, RoutedEventArgs e)
        {
            var baseDir = System.IO.Path.GetDirectoryName(typeof(SchemaBuilder).Assembly.Location);
            var codefile = System.IO.Path.Combine(baseDir, ((ViewModels.ConnectionDialogViewModel)DataContext).ConnectionData.CacheEntityModelFile);

            if (System.IO.File.Exists(codefile))
            {
                System.IO.File.Delete(codefile);
                MessageBox.Show("Cache cleared.");
            }
            else
            {
                MessageBox.Show("No cache exists.");
            }
        }
    }
}
