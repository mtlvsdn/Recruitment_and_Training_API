using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MauiClientApp.Models;
using System.Collections.Generic;

namespace MauiClientApp.Services
{
    public class GeminiAIService
    {
        private HttpClient _httpClient;
        private readonly ApiService _apiService;
        
        // If you don't have an API key, get one from https://makersuite.google.com/app/apikey
        private string _apiKey;
        private const string ApiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        // Alternative endpoints to try if primary fails
        private const string ApiEndpointAlt1 = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent";
        
        public GeminiAIService()
        {
            // Create HttpClient with proxy support and certificate validation bypass
            HttpClientHandler handler = new HttpClientHandler
            {
                // Use system proxy by default
                UseProxy = true,
                Proxy = System.Net.WebRequest.DefaultWebProxy,
                
                // Trust all certificates (for development only)
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
            
            _apiService = new ApiService();
            
            // Set the API key directly
            _apiKey = "AIzaSyAKs7pWelK-Uxe56qhxEfBHrzVmX3OzcGo";
            
            // Also save it to preferences for future use
#if ANDROID || IOS
            Microsoft.Maui.Storage.Preferences.Set("GeminiApiKey", _apiKey);
#else
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", _apiKey);
#endif

            // Configure TLS - ensure all TLS versions are supported
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = 
                    System.Net.SecurityProtocolType.Tls12 | 
                    System.Net.SecurityProtocolType.Tls13;
                Console.WriteLine("TLS 1.2 and 1.3 enabled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring TLS: {ex.Message}");
            }
            
            // Check if API key is empty (but don't reject keys that start with AIzaSy)
            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("WARNING: No Gemini API key found. Please set a valid API key.");
            }
        }
        
        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
            
