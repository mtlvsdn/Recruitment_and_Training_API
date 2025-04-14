using System.Globalization;
using MauiClientApp.Models;

namespace MauiClientApp.Converters
{
    // Convert a index to a progress percentage (for progress bar)
    public class IndexToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentIndex)
            {
                // The view model needs to expose TotalQuestions or we need another approach
                var totalQuestions = 10; // Default fallback
                
                if (parameter is int total)
                {
                    totalQuestions = total;
                }
                
                // Add 1 to currentIndex because it's 0-based, but we want to show progress starting from 1
                return (double)(currentIndex + 1) / totalQuestions;
            }
            
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Convert an answer letter (A, B, C, D) to a color
    public class AnswerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string selectedAnswer = value as string;
            string thisAnswer = parameter as string;
            
            if (string.IsNullOrEmpty(selectedAnswer))
                return Color.FromArgb("#333333"); // Default color
                
            if (selectedAnswer == thisAnswer)
                return Color.FromArgb("#1E88E5"); // Blue for selected
                
            return Color.FromArgb("#333333"); // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Convert a bool (isCorrect) to a color
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCorrect)
            {
                return isCorrect 
                    ? Color.FromArgb("#4CAF50") // Green for correct
                    : Color.FromArgb("#F44336"); // Red for incorrect
            }
            
            return Color.FromArgb("#333333"); // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Convert a score percentage to a color
    public class ScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string scoreStr && scoreStr.EndsWith("%"))
            {
                if (int.TryParse(scoreStr.TrimEnd('%'), out int score))
                {
                    if (score >= 80)
                        return Color.FromArgb("#4CAF50"); // Green for excellent
                    else if (score >= 60)
                        return Color.FromArgb("#FFC107"); // Yellow for good
                    else
                        return Color.FromArgb("#F44336"); // Red for needs improvement
                }
            }
            
            return Color.FromArgb("#FFFFFF"); // Default white
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Convert an answer letter to the corresponding text
    public class AnswerLetterToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string answerLetter = value as string;
            Question question = parameter as Question;
            
            if (string.IsNullOrEmpty(answerLetter) || question == null)
                return "Not answered";
                
            switch (answerLetter)
            {
                case "A": return question.possible_answer_1;
                case "B": return question.possible_answer_2;
                case "C": return question.possible_answer_3;
                case "D": return question.possible_answer_4;
                default: return "Invalid answer";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 