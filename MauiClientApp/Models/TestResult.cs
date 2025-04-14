using System.Collections.Generic;
using System.ComponentModel;

namespace MauiClientApp.Models
{
    public class TestResult : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public double Score { get; set; }
        public int TimeSpent { get; set; } // in seconds
        public DateTime CompletedOn { get; set; }

        private int _rank;
        public int Rank
        {
            get => _rank;
            set
            {
                if (_rank != value)
                {
                    _rank = value;
                    OnPropertyChanged(nameof(Rank));
                }
            }
        }

        // Helper property to format the time spent in a readable format (mm:ss)
        public string FormattedTimeSpent => TimeSpan.FromSeconds(TimeSpent).ToString(@"mm\:ss");

        // Helper property to format the score as a percentage
        public string FormattedScore => $"{Score:P0}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class QuestionResult
    {
        public int question_id { get; set; }
        public string selected_answer { get; set; }
        public bool is_correct { get; set; }
    }
} 