using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Win32.UI.Controls.Dialogs;
using WinUIEx;

namespace CertificateInstallerForMSIX
{
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            this.SetIcon($"{AppContext.BaseDirectory}\\Assets\\Certificate.ico");
            this.Move(50, 50);
        }

        public static class FilePicker
        {
            public static unsafe string ShowDialog(string startingDirectory, string[] filters, string filterName, string dialogTitle)
            {
                var ofn = new OPENFILENAMEW();
                ofn.lStructSize = (uint)Marshal.SizeOf(ofn);

                // Build full filter string
                string filterPattern = string.Join(";", filters.Select(f => $"*.{f}"));
                string fullFilter = $"{filterName}\0{filterPattern}\0All Files (*.*)\0*.*\0\0";

                // Allocate unmanaged memory for filter string
                IntPtr filterPtr = Marshal.StringToHGlobalUni(fullFilter);
                ofn.lpstrFilter = (char*)filterPtr;

                // Allocate buffer for filename
                const int maxFileLength = 1024;
                var fileNameBuffer = (char*)Marshal.AllocHGlobal(maxFileLength * sizeof(char));
                fileNameBuffer[0] = '\0'; // Initialize empty
                ofn.lpstrFile = fileNameBuffer;
                ofn.nMaxFile = maxFileLength;

                // Optional: Set dialog title
                IntPtr titlePtr = Marshal.StringToHGlobalUni(dialogTitle);
                ofn.lpstrTitle = (char*)titlePtr;

                // Call dialog
                string result = string.Empty;
                if (Windows.Win32.PInvoke.GetOpenFileName(ref ofn))
                {
                    result = new string(ofn.lpstrFile);
                }

                // Cleanup
                Marshal.FreeHGlobal((IntPtr)filterPtr);
                Marshal.FreeHGlobal((IntPtr)fileNameBuffer);
                Marshal.FreeHGlobal(titlePtr);

                return result;
            }
        }

        [RelayCommand]
        private async Task GetCertificateAsync()
        {
            try
            {
                // Create a file picker
                var fileName = FilePicker.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), new string[] { "cer" }, "Certificate", "Open");

                if (fileName != null)
                {
                    PathBox.Text = fileName;
                }
                else
                {
                    // Handle case when no file is picked
                }
            }
            catch (COMException ex)
            {
                // Handle the COM exception
                await ShowMessageDialogAsync($"Error: {ex.Message}", "Exception");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                await ShowMessageDialogAsync($"Error: {ex.Message}", "Exception");
            }
        }

        [RelayCommand]
        private void InstallCertificate()
        {
            try
            {
                // Load the certificate from file
                X509Certificate2 certificate = new(PathBox.Text, Password.Password);

                // Define the store location and name
                StoreLocation storeLocation = StoreLocation.LocalMachine;
                StoreName storeName = StoreName.Root;

                // Open the certificate store
                using (X509Store store = new(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);

                    // Add the certificate to the store
                    store.Add(certificate);

                    // Close the store
                    store.Close();
                }

                Close();
            }
            catch
            {

            }
        }
    }
}
