using System.ComponentModel;

namespace MauiClientApp.Models
{
    public class UserTest : INotifyPropertyChanged
    {
        private int _userId;
        public int Userid 
        { 
            get => _userId;
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged(nameof(Userid));
                }
            }
        }
        
        private int _testId;
        public int Testtest_id 
        { 
            get => _testId;
            set
            {
                if (_testId != value)
                {
                    _testId = value;
                    OnPropertyChanged(nameof(Testtest_id));
                }
            }
        }
        
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }
        
        private DateTime? _assignedDate;
        public DateTime? AssignedDate
        {
            get => _assignedDate;
            set
            {
                if (_assignedDate != value)
                {
                    _assignedDate = value;
                    OnPropertyChanged(nameof(AssignedDate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 