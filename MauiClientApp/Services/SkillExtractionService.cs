using MauiClientApp.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MauiClientApp.Services
{
    public class SkillExtractionService
    {
        // Predefined lists of common skills
        private readonly HashSet<string> hardSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Programming Languages
            "C#", "Java", "Python", "JavaScript", "TypeScript", "C++", "PHP", "Ruby", "Swift", "Kotlin", "Go",
            "Rust", "Scala", "R", "MATLAB", "Perl", "Objective-C", "VBA", "Bash", "PowerShell", "SQL", "HTML", "CSS",
            
            // Frameworks & Libraries
            ".NET", "ASP.NET", "Entity Framework", "WPF", "MAUI", "Xamarin", "React", "Angular", "Vue.js", "Node.js",
            "Express", "Django", "Flask", "Spring", "Hibernate", "Laravel", "Symfony", "Ruby on Rails", "jQuery",
            "Bootstrap", "TensorFlow", "PyTorch", "Keras", "scikit-learn", "pandas", "NumPy", "Unity",
            
            // Cloud & DevOps
            "AWS", "Azure", "Google Cloud", "Docker", "Kubernetes", "Jenkins", "Git", "GitHub", "GitLab", "CI/CD",
            "Terraform", "Ansible", "Puppet", "Chef", "Vagrant", "Prometheus", "Grafana", "ELK Stack",
            
            // Databases
            "SQL Server", "MySQL", "PostgreSQL", "Oracle", "MongoDB", "Redis", "Cassandra", "DynamoDB", "CosmosDB",
            "SQLite", "Firebase", "Neo4j", "Elasticsearch", "MariaDB",
            
            // Other Technical Skills
            "RESTful API", "GraphQL", "SOAP", "Microservices", "Serverless", "Machine Learning", "AI", "Data Science",
            "Big Data", "Hadoop", "Spark", "Data Mining", "Data Analysis", "Blockchain", "IoT", "Embedded Systems",
            "Cybersecurity", "Network Security", "Penetration Testing", "Cryptography", "Agile", "Scrum", "Kanban",
            "UX/UI Design", "Responsive Design", "Mobile Development", "Web Development", "Game Development"
        };

        private readonly HashSet<string> softSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Communication", "Teamwork", "Problem Solving", "Critical Thinking", "Time Management",
            "Leadership", "Adaptability", "Flexibility", "Creativity", "Work Ethic", "Attention to Detail",
            "Organization", "Collaboration", "Interpersonal Skills", "Conflict Resolution",
            "Emotional Intelligence", "Persuasion", "Negotiation", "Decision Making", "Project Management",
            "Stress Management", "Self-motivation", "Patience", "Empathy", "Active Listening",
            "Analytical Thinking", "Strategic Planning", "Mentoring", "Public Speaking", "Writing",
            "Customer Service", "Multitasking", "Relationship Building", "Cultural Awareness", "Initiative"
        };

        private readonly ApiService _apiService;

        public SkillExtractionService()
        {
            _apiService = new ApiService();
        }

        public async Task<UserSkills> ExtractSkillsFromCv(int userId)
        {
            try
            {
                // Retrieve CV from database
                var cv = await _apiService.GetAsync<CvPdf>($"cv-pdf/{userId}");
                
                if (cv == null || cv.file_data == null || cv.file_data.Length == 0)
                {
                    return new UserSkills { UserId = userId };
                }

                // In a real implementation, we would extract text from the PDF
                // For this demonstration, we'll use a simulated text extraction
                string cvText = await ExtractTextFromPdf(cv);
                
                // Create user skills object
                var userSkills = new UserSkills
                {
                    UserId = userId,
                    HardSkills = ExtractHardSkills(cvText),
                    SoftSkills = ExtractSoftSkills(cvText)
                };

                return userSkills;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting skills: {ex.Message}");
                return new UserSkills { UserId = userId };
            }
        }

        private async Task<string> ExtractTextFromPdf(CvPdf cv)
        {
            // In a real app, you would use a library like iText, PDFBox (via a binding),
            // or call a PDF text extraction API
            
            // For this demo, we'll use a simulated CV text with common skills
            return @"
                Professional Summary
                Experienced software developer with a strong background in C# and .NET development.
                Skilled in developing web applications using ASP.NET Core and React.
                Proficient in SQL Server database design and optimization.

                Technical Skills
                Programming Languages: C#, JavaScript, TypeScript, HTML, CSS, SQL
                Frameworks & Libraries: ASP.NET Core, Entity Framework, React, Angular, Bootstrap
                Tools & Platforms: Visual Studio, VS Code, Azure, Git, GitHub
                Databases: SQL Server, MongoDB
                
                Work Experience
                Senior Developer at Tech Company
                Led a team of 5 developers, showing strong leadership and communication skills
                Implemented CI/CD pipelines using Azure DevOps, demonstrating problem-solving abilities
                Collaborated with UX designers to improve user experience
                
                Soft Skills
                Excellent communication skills both written and verbal
                Strong problem-solving abilities and critical thinking
                Great teamwork and collaboration
                Adaptable to changing priorities
                Time management skills
                Attention to detail
            ";
        }

        private List<string> ExtractHardSkills(string text)
        {
            var extractedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Simple approach: look for matches in our predefined list
            foreach (var skill in hardSkills)
            {
                if (ContainsWord(text, skill))
                {
                    extractedSkills.Add(skill);
                }
            }
            
            return new List<string>(extractedSkills);
        }

        private List<string> ExtractSoftSkills(string text)
        {
            var extractedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Simple approach: look for matches in our predefined list
            foreach (var skill in softSkills)
            {
                if (ContainsWord(text, skill))
                {
                    extractedSkills.Add(skill);
                }
            }
            
            return new List<string>(extractedSkills);
        }

        // Helper method to check if text contains a whole word (not part of another word)
        private bool ContainsWord(string text, string word)
        {
            string pattern = $@"\b{Regex.Escape(word)}\b";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }
    }
} 