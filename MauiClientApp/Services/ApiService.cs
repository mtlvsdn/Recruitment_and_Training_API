using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MauiClientApp.Services
{
    public class ApiService
    {
#if ANDROID
        private const string _baseUrl = "http://10.0.2.2:7287";
#else
        private const string _baseUrl = "https://localhost:7287";
#endif
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            
            // Configure SSL/TLS
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
#else
            _httpClient = new HttpClient();
#endif

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    Console.WriteLine($"ApiService: Retry {retryCount} of {maxRetries} after error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(1 * retryCount)); // Exponential backoff
                }
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                Console.WriteLine($"ApiService: Making GET request to {_baseUrl}/{endpoint}");
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                Console.WriteLine($"ApiService: Response status code: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ApiService: Response content length: {content.Length} characters");
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ApiService: Error response: {error}");
                
                // Special handling for test endpoints - create mock data for testing
                if (endpoint.StartsWith("test/") && typeof(T).Name == "Test")
                {
                    Console.WriteLine("ApiService: API request failed, using mock test data as fallback");
                    
                    // Create mock test data
                    var mockTest = Activator.CreateInstance<T>();
                    var properties = typeof(T).GetProperties();
                    
                    foreach (var prop in properties)
                    {
                        if (prop.Name == "test_id")
                            prop.SetValue(mockTest, int.TryParse(endpoint.Replace("test/", ""), out int id) ? id : 1);
                        else if (prop.Name == "test_name")
                            prop.SetValue(mockTest, "Mock Test (API Unavailable)");
                        else if (prop.Name == "no_of_questions")
                            prop.SetValue(mockTest, 5);
                        else if (prop.Name == "time_limit")
                            prop.SetValue(mockTest, 10);
                        else if (prop.Name == "company_name")
                            prop.SetValue(mockTest, "Mock Company");
                    }
                    
                    return mockTest;
                }
                
                // For other failures, throw the exception
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {error}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApiService: Error in GetAsync for {endpoint}: {ex.Message}");
                Console.WriteLine($"ApiService: Stack trace: {ex.StackTrace}");
                
                // Special handling for test endpoints when API is completely unavailable
                if (endpoint.StartsWith("test/") && typeof(T).Name == "Test")
                {
                    Console.WriteLine("ApiService: API unavailable, using mock test data as fallback");
                    
                    // Create mock test data
                    var mockTest = Activator.CreateInstance<T>();
                    var properties = typeof(T).GetProperties();
                    
                    foreach (var prop in properties)
                    {
                        if (prop.Name == "test_id")
                            prop.SetValue(mockTest, int.TryParse(endpoint.Replace("test/", ""), out int id) ? id : 1);
                        else if (prop.Name == "test_name")
                            prop.SetValue(mockTest, "Mock Test (API Unavailable)");
                        else if (prop.Name == "no_of_questions")
                            prop.SetValue(mockTest, 5);
                        else if (prop.Name == "time_limit")
                            prop.SetValue(mockTest, 10);
                        else if (prop.Name == "company_name")
                            prop.SetValue(mockTest, "Mock Company");
                    }
                    
                    return mockTest;
                }
                
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Making POST request to {_baseUrl}/{endpoint}");
                Console.WriteLine($"Request body: {json}");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (POST {endpoint}): {ex.Message}");
                throw;
            }
        }

        public async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                try
                {
                    Console.WriteLine($"ApiService: Making GET request to {_baseUrl}/{endpoint}");
                    var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                    Console.WriteLine($"ApiService: Response status code: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"ApiService: Response content length: {content.Length} characters");
                        return JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);
                    }
                    
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ApiService: Error response: {error}");
                    
                    // Special handling for question endpoints - create mock data for testing
                    if (endpoint.StartsWith("questions/test/") && typeof(T).Name == "Question")
                    {
                        Console.WriteLine("ApiService: API request failed, using mock questions data as fallback");
                        
                        var mockQuestions = new List<T>();
                        int testId = int.TryParse(endpoint.Replace("questions/test/", ""), out int id) ? id : 1;
                        
                        // Create 5 mock questions
                        for (int i = 1; i <= 5; i++)
                        {
                            var mockQuestion = Activator.CreateInstance<T>();
                            var properties = typeof(T).GetProperties();
                            
                            foreach (var prop in properties)
                            {
                                if (prop.Name == "question_id")
                                    prop.SetValue(mockQuestion, i);
                                else if (prop.Name == "test_id")
                                    prop.SetValue(mockQuestion, testId);
                                else if (prop.Name == "question_text")
                                    prop.SetValue(mockQuestion, $"Mock Question {i} (API Unavailable)");
                                else if (prop.Name == "possible_answer_1")
                                    prop.SetValue(mockQuestion, "Answer A");
                                else if (prop.Name == "possible_answer_2")
                                    prop.SetValue(mockQuestion, "Answer B");
                                else if (prop.Name == "possible_answer_3")
                                    prop.SetValue(mockQuestion, "Answer C");
                                else if (prop.Name == "possible_answer_4")
                                    prop.SetValue(mockQuestion, "Answer D");
                                else if (prop.Name == "correct_answer")
                                    prop.SetValue(mockQuestion, "Answer A");
                            }
                            
                            mockQuestions.Add(mockQuestion);
                        }
                        
                        return mockQuestions;
                    }
                    
                    // Special handling for user-test endpoints when API is completely unavailable
                    if (endpoint.StartsWith("user-test/") && typeof(T).Name == "UserTest")
                    {
                        Console.WriteLine("ApiService: API unavailable, using mock user-test data as fallback");
                        
                        var mockUserTests = new List<T>();
                        
                        // Extract test or user ID from the endpoint
                        int id = 1;
                        if (endpoint.StartsWith("user-test/user/"))
                            id = int.TryParse(endpoint.Replace("user-test/user/", ""), out int userId) ? userId : 1;
                        else if (endpoint.StartsWith("user-test/test/"))
                            id = int.TryParse(endpoint.Replace("user-test/test/", ""), out int testId) ? testId : 1;
                        
                        // Create a mock user-test assignment
                        var mockUserTest = Activator.CreateInstance<T>();
                        var properties = typeof(T).GetProperties();
                        
                        foreach (var prop in properties)
                        {
                            if (prop.Name == "Userid" && endpoint.StartsWith("user-test/user/"))
                                prop.SetValue(mockUserTest, id);
                            else if (prop.Name == "Userid" && !endpoint.StartsWith("user-test/user/"))
                                prop.SetValue(mockUserTest, 1);
                            else if (prop.Name == "Testtest_id" && endpoint.StartsWith("user-test/test/"))
                                prop.SetValue(mockUserTest, id);
                            else if (prop.Name == "Testtest_id" && !endpoint.StartsWith("user-test/test/"))
                                prop.SetValue(mockUserTest, 1);
                        }
                        
                        mockUserTests.Add(mockUserTest);
                        return mockUserTests;
                    }
                    
                    throw new HttpRequestException($"API request failed: {response.StatusCode} - {error}");
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"ApiService: Request timed out for {endpoint}");
                    
                    // Special handling for question endpoints when API is completely unavailable
                    if (endpoint.StartsWith("questions/test/") && typeof(T).Name == "Question")
                    {
                        Console.WriteLine("ApiService: API unavailable, using mock questions data as fallback");
                        
                        var mockQuestions = new List<T>();
                        int testId = int.TryParse(endpoint.Replace("questions/test/", ""), out int id) ? id : 1;
                        
                        // Create 5 mock questions
                        for (int i = 1; i <= 5; i++)
                        {
                            var mockQuestion = Activator.CreateInstance<T>();
                            var properties = typeof(T).GetProperties();
                            
                            foreach (var prop in properties)
                            {
                                if (prop.Name == "question_id")
                                    prop.SetValue(mockQuestion, i);
                                else if (prop.Name == "test_id")
                                    prop.SetValue(mockQuestion, testId);
                                else if (prop.Name == "question_text")
                                    prop.SetValue(mockQuestion, $"Mock Question {i} (API Unavailable)");
                                else if (prop.Name == "possible_answer_1")
                                    prop.SetValue(mockQuestion, "Answer A");
                                else if (prop.Name == "possible_answer_2")
                                    prop.SetValue(mockQuestion, "Answer B");
                                else if (prop.Name == "possible_answer_3")
                                    prop.SetValue(mockQuestion, "Answer C");
                                else if (prop.Name == "possible_answer_4")
                                    prop.SetValue(mockQuestion, "Answer D");
                                else if (prop.Name == "correct_answer")
                                    prop.SetValue(mockQuestion, "Answer A");
                            }
                            
                            mockQuestions.Add(mockQuestion);
                        }
                        
                        return mockQuestions;
                    }
                    
                    throw new HttpRequestException($"Request timed out: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ApiService: Error in GetListAsync for {endpoint}: {ex.Message}");
                    Console.WriteLine($"ApiService: Stack trace: {ex.StackTrace}");
                    
                    // Special handling for question endpoints when API is completely unavailable
                    if (endpoint.StartsWith("questions/test/") && typeof(T).Name == "Question")
                    {
                        Console.WriteLine("ApiService: API unavailable, using mock questions data as fallback");
                        
                        var mockQuestions = new List<T>();
                        int testId = int.TryParse(endpoint.Replace("questions/test/", ""), out int id) ? id : 1;
                        
                        // Create 5 mock questions
                        for (int i = 1; i <= 5; i++)
                        {
                            var mockQuestion = Activator.CreateInstance<T>();
                            var properties = typeof(T).GetProperties();
                            
                            foreach (var prop in properties)
                            {
                                if (prop.Name == "question_id")
                                    prop.SetValue(mockQuestion, i);
                                else if (prop.Name == "test_id")
                                    prop.SetValue(mockQuestion, testId);
                                else if (prop.Name == "question_text")
                                    prop.SetValue(mockQuestion, $"Mock Question {i} (API Unavailable)");
                                else if (prop.Name == "possible_answer_1")
                                    prop.SetValue(mockQuestion, "Answer A");
                                else if (prop.Name == "possible_answer_2")
                                    prop.SetValue(mockQuestion, "Answer B");
                                else if (prop.Name == "possible_answer_3")
                                    prop.SetValue(mockQuestion, "Answer C");
                                else if (prop.Name == "possible_answer_4")
                                    prop.SetValue(mockQuestion, "Answer D");
                                else if (prop.Name == "correct_answer")
                                    prop.SetValue(mockQuestion, "Answer A");
                            }
                            
                            mockQuestions.Add(mockQuestion);
                        }
                        
                        return mockQuestions;
                    }
                    
                    // Special handling for user-test endpoints when API is completely unavailable
                    if (endpoint.StartsWith("user-test/") && typeof(T).Name == "UserTest")
                    {
                        Console.WriteLine("ApiService: API unavailable, using mock user-test data as fallback");
                        
                        var mockUserTests = new List<T>();
                        
                        // Extract test or user ID from the endpoint
                        int id = 1;
                        if (endpoint.StartsWith("user-test/user/"))
                            id = int.TryParse(endpoint.Replace("user-test/user/", ""), out int userId) ? userId : 1;
                        else if (endpoint.StartsWith("user-test/test/"))
                            id = int.TryParse(endpoint.Replace("user-test/test/", ""), out int testId) ? testId : 1;
                        
                        // Create a mock user-test assignment
                        var mockUserTest = Activator.CreateInstance<T>();
                        var properties = typeof(T).GetProperties();
                        
                        foreach (var prop in properties)
                        {
                            if (prop.Name == "Userid" && endpoint.StartsWith("user-test/user/"))
                                prop.SetValue(mockUserTest, id);
                            else if (prop.Name == "Userid" && !endpoint.StartsWith("user-test/user/"))
                                prop.SetValue(mockUserTest, 1);
                            else if (prop.Name == "Testtest_id" && endpoint.StartsWith("user-test/test/"))
                                prop.SetValue(mockUserTest, id);
                            else if (prop.Name == "Testtest_id" && !endpoint.StartsWith("user-test/test/"))
                                prop.SetValue(mockUserTest, 1);
                        }
                        
                        mockUserTests.Add(mockUserTest);
                        return mockUserTests;
                    }
                    
                    throw;
                }
            });
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"ApiService: Making PUT request to {_baseUrl}/{endpoint}");
                Console.WriteLine($"ApiService: Request body: {json}");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"ApiService: Response status code: {response.StatusCode}");
                Console.WriteLine($"ApiService: Response content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                
                Console.WriteLine($"ApiService: Error response from PUT request to {endpoint}: {response.StatusCode} - {responseContent}");
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (PUT {endpoint}): {ex.Message}");
                Console.WriteLine($"API Error Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<T> PatchAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"ApiService: Making PATCH request to {_baseUrl}/{endpoint}");
                Console.WriteLine($"ApiService: Request body: {json}");
                
                // HttpClient doesn't have a PatchAsync method, so we need to use SendAsync
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/{endpoint}")
                {
                    Content = content
                };
                
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"ApiService: Response status code: {response.StatusCode}");
                Console.WriteLine($"ApiService: Response content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                
                Console.WriteLine($"ApiService: Error response from PATCH request to {endpoint}: {response.StatusCode} - {responseContent}");
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (PATCH {endpoint}): {ex.Message}");
                Console.WriteLine($"API Error Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                Console.WriteLine($"ApiService: Making DELETE request to {_baseUrl}/{endpoint}");
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"ApiService: Delete response status: {response.StatusCode}");
                Console.WriteLine($"ApiService: Delete response content: {content}");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Delete request failed with status {response.StatusCode}: {content}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApiService: Error in DeleteAsync for {endpoint}: {ex.Message}");
                Console.WriteLine($"ApiService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
