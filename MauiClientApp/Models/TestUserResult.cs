using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiClientApp.Models
{
    public class TestUserResult : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool HasTakenTest { get; set; }
        
        // Properties available only if the user has taken the test
        public int? Rank { get; set; }
        public double? Score { get; set; }
        public int? TimeSpent { get; set; } // in seconds
        public DateTime? CompletedOn { get; set; }
        
        public string StatusText => HasTakenTest ? "Completed" : "Not attended yet";
        
        // Formatted display properties
        public string FormattedScore => HasTakenTest && Score.HasValue ? $"{Score.Value:P0}" : "-";
        public string FormattedTimeSpent => HasTakenTest && TimeSpent.HasValue ? 
            TimeSpan.FromSeconds(TimeSpent.Value).ToString(@"mm\:ss") : "-";
        public string FormattedRank => HasTakenTest && Rank.HasValue ? $"#{Rank.Value}" : "-";
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 