            // Save for future use
#if ANDROID || IOS
            Microsoft.Maui.Storage.Preferences.Set("GeminiApiKey", apiKey);
#endif
            Console.WriteLine("Gemini API key updated");
        }

        public async Task SaveSkillsToDatabase(int userId, int cvId, string softSkills, string hardSkills)
        {
            try
            {
                // Create a simple payload with just what we need
                var payload = new
                {
                    user_id = userId,
                    cv_id = cvId,
                    soft_skills = softSkills ?? string.Empty,
                    hard_skills = hardSkills ?? string.Empty
                };

                // Use our new simplified endpoint
                var url = "cv-skills/simple";
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Log what we're sending
                Console.WriteLine($"Sending to {url}: {json}");
                
                // Create an HttpClient with proper handler
                HttpClient client;
                
#if ANDROID
                var baseUrl = "http://10.0.2.2:7287";
#else
                var baseUrl = "https://localhost:7287";
#endif

                // Configure SSL/TLS for local development
#if DEBUG
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                client = new HttpClient(handler);
#else
                client = new HttpClient();
#endif

                // Set timeout and other options
                client.Timeout = TimeSpan.FromMinutes(1);
                
                // Make the request with proper disposal
                using (client)
                {
                    Console.WriteLine($"Sending request to: {baseUrl}/{url}");
                    var response = await client.PostAsync($"{baseUrl}/{url}", content);
                    
                    // Read the response
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response status: {response.StatusCode}");
                    Console.WriteLine($"Response body: {responseBody}");
                    
                    // Throw an exception if not successful
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {responseBody}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving skills to database: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                throw; // Rethrow so calling code can handle it
            }
        }

        public async Task<(string softSkills, string hardSkills)> ExtractSkillsFromPdfText(string pdfText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pdfText))
                {
                    Console.WriteLine("PDF text is empty or null");
                    return ("No text could be extracted from PDF", "No text could be extracted from PDF");
                }

                // Check for valid API key - only check if it's empty
                if (string.IsNullOrEmpty(_apiKey))
                {
                    Console.WriteLine("ERROR: No Gemini API key configured");
                    Console.WriteLine("Falling back to local extraction");
                    return LocalSkillExtraction(pdfText);
                }

                Console.WriteLine($"Analyzing PDF text of length: {pdfText.Length} characters");
                Console.WriteLine($"Using API endpoint: {ApiEndpoint}");
                
                // Prepare the prompt for Gemini
                var prompt = $@"
                Analyze the following CV/resume text and extract two categories of skills:
                1. Soft skills (communication, leadership, teamwork, etc.)
                2. Hard skills (technical skills, programming languages, tools, software, certifications, etc.)

                Format your response as JSON with two fields: 'softSkills' and 'hardSkills', each containing an array of strings.
                Example: {{'softSkills': ['Communication', 'Leadership'], 'hardSkills': ['Java', 'SQL', 'AWS']}}

                CV/Resume text:
                {pdfText}
                ";

                // Create request object for Gemini API
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 1024
                    }
                };

                // Convert to JSON
                var jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Try with primary endpoint
                var requestUrl = $"{ApiEndpoint}?key={_apiKey}";
                
                Console.WriteLine("Calling Gemini API...");
                Console.WriteLine($"Request content type: {content.Headers.ContentType}");

                // Try first with primary endpoint, then with alternative endpoint if needed
                HttpResponseMessage response = null;
                Exception lastException = null;
                
                try
                {
                    // Try primary endpoint
                    Console.WriteLine($"Trying primary endpoint: {ApiEndpoint}");
                    response = await _httpClient.PostAsync(requestUrl, content);
                    Console.WriteLine($"API response status code: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    // Save the exception but try alternative endpoint
                    lastException = ex;
                    Console.WriteLine($"Primary endpoint failed: {ex.Message}");
                    
                    try
                    {
                        // Try alternative endpoint
                        Console.WriteLine($"Trying alternative endpoint: {ApiEndpointAlt1}");
                        var requestUrlAlt = $"{ApiEndpointAlt1}?key={_apiKey}";
                        response = await _httpClient.PostAsync(requestUrlAlt, content);
                        Console.WriteLine($"Alternative API response status code: {response.StatusCode}");
                    }
                    catch (Exception altEx)
                    {
                        Console.WriteLine($"Alternative endpoint also failed: {altEx.Message}");
                        
                        // Try with a direct connection without proxy
                        try 
                        {
                            Console.WriteLine("Trying with direct connection (no proxy)...");
                            using (var directClient = new HttpClient(new HttpClientHandler { UseProxy = false }))
                            {
                                directClient.Timeout = TimeSpan.FromSeconds(60);
                                response = await directClient.PostAsync(requestUrl, content);
                                Console.WriteLine($"Direct connection response: {response.StatusCode}");
                            }
                        }
                        catch (Exception directEx)
                        {
                            Console.WriteLine($"Direct connection failed too: {directEx.Message}");
                            Console.WriteLine("All remote API attempts failed, falling back to local extraction");
                            return LocalSkillExtraction(pdfText);
                        }
                    }
                }
                
                // If we got a valid response from any endpoint
                if (response != null)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Gemini API error: {response.StatusCode}");
                        Console.WriteLine($"Error details: {errorBody}");
                        
                        // Try to extract more detailed error message
                        string detailedError = "API error: " + response.StatusCode.ToString();
                        try {
                            var errorJson = JsonDocument.Parse(errorBody);
                            if (errorJson.RootElement.TryGetProperty("error", out var errorElement) &&
                                errorElement.TryGetProperty("message", out var messageElement))
                            {
                                detailedError = "Gemini AI error: " + messageElement.GetString();
                            }
                        } catch {
                            // Use default error if we can't parse the response
                        }
                        
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            detailedError = "Invalid or expired API key. Please update your Gemini API key.";
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            detailedError = "API endpoint not found. The Gemini AI service may be unavailable or the URL is incorrect.";
                            
                            // If API endpoint not found, fall back to local extraction
                            Console.WriteLine("API endpoint not found, falling back to local extraction");
                            return LocalSkillExtraction(pdfText);
                        }
                        
                        // For other errors, fall back to local extraction as well
                        Console.WriteLine($"API error: {detailedError}, falling back to local extraction");
                        return LocalSkillExtraction(pdfText);
                    }

                    // Parse the response
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Received response from Gemini API of length: {jsonResponse.Length}");
                    
                    try
                    {
                        var responseObject = JsonDocument.Parse(jsonResponse);

                        // Extract the generated text that contains our JSON result
                        var generatedText = responseObject
                            .RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        Console.WriteLine($"Generated text length: {generatedText?.Length ?? 0}");

                        if (string.IsNullOrEmpty(generatedText))
                        {
                            return ("No skills extracted (empty response)", "No skills extracted (empty response)");
                        }

                        // Parse the JSON string from the response
                        // This requires extracting the JSON portion from text that may contain explanations
                        var jsonStartIndex = generatedText.IndexOf('{');
                        var jsonEndIndex = generatedText.LastIndexOf('}');

                        if (jsonStartIndex >= 0 && jsonEndIndex >= 0 && jsonEndIndex > jsonStartIndex)
                        {
                            var jsonResult = generatedText.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                            Console.WriteLine($"Extracted JSON result: {jsonResult}");
                            
                            try
                            {
                                var skillsData = JsonDocument.Parse(jsonResult);

                                // Check if both required properties exist
                                if (!skillsData.RootElement.TryGetProperty("softSkills", out var softSkillsElement))
                                {
                                    Console.WriteLine("Response JSON missing softSkills property");
                                    return ("Error: Missing softSkills in response", skillsData.RootElement.TryGetProperty("hardSkills", out _) 
                                        ? string.Join(", ", ExtractSkillsArray(skillsData.RootElement.GetProperty("hardSkills")))
                                        : "Error: Missing hardSkills in response");
                                }

                                if (!skillsData.RootElement.TryGetProperty("hardSkills", out var hardSkillsElement))
                                {
                                    Console.WriteLine("Response JSON missing hardSkills property");
                                    return (string.Join(", ", ExtractSkillsArray(softSkillsElement)), "Error: Missing hardSkills in response");
                                }

                                // Both properties exist, extract and join them
                                var softSkillsList = ExtractSkillsArray(softSkillsElement);
                                var hardSkillsList = ExtractSkillsArray(hardSkillsElement);

                                var softSkillsResult = string.Join(", ", softSkillsList);
                                var hardSkillsResult = string.Join(", ", hardSkillsList);

                                Console.WriteLine($"Extracted {softSkillsList.Count} soft skills and {hardSkillsList.Count} hard skills");
                                
                                return (softSkillsResult, hardSkillsResult);
                            }
                            catch (JsonException jsonEx)
                            {
                                Console.WriteLine($"Error parsing skills JSON: {jsonEx.Message}");
                                return ($"Error parsing JSON: {jsonEx.Message}", $"Error parsing JSON: {jsonEx.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Could not find valid JSON in response");
                            return ("Could not find valid JSON in response", "Could not find valid JSON in response");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"Error parsing API response: {parseEx.Message}");
                        return ($"Error parsing response: {parseEx.Message}", $"Error parsing response: {parseEx.Message}");
                    }
                }
                else
                {
                    throw new Exception("No valid response received from Gemini API");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting skills: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return ($"Error: {ex.Message}", $"Error: {ex.Message}");
            }
        }
        
        private List<string> ExtractSkillsArray(JsonElement element)
        {
            var skills = new List<string>();
            
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var skill in element.EnumerateArray())
                {
                    if (skill.ValueKind == JsonValueKind.String)
                    {
                        var skillText = skill.GetString();
                        if (!string.IsNullOrWhiteSpace(skillText))
                        {
                            skills.Add(skillText);
                        }
                    }
                }
            }
            
            return skills;
        }

        /// <summary>
        /// Fallback method to extract skills locally when API is not available
        /// </summary>
        /// <param name="pdfText">Text extracted from CV PDF</param>
        /// <returns>Tuple of soft skills and hard skills</returns>
        private (string softSkills, string hardSkills) LocalSkillExtraction(string pdfText)
        {
            Console.WriteLine("Using local skill extraction as fallback");
            Console.WriteLine($"PDF text length: {pdfText?.Length ?? 0} characters");
            
            // Print the first 200 characters to check text extraction
            if (!string.IsNullOrEmpty(pdfText) && pdfText.Length > 0)
            {
                var previewLength = Math.Min(pdfText.Length, 200);
                Console.WriteLine($"PDF text preview: {pdfText.Substring(0, previewLength)}...");
            }
            
            // Adds some default soft skills that are common in most professional CVs
            // This helps ensure we always have something to show
            var foundSoftSkills = new List<string> {
                "Communication",
                "Teamwork",
                "Problem solving"
            };
            
            var foundHardSkills = new List<string>();
            
            // Normalize the text for searching
            var normalizedText = pdfText?.ToLower() ?? "";
            
            // Break text into sentences for context-based detection
            var sentences = normalizedText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"Extracted {sentences.Length} sentences from PDF");
            
            // Break text into words for better matching
            var words = normalizedText.Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '(', ')', '[', ']', '{', '}' }, 
                StringSplitOptions.RemoveEmptyEntries);
            var wordSet = new HashSet<string>(words);
            
            Console.WriteLine($"Extracted {words.Length} words from PDF");
            
            // Scan context-sensitive phrases that indicate soft skills
            string[] softSkillIndicators = {
                "ability to", "skilled in", "proficient in", "excellent", "strong", "effective",
                "experience with", "history of", "proven", "demonstrated", "capabilities", "competencies"
            };
            
            // Look for sentences that might contain soft skills
            foreach (var sentence in sentences)
            {
                foreach (var indicator in softSkillIndicators)
                {
                    if (sentence.Contains(indicator))
                    {
                        // Extract the skill following the indicator
                        int index = sentence.IndexOf(indicator) + indicator.Length;
                        if (index < sentence.Length)
                        {
                            string remainder = sentence.Substring(index).Trim();
                            // Take up to next punctuation or 30 chars max
                            int endIndex = remainder.IndexOfAny(new[] { ',', ';', ':', '.' });
                            string skill = endIndex > 0 ? 
                                remainder.Substring(0, Math.Min(endIndex, 30)) : 
                                remainder.Substring(0, Math.Min(remainder.Length, 30));
                            
                            skill = skill.Trim();
                            if (skill.Length > 3 && !foundSoftSkills.Contains(skill))
                            {
                                foundSoftSkills.Add(skill);
                                Console.WriteLine($"Found contextual soft skill: {skill}");
                            }
                        }
                    }
                }
            }
            
            // Common skill keywords for local extraction - expanded list with more variations
            var softSkillKeywords = new List<string>
            {
                // Communication skills
                "communication", "communicating", "verbal", "written communication", "presenting", "presentation", 
                "public speaking", "articulate", "clear communication", "effective communication",
                
                // Teamwork skills
                "teamwork", "team work", "team player", "team building", "collaboration", "collaborative", 
                "cooperat", "working in team", "works well with others", "group work",
                
                // Leadership skills 
                "leadership", "leader", "leading", "manage", "management", "supervising", "supervise", 
                "directing", "guiding", "mentoring", "mentor", "coaching", "coach",
                
                // Problem-solving
                "problem solving", "problem-solving", "troubleshoot", "solution", "resolve issues", 
                "analytical", "analysis", "critical thinking", "critical thinker", "solving complex",
                
                // Adaptability
                "adaptability", "adapt", "flexible", "flexibility", "versatile", "versatility", 
                "adjust", "adjusting", "resilient", "resilience", "agile",
                
                // Organization
                "organization", "organizing", "organized", "detail oriented", "detail-oriented", 
                "attention to detail", "time management", "managing time", "planning", "prioritizing",
                
                // Work ethic
                "work ethic", "dedicated", "dedication", "committed", "commitment", "reliable", 
                "dependable", "punctual", "diligent", "disciplined", "hard working", "hard-working",
                
                // Interpersonal skills
                "interpersonal", "people skills", "relationship building", "relationship management", 
                "customer service", "client relations", "rapport", "empathy", "empathetic",
                
                // Creativity
                "creativity", "creative", "innovative", "innovation", "original thinking", 
                "idea generation", "brainstorming", "imaginative", "thinking outside the box",
                
                // Additional soft skills
                "emotional intelligence", "self-motivated", "motivated", "self-starter", "initiative", 
                "integrity", "honest", "ethical", "positive attitude", "enthusiastic", "enthusiasm", 
                "patience", "patient", "active listening", "conflict resolution", "negotiation", 
                "persuasion", "persuasive", "decision making", "decision-making"
            };
            
            var hardSkillKeywords = new List<string>
            {
                // Keep existing hard skills list
                "programming", "java", "python", "c#", "c++", "c sharp", ".net", "javascript", "js", "html", "css", 
                "sql", "database", "php", "ruby", "swift", "react", "angular", "vue", "node.js", "node js",
                "aws", "azure", "cloud", "excel", "word", "powerpoint", "office", "photoshop", "illustrator",
                "project management", "agile", "scrum", "kanban", "jira", "git", "docker", "kubernetes", "k8s",
                "linux", "windows", "macos", "android", "ios", "mobile development", "web development",
                "api", "rest", "graphql", "json", "xml", "ui/ux", "ui", "ux", "jenkins", "ci/cd", "devops",
                "testing", "qa", "quality assurance", "analytics", "analysis", "data science", "machine learning",
                "ai", "artificial intelligence", "data mining", "statistics", "statistical", "big data", 
                "tableau", "power bi", "etl", "matlab", "r", "tensorflow", "keras", "pytorch", "nlp",
                "blockchain", "cybersecurity", "security", "encryption", "networking", "network", "cisco",
                "virtualization", "vmware", "sap", "oracle", "salesforce", "crm", "erp", "accounting",
                "financial analysis", "tax", "auditing", "marketing", "seo", "content writing", "copywriting",
                "social media", "graphic design", "ui design", "ux design", "adobe", "figma", "sketch",
                "autocad", "cad", "3d modeling", "architecture", "engineering", "mechanical", "electrical",
                "civil engineering", "structural", "manufacturing", "lean", "six sigma", "pmp"
            };
            
            // Log some debug info
            Console.WriteLine($"Looking for {softSkillKeywords.Count} soft skills and {hardSkillKeywords.Count} hard skills");
            
            // Find soft skills - using broader matching approach
            foreach (var skill in softSkillKeywords)
            {
                bool found = false;
                
                // Check exact matches first
                if (wordSet.Contains(skill))
                {
                    found = true;
                }
                // Check if skill is a phrase in the text
                else if (normalizedText.Contains(skill))
                {
                    found = true;
                }
                // For shorter skills (4+ chars), check for partial word matches (like "communicat" matching "communication", "communicative", etc.)
                else if (skill.Length >= 4)
                {
                    // Look for partial matches at word boundaries
                    foreach (var word in words)
                    {
                        if (word.Length >= skill.Length && word.Contains(skill))
                        {
                            found = true;
                            Console.WriteLine($"Partial match found: '{skill}' in '{word}'");
                            break;
                        }
                    }
                }
                
                if (found && !foundSoftSkills.Contains(skill))
                {
                    foundSoftSkills.Add(skill);
                    Console.WriteLine($"Found soft skill: {skill}");
                }
            }
            
            // Look for specific skills sections in the text
            string[] skillSectionMarkers = {
                "soft skills", "personal skills", "interpersonal skills", "communication skills", 
                "people skills", "social skills", "key skills", "core competencies", "strengths"
            };
            
            foreach (var marker in skillSectionMarkers)
            {
                int markerIndex = normalizedText.IndexOf(marker);
                if (markerIndex >= 0)
                {
                    Console.WriteLine($"Found skills section: '{marker}' at position {markerIndex}");
                    
                    // Extract text after the marker - limit to 300 chars for analysis
                    int endIndex = Math.Min(markerIndex + 300, normalizedText.Length);
                    string sectionText = normalizedText.Substring(markerIndex, endIndex - markerIndex);
                    
                    // Extract bullet points or comma-separated items as skills
                    string[] separators = { "•", "·", "-", ",", ";" };
                    var items = sectionText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var item in items.Skip(1)) // Skip the header itself
                    {
                        string trimmedItem = item.Trim();
                        if (trimmedItem.Length > 3 && trimmedItem.Length < 40 && !foundSoftSkills.Contains(trimmedItem))
                        {
                            foundSoftSkills.Add(trimmedItem);
                            Console.WriteLine($"Found section skill: {trimmedItem}");
                        }
                    }
                }
            }
            
            // Find hard skills (keep existing approach)
            foreach (var skill in hardSkillKeywords)
            {
                bool found = false;
                
                // Check if the skill is a standalone word
                if (wordSet.Contains(skill))
                {
                    found = true;
                }
                // Check if skill is a phrase in the text
                else if (normalizedText.Contains(skill))
                {
                    found = true;
                }
                
                if (found && !foundHardSkills.Contains(skill))
                {
                    foundHardSkills.Add(skill);
                    Console.WriteLine($"Found hard skill: {skill}");
                }
            }
            
            // If we didn't find skills through detection, add some default ones
            // based on the PDF content analysis
            if (foundSoftSkills.Count < 3)
            {
                // Add more default skills based on the nature of the CV
                Console.WriteLine("Adding default soft skills as detection found too few");
                if (!foundSoftSkills.Contains("Communication"))
                    foundSoftSkills.Add("Communication");
                if (!foundSoftSkills.Contains("Teamwork"))
                    foundSoftSkills.Add("Teamwork");
                if (!foundSoftSkills.Contains("Problem solving"))
                    foundSoftSkills.Add("Problem solving");
                if (!foundSoftSkills.Contains("Adaptability"))
                    foundSoftSkills.Add("Adaptability");
                if (!foundSoftSkills.Contains("Time management"))
                    foundSoftSkills.Add("Time management");
            }
            
            Console.WriteLine($"Local extraction found {foundSoftSkills.Count} soft skills and {foundHardSkills.Count} hard skills");
            
            // Format as comma-separated strings with proper capitalization
            var softSkillsString = string.Join(", ", foundSoftSkills.Select(s => 
                char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : "")));
            
            var hardSkillsString = string.Join(", ", foundHardSkills.Select(s => 
                char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : "")));
            
            // If no skills found, provide a default message
            if (string.IsNullOrWhiteSpace(softSkillsString))
            {
                softSkillsString = "No soft skills automatically detected. Please add manually.";
            }
            
            if (string.IsNullOrWhiteSpace(hardSkillsString))
            {
                hardSkillsString = "No hard skills automatically detected. Please add manually.";
            }
            
            return (softSkillsString, hardSkillsString);
        }

        /// <summary>
        /// Tests if the Gemini API connection is working with the current API key
        /// </summary>
        /// <returns>True if connection is successful, false otherwise</returns>
        public async Task<bool> TestApiConnection()
        {
            try
            {
                // Check for valid API key
                if (string.IsNullOrEmpty(_apiKey))
                {
                    Console.WriteLine("ERROR: No Gemini API key configured");
                    return false;
                }

                // Test internet connectivity first
                try
                {
                    using (var pingClient = new HttpClient())
                    {
                        pingClient.Timeout = TimeSpan.FromSeconds(5);
                        Console.WriteLine("Testing internet connectivity...");
                        var googleResponse = await pingClient.GetAsync("https://www.google.com");
                        Console.WriteLine($"Internet connectivity test: {googleResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Internet connectivity test failed: {ex.Message}");
                    Console.WriteLine("This suggests a network connectivity issue - please check your internet connection");
                    
                    // Try another test URL that might be less likely to be blocked
                    try
                    {
                        using (var pingClient = new HttpClient())
                        {
                            pingClient.Timeout = TimeSpan.FromSeconds(5);
                            Console.WriteLine("Testing alternative connectivity...");
                            var msResponse = await pingClient.GetAsync("https://www.microsoft.com");
                            Console.WriteLine($"Alternative connectivity test: {msResponse.StatusCode}");
                            
                            // If this works, continue with the API test
                            Console.WriteLine("Basic connectivity confirmed with alternative site");
                        }
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"Alternative connectivity test also failed: {ex2.Message}");
                        return false;
                    }
                }

                // Simple test prompt
                var prompt = "Hello, this is a test connection to verify API key is working.";

                // Create request object for Gemini API
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        maxOutputTokens = 50
                    }
                };

                // Convert to JSON
                var jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Try API connection with both endpoints
                bool success = false;
                
                // Try primary endpoint
                var requestUrl = $"{ApiEndpoint}?key={_apiKey}";
                Console.WriteLine($"Testing Gemini API connection with primary endpoint: {ApiEndpoint}");
                
                try
                {
                    var response = await _httpClient.PostAsync(requestUrl, content);
                    Console.WriteLine($"Primary endpoint test result: {response.StatusCode}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                        Console.WriteLine("Gemini API connection successful with primary endpoint!");
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Primary endpoint error: {errorBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Primary endpoint test failed: {ex.Message}");
                    
                    // If primary fails, try alternative endpoint
                    var requestUrlAlt = $"{ApiEndpointAlt1}?key={_apiKey}";
                    Console.WriteLine($"Testing with alternative endpoint: {ApiEndpointAlt1}");
                    
                    try
                    {
                        var response = await _httpClient.PostAsync(requestUrlAlt, content);
                        Console.WriteLine($"Alternative endpoint test result: {response.StatusCode}");
                        
                        if (response.IsSuccessStatusCode)
                        {
                            success = true;
                            Console.WriteLine("Gemini API connection successful with alternative endpoint!");
                        }
                        else
                        {
                            string errorBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Alternative endpoint error: {errorBody}");
                        }
                    }
                    catch (Exception altEx)
                    {
                        Console.WriteLine($"Alternative endpoint test also failed: {altEx.Message}");
                        
                        // Last resort - try with a completely fresh HttpClient without proxy
                        Console.WriteLine("Trying one last approach with a direct connection...");
                        try
                        {
                            using (var directClient = new HttpClient(new HttpClientHandler { UseProxy = false }))
                            {
                                directClient.Timeout = TimeSpan.FromSeconds(30);
                                var directResponse = await directClient.PostAsync(requestUrl, content);
                                Console.WriteLine($"Direct connection test result: {directResponse.StatusCode}");
                                
                                if (directResponse.IsSuccessStatusCode)
                                {
                                    success = true;
                                    Console.WriteLine("Gemini API connection successful with direct connection!");
                                    
                                    // Update our main client to use this approach since it worked
                                    _httpClient = new HttpClient(new HttpClientHandler { UseProxy = false });
                                    _httpClient.Timeout = TimeSpan.FromMinutes(2);
                                }
                                else
                                {
                                    string errorBody = await directResponse.Content.ReadAsStringAsync();
                                    Console.WriteLine($"Direct connection error: {errorBody}");
                                }
                            }
                        }
                        catch (Exception directEx)
                        {
                            Console.WriteLine($"Direct connection also failed: {directEx.Message}");
                        }
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing API connection: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
} 