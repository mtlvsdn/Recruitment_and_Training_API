using System.Net.Http.Json;
using MauiClientApp.Models;

namespace MauiClientApp.Services
{
    public class TestService : ITestService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TestService(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<Test> GetTestDetailsAsync(int testId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tests/{testId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Test>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting test details: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TestResult>> GetTestResultsAsync(int testId)
        {
            try
            {
                Console.WriteLine($"TestService: Getting test results for testId={testId}");
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tests/{testId}/results");
                
                // Handle 404 Not Found gracefully - it just means no results exist yet
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("TestService: No results found (404), returning empty list");
                    return new List<TestResult>(); // Return empty list instead of throwing an exception
                }
                
                // For other error codes, ensure success and throw as usual
                response.EnsureSuccessStatusCode();
                var results = await response.Content.ReadFromJsonAsync<List<TestResult>>();
                Console.WriteLine($"TestService: Successfully retrieved {results?.Count ?? 0} results");
                return results ?? new List<TestResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting test results: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserModel>> GetTestUsersAsync(int testId)
        {
            try
            {
                Console.WriteLine($"TestService: Getting users for testId={testId}");
                
                // First get the user-test associations from the correct endpoint
                var response = await _httpClient.GetAsync($"{_baseUrl}/user-test/test/{testId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("TestService: No user-test associations found (404), returning empty list");
                    return new List<UserModel>();
                }
                
                response.EnsureSuccessStatusCode();
                var userTestAssociations = await response.Content.ReadFromJsonAsync<List<UserTest>>();
                
                if (userTestAssociations == null || !userTestAssociations.Any())
                {
                    Console.WriteLine("TestService: No user-test associations found, returning empty list");
                    return new List<UserModel>();
                }
                
                Console.WriteLine($"TestService: Found {userTestAssociations.Count} user-test associations");
                
                // Now fetch details for each user
                var users = new List<UserModel>();
                foreach (var association in userTestAssociations)
                {
                    try {
                        var userResponse = await _httpClient.GetAsync($"{_baseUrl}/user/{association.Userid}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var user = await userResponse.Content.ReadFromJsonAsync<UserModel>();
                            if (user != null)
                            {
                                users.Add(user);
                                Console.WriteLine($"TestService: Added user {user.Full_Name} to results");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"TestService: Failed to get user {association.Userid}, Status: {userResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"TestService: Error fetching user {association.Userid}: {ex.Message}");
                    }
                }
                
                Console.WriteLine($"TestService: Successfully retrieved {users.Count} users");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting test users: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 