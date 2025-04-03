using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Services;
using MauiClientApp.Models;
using Microsoft.Maui.ApplicationModel;

namespace MauiClientApp.ViewModels
{
    public partial class UserCvViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private FileResult _selectedFile;
        private byte[] _fileData;
        private CvPdf _existingCv;

        [ObservableProperty]
        private string uploadedFileName = "No file uploaded";

        [ObservableProperty]
        private bool isFileUploaded = false;

        [ObservableProperty]
        private bool hasExistingCv = false;

        public ICommand UploadCvCommand { get; }
        public ICommand DeleteCvCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand SaveToDatabaseCommand { get; }
        public ICommand DownloadCvCommand { get; }
        public ICommand InitializeCommand { get; }

        public UserCvViewModel()
        {
            _apiService = new ApiService();
            UploadCvCommand = new AsyncRelayCommand(UploadCvAsync);
            DeleteCvCommand = new AsyncRelayCommand(DeleteCvAsync);
            GoBackCommand = new AsyncRelayCommand(GoBackAsync);
            SaveToDatabaseCommand = new AsyncRelayCommand(SaveToDatabaseAsync);
            DownloadCvCommand = new AsyncRelayCommand(DownloadCvAsync);
            InitializeCommand = new AsyncRelayCommand(InitializeAsync);
            
            // Call initialize on page load
            Task.Run(() => InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Check if the user has a CV in the database
                _existingCv = await _apiService.GetAsync<CvPdf>($"cv-pdf/{SessionManager.Instance.UserId}");
                
                if (_existingCv != null && !string.IsNullOrEmpty(_existingCv.file_name))
                {
                    // Update UI to show CV is available
                    UploadedFileName = $"Current CV: {_existingCv.file_name}";
                    HasExistingCv = true;
                    IsFileUploaded = true; // Enable delete button
                    _fileData = _existingCv.file_data; // Store the file data
                }
            }
            catch (Exception ex)
            {
                // CV might not exist, which is fine
                Console.WriteLine($"CV not found or error: {ex.Message}");
                HasExistingCv = false;
            }
        }

        private async Task GoBackAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task UploadCvAsync()
        {
            try
            {
                // Request permissions
                var status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert("Permission Required", "Storage permission is required to select PDF files.", "OK");
                    return;
                }

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                        { DevicePlatform.Android, new[] { "application/pdf" } },
                        { DevicePlatform.WinUI, new[] { ".pdf" } },
                        { DevicePlatform.MacCatalyst, new[] { "com.adobe.pdf" } }
                    })
                });

                if (result != null)
                {
                    _selectedFile = result;
                    UploadedFileName = result.FileName;
                    IsFileUploaded = true;

                    // Read the file data
                    using (var stream = await result.OpenReadAsync())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            _fileData = memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error selecting file: {ex.Message}", "OK");
            }
        }

        private async Task DownloadCvAsync()
        {
            if (!HasExistingCv || _fileData == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No CV available to download", "OK");
                return;
            }

            try
            {
                // Create a temporary file
                string tempFilePath = Path.Combine(FileSystem.CacheDirectory, _existingCv.file_name);
                
                // Write the file data to the temporary file
                File.WriteAllBytes(tempFilePath, _fileData);
                
                // Share the file or open it
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Your CV",
                    File = new ShareFile(tempFilePath)
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error downloading CV: {ex.Message}", "OK");
            }
        }

        private async Task SaveToDatabaseAsync()
        {
            if (_selectedFile == null || _fileData == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No file selected", "OK");
                return;
            }

            try
            {
                var cvPdf = new CvPdf
                {
                    cv_id = SessionManager.Instance.UserId,
                    file_name = _selectedFile.FileName,
                    file_size = _fileData.Length, // Store as bytes
                    file_data = _fileData
                };

                Console.WriteLine($"Attempting to upload CV for user ID: {cvPdf.cv_id}");
                Console.WriteLine($"File name: {cvPdf.file_name}");
                Console.WriteLine($"File size: {cvPdf.file_size} bytes");
                Console.WriteLine($"File data length: {cvPdf.file_data.Length} bytes");

                var response = await _apiService.PostAsync<CvPdf>("cv-pdf/upload-json", cvPdf);
                await Application.Current.MainPage.DisplayAlert("Success", "CV uploaded successfully", "OK");
                
                // Update state to reflect that there's now a CV in the database
                _existingCv = cvPdf;
                HasExistingCv = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveToDatabaseAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error uploading CV: {ex.Message}", "OK");
            }
        }

        private async Task DeleteCvAsync()
        {
            if (!IsFileUploaded && !HasExistingCv)
                return;

            try
            {
                // Delete from database
                await _apiService.DeleteAsync($"cv-pdf/{SessionManager.Instance.UserId}");

                // Clear local file data
                _selectedFile = null;
                _fileData = null;
                _existingCv = null;
                UploadedFileName = "No file uploaded";
                IsFileUploaded = false;
                HasExistingCv = false;

                await Application.Current.MainPage.DisplayAlert("Success", "CV deleted successfully", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error deleting CV: {ex.Message}", "OK");
            }
        }
    }
}