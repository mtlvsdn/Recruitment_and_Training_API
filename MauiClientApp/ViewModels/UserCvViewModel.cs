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
using System.Net.Http;
using System.Linq;

namespace MauiClientApp.ViewModels
{
    public partial class UserCvViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly GeminiAIService _geminiService;
        private readonly PdfTextExtractionService _pdfTextExtractionService;
        private FileResult _selectedFile;
        private byte[] _fileData;
        private CvPdf _existingCv;

        [ObservableProperty]
        private string uploadedFileName = "No file uploaded";

        [ObservableProperty]
        private bool isFileUploaded = false;

        [ObservableProperty]
        private bool hasExistingCv = false;

        [ObservableProperty]
        private bool isProcessing = false;

        [ObservableProperty]
        private string processingStatus = string.Empty;

        public ICommand UploadCvCommand { get; }
        public ICommand DeleteCvCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand SaveToDatabaseCommand { get; }
        public ICommand DownloadCvCommand { get; }
        public ICommand InitializeCommand { get; }

        public UserCvViewModel()
        {
            _apiService = new ApiService();
            _geminiService = new GeminiAIService();
            _pdfTextExtractionService = new PdfTextExtractionService();
            
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

            IsProcessing = true;
            ProcessingStatus = "Uploading CV...";

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

                // Upload the CV to the database
                var response = await _apiService.PostAsync<CvPdf>("cv-pdf/upload-json", cvPdf);
                
                // Update state to reflect that there's now a CV in the database
                _existingCv = cvPdf;
                HasExistingCv = true;
                
                // Now process it with Gemini AI
                await ProcessCvWithGeminiAsync(cvPdf);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveToDatabaseAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error uploading CV: {ex.Message}", "OK");
            }
            finally
            {
                IsProcessing = false;
                ProcessingStatus = string.Empty;
            }
        }

        private async Task ProcessCvWithGeminiAsync(CvPdf cvPdf)
        {
            try
            {
                ProcessingStatus = "Extracting text from PDF...";
                
                // Extract text from PDF
                string pdfText;
                try 
                {
                    pdfText = await _pdfTextExtractionService.ExtractTextFromPdf(cvPdf.file_data);
                    Console.WriteLine($"Extracted PDF text of length: {pdfText?.Length ?? 0}");
                    
                    if (string.IsNullOrWhiteSpace(pdfText))
                    {
                        throw new Exception("No text could be extracted from the PDF");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PDF text extraction error: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("PDF Extraction Error", 
                        $"Could not extract text from PDF: {ex.Message}", 
                        "OK");
                    return;
                }
                
                ProcessingStatus = "Analyzing skills with Gemini AI...";
                
                // Use Gemini to extract skills
                string softSkills;
                string hardSkills;
                
                try
                {
                    (softSkills, hardSkills) = await _geminiService.ExtractSkillsFromPdfText(pdfText);
                    
                    // Check if we got meaningful results
                    bool hasSoftSkills = !softSkills.StartsWith("Error:") && !softSkills.StartsWith("API error:");
                    bool hasHardSkills = !hardSkills.StartsWith("Error:") && !hardSkills.StartsWith("API error:");
                    
                    if (!hasSoftSkills && !hasHardSkills)
                    {
                        throw new Exception("Could not extract any skills from your CV. Gemini AI response: " + softSkills);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Gemini AI error: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Skill Extraction Error", 
                        $"Error extracting skills with Gemini AI: {ex.Message}", 
                        "OK");
                    return;
                }
                
                ProcessingStatus = "Saving skills to database...";
                
                try
                {
                    // Use the new direct method instead of going through Entity Framework
                    await _geminiService.SaveSkillsToDatabase(
                        SessionManager.Instance.UserId,
                        cvPdf.cv_id,
                        softSkills,
                        hardSkills
                    );
                    
                    // Create formatted skills display
                    string formattedSoftSkills = FormatSkillsForDisplay(softSkills);
                    string formattedHardSkills = FormatSkillsForDisplay(hardSkills);
                    
                    await Application.Current.MainPage.DisplayAlert("Success", 
                        "CV uploaded and skills extracted successfully!\n\n" +
                        $"Soft Skills:\n{formattedSoftSkills}\n\n" +
                        $"Hard Skills:\n{formattedHardSkills}", 
                        "OK");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Database error: {dbEx.Message}");
                    
                    // Log more details
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    
                    // Show error with more details for debugging
                    await Application.Current.MainPage.DisplayAlert("Database Error", 
                        $"CV was uploaded but there was a database error storing the skills.\n\nDetails: {dbEx.Message}", 
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting skills: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Warning", 
                    "CV was uploaded but there was an error extracting skills: " + ex.Message, 
                    "OK");
            }
        }
        
        private string FormatSkillsForDisplay(string skills)
        {
            if (string.IsNullOrWhiteSpace(skills))
            {
                return "None found";
            }
            
            if (skills.StartsWith("Error:") || skills.StartsWith("API error:"))
            {
                return skills;
            }
            
            // Split by comma and create a bullet list
            return string.Join("\n", skills.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => $"• {s}"));
        }

        private async Task DeleteCvAsync()
        {
            if (!IsFileUploaded && !HasExistingCv)
                return;

            try
            {
                // Delete from database
                await _apiService.DeleteAsync($"cv-pdf/{SessionManager.Instance.UserId}");
                
                // Also delete from CV_Summarised if exists
                try
                {
                    await _apiService.DeleteAsync($"cv-summarised/by-user/{SessionManager.Instance.UserId}");
                }
                catch
                {
                    // Ignore error if CV_Summarised doesn't exist
                }

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