using MauiClientApp.Models;

namespace MauiClientApp.Services
{
    public interface ITestService
    {
        Task<Test> GetTestDetailsAsync(int testId);
        Task<List<TestResult>> GetTestResultsAsync(int testId);
        Task<List<UserModel>> GetTestUsersAsync(int testId);
    }
} 