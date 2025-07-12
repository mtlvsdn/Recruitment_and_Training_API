using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON options to handle Base64 strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

//Configure Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost\\MSSQLSERVER01;Database=RecruitmentAndTraining;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll"); // Apply CORS policy

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure to use both HTTP and HTTPS
app.UseHttpsRedirection();

// Run on both HTTP and HTTPS ports
var urls = new[] { "http://0.0.0.0:7287", "https://0.0.0.0:7288" };
app.Urls.Clear();
foreach (var url in urls)
{
    app.Urls.Add(url);
}

//Database Context
var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

////////////////////////////////////////////////////////////////////
///////////////////////////////SUPERUSER////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/superuser", async (AppDbContext db) =>
    await db.SuperUser.ToListAsync());

app.MapGet("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail) =>
    await db.SuperUser.FindAsync(SuperUseremail) is SuperUser user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/superuser", async (AppDbContext db, SuperUser superUser) =>
{
    db.SuperUser.Add(superUser);
    await db.SaveChangesAsync();
    return Results.Created($"/superuser/{superUser.SuperUseremail}", superUser);
});

app.MapPut("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail, SuperUser updatedSuperUser) =>
{
    var existingUser = await db.SuperUser.FindAsync(SuperUseremail);
    if (existingUser == null)
    {
        return Results.NotFound();
    }
    existingUser.Password = updatedSuperUser.Password;
    await db.SaveChangesAsync();
    return Results.Ok(existingUser);
});

app.MapDelete("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail) =>
{
    var userToRemove = await db.SuperUser.FindAsync(SuperUseremail);
    if (userToRemove == null)
    {
        return Results.NotFound();
    }
    db.SuperUser.Remove(userToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(userToRemove);
});

////////////////////////////////////////////////////////////////////
////////////////////////////////USER////////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/user", async (AppDbContext db) =>
    await db.User.ToListAsync());

app.MapGet("/user/byemail/{email}", async (AppDbContext db, string email) =>
    await db.User.FirstOrDefaultAsync(u => u.Email == email) is User user
        ? Results.Ok(user)
        : Results.NotFound());

app.MapGet("/user/{id}", async (AppDbContext db, int id) =>
    await db.User.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/user", async (AppDbContext db, User user) => {
    db.User.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/user/{user.Id}", user);
});

app.MapPut("/user/{id}", async (AppDbContext db, int id, User updatedUser) =>
{
    var existingUser = await db.User.FindAsync(id);
    if (existingUser == null)
    {
        return Results.NotFound();
    }

    existingUser.Email = updatedUser.Email;
    existingUser.Full_Name = updatedUser.Full_Name;
    existingUser.Password = updatedUser.Password;
    existingUser.Company_Name = updatedUser.Company_Name;

    await db.SaveChangesAsync();
    return Results.Ok(existingUser);
});

app.MapDelete("/user/{id}", async (AppDbContext db, int id) =>
{
    var userToRemove = await db.User.FindAsync(id);
    if (userToRemove == null)
    {
        return Results.NotFound();
    }
    db.User.Remove(userToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(userToRemove);
});

////////////////////////////////////////////////////////////////////
//////////////////////////////COMPANY///////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/company", async (AppDbContext db) =>
    await db.Company.ToListAsync());

app.MapGet("/company/byemail/{email}", async (AppDbContext db, string email) =>
    await db.Company.FirstOrDefaultAsync(c => c.Email == email) is Company company
        ? Results.Ok(company)
        : Results.NotFound());

app.MapGet("/company/{company_name}", async (AppDbContext db, string company_name) =>
    await db.Company.FindAsync(company_name) is Company company ? Results.Ok(company) : Results.NotFound());

app.MapPost("/company", async (AppDbContext db, Company company) =>
{
    db.Company.Add(company);
    await db.SaveChangesAsync();
    return Results.Created($"/company/{company.Company_Name}", company);
});

app.MapPut("/company/{company_name}", async (AppDbContext db, string company_name, Company updatedCompany) =>
{
    var existingCompany = await db.Company.FindAsync(company_name);
    if (existingCompany == null)
    {
        return Results.NotFound();
    }
    existingCompany.Email = updatedCompany.Email;
    existingCompany.Password = updatedCompany.Password;
    existingCompany.Nr_of_accounts = updatedCompany.Nr_of_accounts;
    existingCompany.SuperUseremail = updatedCompany.SuperUseremail;
    await db.SaveChangesAsync();
    return Results.Ok(existingCompany);
});

app.MapDelete("/company/{companyName}", async (AppDbContext db, string companyName) =>
{
    var companyToRemove = await db.Company.FindAsync(companyName);
    if (companyToRemove == null) return Results.NotFound();

    var relatedUsers = await db.User.Where(u => u.Company_Name == companyName).ToListAsync();
    if (relatedUsers.Any())
    {
        db.User.RemoveRange(relatedUsers);
    }

    db.Company.Remove(companyToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(companyToRemove);
});
////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION DEVS////////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate", async (AppDbContext db, LoginRequest loginRequest) =>
{
    var user = await db.SuperUser.FindAsync(loginRequest.SuperUseremail);

    if (user == null)
        return Results.NotFound("User not found");

    if (user.Password != loginRequest.Password)
        return Results.Unauthorized();

    var token = Guid.NewGuid().ToString();

    return Results.Ok(new
    {
        Token = token,
        Email = user.SuperUseremail
    });
});

////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION USERS///////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate-user", async (AppDbContext db, LoginRequestUser loginRequestUser) =>
{
    try
    {
        // Find user by email instead of trying to use FindAsync with email as primary key
        var user = await db.User.FirstOrDefaultAsync(u => u.Email == loginRequestUser.Email);

        if (user == null)
            return Results.NotFound("User not found");

        if (user.Password != loginRequestUser.Password)
            return Results.Unauthorized();

        var token = Guid.NewGuid().ToString();

        return Results.Ok(new
        {
            Token = token,
            Email = user.Email
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Authentication error: {ex.Message}");
    }
});

////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION COMPANY/////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate-company", async (AppDbContext db, LoginRequestUser loginRequestCompany) =>
{
    try
    {
        Console.WriteLine($"Attempting company login for email: {loginRequestCompany.Email}");
        
        if (string.IsNullOrEmpty(loginRequestCompany.Email) || string.IsNullOrEmpty(loginRequestCompany.Password))
        {
            Console.WriteLine("Login attempt failed: Email or password is empty");
            return Results.BadRequest("Email and password are required");
        }

        var company = await db.Company.FirstOrDefaultAsync(c => c.Email == loginRequestCompany.Email);
        
        if (company == null)
        {
            Console.WriteLine("Login attempt failed: Company not found");
            return Results.NotFound("Company not found");
        }

        if (company.Password != loginRequestCompany.Password)
        {
            Console.WriteLine("Login attempt failed: Invalid password");
            return Results.Unauthorized();
        }

        var token = Guid.NewGuid().ToString();
        Console.WriteLine($"Login successful for company: {company.Company_Name}");

        return Results.Ok(new
        {
            Token = token,
            Email = company.Email,
            CompanyName = company.Company_Name,
            NrOfAccounts = company.Nr_of_accounts
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during company authentication: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Authentication Error",
            detail: "An error occurred during authentication. Please try again.",
            statusCode: 500
        );
    }
});

////////////////////////////////////////////////////////////////////
///////////////////////////HOMEPAGE CARDS///////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/company/count", async (AppDbContext db) =>
{
    var companyCount = await db.Company.CountAsync();
    return Results.Ok(companyCount);
});

app.MapGet("/user/count", async (AppDbContext db) =>
{
    var userCount = await db.User.CountAsync();
    return Results.Ok(userCount);
});

////////////////////////////////////////////////////////////////////
///////////////////////////////CV PDF///////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/cv-pdf/{userId}", async (AppDbContext db, int userId) =>
    await db.CvPdf.FindAsync(userId) is CvPdf cv ? Results.Ok(cv) : Results.NotFound());

app.MapDelete("/cv-pdf/{userId}", async (AppDbContext db, int userId) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM CV_as_PDF WHERE cv_id = @p0", 
            userId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting CV: {ex.Message}");
        return Results.Problem($"Error deleting CV: {ex.Message}");
    }
});

// Form data upload endpoint
app.MapPost("/cv-pdf/upload", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();
        var userIdString = form["userId"].ToString();

        if (file == null)
        {
            return Results.BadRequest("No file uploaded");
        }

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Results.BadRequest("Valid user ID is required");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileData = ms.ToArray();

        var cvPdf = new CvPdf
        {
            cv_id = userId,
            file_name = file.FileName,
            file_size = fileData.Length,
            file_data = fileData
        };

        var existingCv = await db.CvPdf.FindAsync(userId);
        if (existingCv != null)
        {
            existingCv.file_name = cvPdf.file_name;
            existingCv.file_size = cvPdf.file_size;
            existingCv.file_data = cvPdf.file_data;
            db.CvPdf.Update(existingCv);
        }
        else
        {
            db.CvPdf.Add(cvPdf);
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "File uploaded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing file upload: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error uploading file: {ex.Message}");
    }
})
.DisableAntiforgery()
.WithName("UploadCvForm")
.WithOpenApi();

// JSON upload endpoint (for Swagger testing)
app.MapPost("/cv-pdf/upload-json", async ([FromBody] CvPdfUploadRequest request, AppDbContext db) =>
{
    try
    {
        if (request == null)
        {
            return Results.BadRequest("Request data is required");
        }

        byte[] fileData;
        try
        {
            fileData = Convert.FromBase64String(request.file_data);
        }
        catch (FormatException)
        {
            return Results.BadRequest("Invalid file data format. File data must be Base64 encoded.");
        }

        var cvPdf = new CvPdf
        {
            cv_id = request.cv_id,
            file_name = request.file_name,
            file_size = fileData.Length,
            file_data = fileData
        };

        var existingCv = await db.CvPdf.FindAsync(request.cv_id);
        if (existingCv != null)
        {
            existingCv.file_name = cvPdf.file_name;
            existingCv.file_size = cvPdf.file_size;
            existingCv.file_data = cvPdf.file_data;
            db.CvPdf.Update(existingCv);
        }
        else
        {
            db.CvPdf.Add(cvPdf);
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "File uploaded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error uploading file: {ex.Message}");
        return Results.Problem($"Error uploading file: {ex.Message}");
    }
})
.WithName("UploadCvJson")
.WithOpenApi();

////////////////////////////////////////////////////////////////////
////////////////////////////////TEST////////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/test", async (AppDbContext db) =>
    await db.Test.ToListAsync());

app.MapGet("/test/{id}", async (AppDbContext db, int id) =>
    await db.Test.FindAsync(id) is Test test ? Results.Ok(test) : Results.NotFound());

app.MapPost("/test", async (AppDbContext db, [FromBody] Test test) =>
{
    try
    {
        Console.WriteLine($"Attempting to save test: {test.test_name} for company: {test.company_name}");
        
        // Validate company exists
        var company = await db.Company.FindAsync(test.company_name);
        if (company == null)
        {
            Console.WriteLine($"Company not found: {test.company_name}");
            return Results.NotFound($"Company {test.company_name} not found");
        }

        // Create new test entity
        var newTest = new Test
        {
            test_name = test.test_name,
            no_of_questions = test.no_of_questions,
            time_limit = test.time_limit,
            company_name = test.company_name
        };

        // Add and save
        db.Test.Add(newTest);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"Test saved successfully with ID: {newTest.test_id}");
        return Results.Created($"/test/{newTest.test_id}", newTest);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving test: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Saving Test",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapPut("/test/{id}", async (AppDbContext db, int id, Test updatedTest) =>
{
    var existingTest = await db.Test.FindAsync(id);
    if (existingTest == null)
    {
        return Results.NotFound();
    }

    existingTest.test_name = updatedTest.test_name;
    existingTest.no_of_questions = updatedTest.no_of_questions;
    existingTest.time_limit = updatedTest.time_limit;
    existingTest.company_name = updatedTest.company_name;

    await db.SaveChangesAsync();
    return Results.Ok(existingTest);
});

app.MapDelete("/test/{id}", async (AppDbContext db, int id) =>
{
    try
    {
        Console.WriteLine($"Attempting to delete test with ID: {id}");
        
        var testToRemove = await db.Test.FindAsync(id);
        if (testToRemove == null)
        {
            Console.WriteLine($"Test not found with ID: {id}");
            return Results.NotFound();
        }
        
        // First, delete any user-test associations
        var userTestAssociations = await db.UserTest.Where(ut => ut.Testtest_id == id).ToListAsync();
        if (userTestAssociations.Any())
        {
            Console.WriteLine($"Deleting {userTestAssociations.Count} user-test associations for test ID: {id}");
            db.UserTest.RemoveRange(userTestAssociations);
            await db.SaveChangesAsync();
        }
        
        // Next, delete all questions associated with this test
        var associatedQuestions = await db.Questions.Where(q => q.test_id == id).ToListAsync();
        if (associatedQuestions.Any())
        {
            Console.WriteLine($"Deleting {associatedQuestions.Count} questions for test ID: {id}");
            db.Questions.RemoveRange(associatedQuestions);
            await db.SaveChangesAsync();
        }
        
        // Finally, delete the test itself
        db.Test.Remove(testToRemove);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"Successfully deleted test with ID: {id}");
        return Results.Ok(testToRemove);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting test {id}: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Deleting Test",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

////////////////////////////////////////////////////////////////////
////////////////////////////////QUESTIONS////////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/questions", async (AppDbContext db) =>
    await db.Questions.ToListAsync());

app.MapGet("/questions/{id}", async (AppDbContext db, int id) =>
    await db.Questions.FindAsync(id) is Questions question ? Results.Ok(question) : Results.NotFound());

app.MapGet("/questions/test/{testId}", async (AppDbContext db, int testId) =>
    await db.Questions.Where(q => q.test_id == testId).ToListAsync());

app.MapPut("/questions/{id}", async (AppDbContext db, int id, Questions updatedQuestion) =>
{
    try
    {
        Console.WriteLine($"Attempting to update question with ID: {id}");
        
        var existingQuestion = await db.Questions.FindAsync(id);
        if (existingQuestion == null)
        {
            Console.WriteLine($"Question not found with ID: {id}");
            return Results.NotFound($"Question with ID {id} not found");
        }

        // Update all fields
        existingQuestion.test_id = updatedQuestion.test_id;
        existingQuestion.question_text = updatedQuestion.question_text;
        existingQuestion.possible_answer_1 = updatedQuestion.possible_answer_1;
        existingQuestion.possible_answer_2 = updatedQuestion.possible_answer_2;
        existingQuestion.possible_answer_3 = updatedQuestion.possible_answer_3;
        existingQuestion.possible_answer_4 = updatedQuestion.possible_answer_4;
        existingQuestion.correct_answer = updatedQuestion.correct_answer;

        await db.SaveChangesAsync();
        
        Console.WriteLine($"Successfully updated question with ID: {id}");
        return Results.Ok(existingQuestion);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating question {id}: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Updating Question",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapPost("/questions", async (AppDbContext db, Questions question) =>
{
    db.Questions.Add(question);
    await db.SaveChangesAsync();
    return Results.Created($"/questions/{question.question_id}", question);
});

app.MapDelete("/questions/{id}", async (AppDbContext db, int id) =>
{
    try
    {
        Console.WriteLine($"Attempting to delete question with ID: {id}");
        
        var questionToDelete = await db.Questions.FindAsync(id);
        if (questionToDelete == null)
        {
            Console.WriteLine($"Question not found with ID: {id}");
            return Results.NotFound($"Question with ID {id} not found");
        }

        db.Questions.Remove(questionToDelete);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"Successfully deleted question with ID: {id}");
        return Results.Ok(questionToDelete);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting question {id}: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Deleting Question",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

////////////////////////////////////////////////////////////////////
////////////////////////////////USER-TEST////////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/user-test", async (AppDbContext db, UserTest userTest) =>
{
    try
    {
        Console.WriteLine($"Attempting to create user-test association: User={userTest.Userid}, Test={userTest.Testtest_id}");
        
        // Validate user exists
        var user = await db.User.FindAsync(userTest.Userid);
        if (user == null)
        {
            Console.WriteLine($"User not found with ID: {userTest.Userid}");
            return Results.NotFound($"User with ID {userTest.Userid} not found");
        }

        // Validate test exists
        var test = await db.Test.FindAsync(userTest.Testtest_id);
        if (test == null)
        {
            Console.WriteLine($"Test not found with ID: {userTest.Testtest_id}");
            return Results.NotFound($"Test with ID {userTest.Testtest_id} not found");
        }

        // Check if association already exists
        var existingAssociation = await db.UserTest
            .FirstOrDefaultAsync(ut => ut.Userid == userTest.Userid && ut.Testtest_id == userTest.Testtest_id);
            
        if (existingAssociation != null)
        {
            Console.WriteLine($"Association already exists for User={userTest.Userid}, Test={userTest.Testtest_id}");
            return Results.Conflict("This user is already assigned to this test");
        }

        // Create the association
        db.UserTest.Add(userTest);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"Successfully created user-test association: User={userTest.Userid}, Test={userTest.Testtest_id}");
        return Results.Created($"/user-test/{userTest.Userid}-{userTest.Testtest_id}", userTest);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating user-test association: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Creating User-Test Association",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapGet("/user-test/user/{userId}", async (AppDbContext db, int userId) =>
    await db.UserTest.Where(ut => ut.Userid == userId).ToListAsync());

app.MapGet("/user-test/test/{testId}", async (AppDbContext db, int testId) =>
    await db.UserTest.Where(ut => ut.Testtest_id == testId).ToListAsync());

app.MapDelete("/user-test/{userId}-{testId}", async (AppDbContext db, int userId, int testId) =>
{
    try
    {
        Console.WriteLine($"Attempting to delete user-test association: User={userId}, Test={testId}");
        
        var association = await db.UserTest
            .FirstOrDefaultAsync(ut => ut.Userid == userId && ut.Testtest_id == testId);
            
        if (association == null)
        {
            Console.WriteLine($"User-test association not found for User={userId}, Test={testId}");
            return Results.NotFound("Association not found");
        }
        
        db.UserTest.Remove(association);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"Successfully deleted user-test association for User={userId}, Test={testId}");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting user-test association: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(
            title: "Error Deleting User-Test Association",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

////////////////////////////////////////////////////////////////////
////////////////////////////////TEST RESULTS////////////////////////////////
////////////////////////////////////////////////////////////////////

// Get all test results
app.MapGet("/test-results", async (AppDbContext db) =>
{
    try
    {
        var results = await db.Test_Results.ToListAsync();
        return Results.Ok(results);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting test results: {ex.Message}");
        return Results.Problem("Error retrieving test results");
    }
});

// Get test result by ID
app.MapGet("/test-results/{resultId}", async (AppDbContext db, int resultId) =>
{
    try
    {
        var result = await db.Test_Results.FindAsync(resultId);
        return result != null ? Results.Ok(result) : Results.NotFound("Test result not found");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting test result {resultId}: {ex.Message}");
        return Results.Problem($"Error retrieving test result {resultId}");
    }
});

// Get test results by test ID
app.MapGet("/test-results/by-test/{testId}", async (AppDbContext db, int testId) =>
{
    try
    {
        var results = await db.Test_Results
            .Where(r => r.Testtest_id == testId)
            .ToListAsync();
        return results.Any() ? Results.Ok(results) : Results.NotFound("No test results found for this test");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting test results for test {testId}: {ex.Message}");
        return Results.Problem($"Error retrieving test results for test {testId}");
    }
});

// Get test results by user ID
app.MapGet("/test-results/by-user/{userId}", async (AppDbContext db, int userId) =>
{
    try
    {
        var results = await db.Test_Results
            .Where(r => r.Userid == userId)
            .ToListAsync();
        return results.Any() ? Results.Ok(results) : Results.NotFound("No test results found for this user");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting test results for user {userId}: {ex.Message}");
        return Results.Problem($"Error retrieving test results for user {userId}");
    }
});

// Create new test result
app.MapPost("/test-results", async (AppDbContext db, [FromBody] Test_Results testResult) =>
{
    try
    {
        if (testResult == null)
            return Results.BadRequest("Invalid test result data");

        db.Test_Results.Add(testResult);
        await db.SaveChangesAsync();
        return Results.Created($"/test-results/{testResult.result_id}", testResult);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating test result: {ex.Message}");
        return Results.Problem("Error creating test result");
    }
});

// Update test result
app.MapPut("/test-results/{resultId}", async (AppDbContext db, int resultId, [FromBody] Test_Results updatedResult) =>
{
    try
    {
        var existingResult = await db.Test_Results.FindAsync(resultId);
        if (existingResult == null)
            return Results.NotFound("Test result not found");

        existingResult.score = updatedResult.score;
        existingResult.completion_date = updatedResult.completion_date;
        existingResult.total_questions = updatedResult.total_questions;

        await db.SaveChangesAsync();
        return Results.Ok(existingResult);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating test result {resultId}: {ex.Message}");
        return Results.Problem($"Error updating test result {resultId}");
    }
});

// Delete test result
app.MapDelete("/test-results/{resultId}", async (AppDbContext db, int resultId) =>
{
    try
    {
        var result = await db.Test_Results.FindAsync(resultId);
        if (result == null)
            return Results.NotFound("Test result not found");

        db.Test_Results.Remove(result);
        await db.SaveChangesAsync();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting test result {resultId}: {ex.Message}");
        return Results.Problem($"Error deleting test result {resultId}");
    }
});

// CV_Summarised endpoints
app.MapGet("/cv-summarised/by-user/{userId}", async (int userId, AppDbContext db) =>
{
    var cvSummarised = await db.CV_Summarised.FirstOrDefaultAsync(c => c.user_id == userId);
    return cvSummarised != null ? Results.Ok(cvSummarised) : Results.NotFound();
})
.WithName("GetCvSummarisedByUser");

app.MapPost("/cv-summarised", async (CV_Summarised cvSummarised, AppDbContext db) =>
{
    try
    {
        // Check if a record already exists for this user
        var existing = await db.CV_Summarised.FirstOrDefaultAsync(c => c.user_id == cvSummarised.user_id);
        
        if (existing != null)
        {
            // Update existing record
            existing.cv_id = cvSummarised.cv_id;
            existing.soft_skills = cvSummarised.soft_skills ?? string.Empty;
            existing.hard_skills = cvSummarised.hard_skills ?? string.Empty;
            db.CV_Summarised.Update(existing);
            await db.SaveChangesAsync();
            return Results.Ok(existing);
        }
        else
        {
            // For a new record, use direct SQL command to insert
            int userId = cvSummarised.user_id;
            int cvId = cvSummarised.cv_id;
            string softSkills = cvSummarised.soft_skills ?? string.Empty;
            string hardSkills = cvSummarised.hard_skills ?? string.Empty;
            
            // Execute a direct SQL INSERT command that doesn't include the identity column
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO CV_Summarised (user_id, cv_id, soft_skills, hard_skills) VALUES ({0}, {1}, {2}, {3})",
                userId, cvId, softSkills, hardSkills);
            
            // Retrieve the newly created record
            var newRecord = await db.CV_Summarised.FirstOrDefaultAsync(c => c.user_id == userId);
            if (newRecord == null)
            {
                return Results.Problem("Record was created but could not be retrieved");
            }
            
            return Results.Created($"/cv-summarised/by-user/{newRecord.user_id}", newRecord);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating CV_Summarised: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
        }
        return Results.Problem(
            detail: ex.InnerException?.Message ?? ex.Message,
            title: "Error creating CV_Summarised record",
            statusCode: 500
        );
    }
})
.WithName("AddCvSummarised");

app.MapDelete("/cv-summarised/by-user/{userId}", async (int userId, AppDbContext db) =>
{
    var cvSummarised = await db.CV_Summarised.FirstOrDefaultAsync(c => c.user_id == userId);
    if (cvSummarised == null) return Results.NotFound();
    
    db.CV_Summarised.Remove(cvSummarised);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteCvSummarised");

// Simple direct SQL endpoint for skills
app.MapPost("/cv-skills/simple", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        // Read the request body manually
        string requestBody;
        using (var reader = new StreamReader(context.Request.Body))
        {
            requestBody = await reader.ReadToEndAsync();
        }
        
        Console.WriteLine($"[SKILLS] Received request body: {requestBody}");
        
        // Parse the JSON manually to avoid any serialization issues
        var requestData = JsonSerializer.Deserialize<JsonElement>(requestBody);
        
        // Extract required values with detailed logging
        int userId = 0;
        int cvId = 0;
        string softSkills = "";
        string hardSkills = "";
        
        try {
            userId = requestData.GetProperty("user_id").GetInt32();
            Console.WriteLine($"[SKILLS] Parsed user_id: {userId}");
        } catch (Exception ex) {
            Console.WriteLine($"[SKILLS] Error parsing user_id: {ex.Message}");
            return Results.BadRequest("Invalid user_id format");
        }
        
        try {
            cvId = requestData.GetProperty("cv_id").GetInt32();
            Console.WriteLine($"[SKILLS] Parsed cv_id: {cvId}");
        } catch (Exception ex) {
            Console.WriteLine($"[SKILLS] Error parsing cv_id: {ex.Message}");
            return Results.BadRequest("Invalid cv_id format");
        }
        
        if (requestData.TryGetProperty("soft_skills", out JsonElement softSkillsElement)) {
            if (softSkillsElement.ValueKind != JsonValueKind.Null) {
                softSkills = softSkillsElement.GetString() ?? "";
            }
        }
        Console.WriteLine($"[SKILLS] Parsed soft_skills: {softSkills}");
        
        if (requestData.TryGetProperty("hard_skills", out JsonElement hardSkillsElement)) {
            if (hardSkillsElement.ValueKind != JsonValueKind.Null) {
                hardSkills = hardSkillsElement.GetString() ?? "";
            }
        }
        Console.WriteLine($"[SKILLS] Parsed hard_skills: {hardSkills}");
        
        // First check if we need to update or insert
        string sqlCheck = "SELECT COUNT(*) FROM CV_Summarised WHERE user_id = @userId";
        int count = 0;
        
        using (var connection = new Microsoft.Data.SqlClient.SqlConnection(db.Database.GetConnectionString()))
        {
            await connection.OpenAsync();
            
            using (var command = new Microsoft.Data.SqlClient.SqlCommand(sqlCheck, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                var result = await command.ExecuteScalarAsync();
                count = Convert.ToInt32(result);
                Console.WriteLine($"[SKILLS] Count of existing records: {count}");
            }
            
            // Now perform the update or insert
            string sql;
            if (count > 0)
            {
                // Update
                sql = @"
                    UPDATE CV_Summarised 
                    SET cv_id = @cvId, soft_skills = @softSkills, hard_skills = @hardSkills
                    WHERE user_id = @userId";
                
                Console.WriteLine("[SKILLS] Executing UPDATE SQL");
            }
            else
            {
                // Insert
                sql = @"
                    DECLARE @NextId INT;
                    SELECT @NextId = ISNULL(MAX(cv_sum_id), 0) + 1 FROM CV_Summarised;
                    
                    INSERT INTO CV_Summarised (cv_sum_id, user_id, cv_id, soft_skills, hard_skills)
                    VALUES (@NextId, @userId, @cvId, @softSkills, @hardSkills)";
                
                Console.WriteLine("[SKILLS] Executing INSERT SQL with explicit ID");
            }
            
            using (var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@cvId", cvId);
                command.Parameters.AddWithValue("@softSkills", softSkills);
                command.Parameters.AddWithValue("@hardSkills", hardSkills);
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"[SKILLS] SQL executed successfully. Rows affected: {rowsAffected}");
            }
        }
        
        return Results.Ok(new { 
            success = true, 
            message = "Skills saved successfully", 
            operation = count > 0 ? "updated" : "inserted",
            user_id = userId,
            cv_id = cvId
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SKILLS] Error in /cv-skills/simple: {ex.Message}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[SKILLS] Inner Exception: {ex.InnerException.Message}");
        }
        
        return Results.Problem(
            detail: ex.Message,
            title: "Error saving skills",
            statusCode: 500
        );
    }
})
.WithName("SimpleAddCvSkills");

// PDF text extraction endpoint
app.MapPost("/pdf/extract-text", async (HttpContext context) =>
{
    try
    {
        Console.WriteLine("[PDF] Received text extraction request");
        
        // Check if the request contains a file
        var form = await context.Request.ReadFormAsync();
        var file = form.Files.GetFile("file");
        
        if (file == null || file.Length == 0)
        {
            Console.WriteLine("[PDF] No file received or file is empty");
            return Results.BadRequest("No file received or file is empty");
        }
        
        Console.WriteLine($"[PDF] Received file of size {file.Length} bytes");
        
        // Read the file content
        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }
        
        // Extract text from PDF
        string extractedText = await ExtractTextFromPdfAsync(fileBytes);
        
        Console.WriteLine($"[PDF] Extracted {extractedText.Length} characters of text");
        
        // Return the extracted text
        return Results.Ok(new { Text = extractedText });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[PDF] Error extracting text: {ex.Message}");
        return Results.Problem(
            detail: ex.Message,
            title: "Error extracting text from PDF",
            statusCode: 500
        );
    }
})
.WithName("ExtractPdfText");

// PDF text extraction helper method
async Task<string> ExtractTextFromPdfAsync(byte[] pdfBytes)
{
    try
    {
        // For now, use a simple approach - you can replace this with a more robust library in production
        // iTextSharp, PdfPig, or other PDF libraries would be better for production use
        
        // For this sample, we'll use a simplified approach
        var extractedText = new StringBuilder();
        
        using (var stream = new MemoryStream(pdfBytes))
        {
            try
            {
                // Use the UglyToad.PdfPig library for text extraction
                // This needs to be installed via NuGet: dotnet add package UglyToad.PdfPig
                
                // As a fallback for this example, we'll extract some content
                // by looking for text-like patterns in the PDF data
                var pdfText = Encoding.UTF8.GetString(pdfBytes);
                
                // Basic regex to find text in PDF content
                var textMatches = System.Text.RegularExpressions.Regex.Matches(
                    pdfText, 
                    @"(\([\w\s\.,;:'""\-+=/\\!@#$%^&*()]+\))",
                    System.Text.RegularExpressions.RegexOptions.Compiled
                );
                
                foreach (System.Text.RegularExpressions.Match match in textMatches)
                {
                    var text = match.Groups[1].Value;
                    text = text.Trim('(', ')');
                    if (text.Length > 2 && text.All(c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)))
                    {
                        extractedText.AppendLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF] Error in PDF parsing: {ex.Message}");
                extractedText.AppendLine("Error: Could not parse PDF content.");
            }
        }
        
        // If we couldn't extract anything, provide a helpful message
        if (extractedText.Length == 0)
        {
            extractedText.AppendLine("Could not extract readable text from this PDF.");
            extractedText.AppendLine("The PDF might be scanned, encrypted, or contain only images.");
        }
        
        return extractedText.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[PDF] Error in ExtractTextFromPdfAsync: {ex.Message}");
        return $"Error extracting text: {ex.Message}";
    }
}

app.Run();

class AppDbContext : DbContext
{
    public DbSet<SuperUser> SuperUser { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Company> Company { get; set; }
    public DbSet<CvPdf> CvPdf { get; set; }
    public DbSet<Test> Test { get; set; }
    public DbSet<Questions> Questions { get; set; }
    public DbSet<UserTest> UserTest { get; set; }
    public DbSet<Test_Results> Test_Results { get; set; }
    public DbSet<CV_Summarised> CV_Summarised { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuperUser>()
            .HasKey(s => s.SuperUseremail);
        modelBuilder.Entity<SuperUser>()
            .ToTable("SuperUser");

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .ToTable("User");

        modelBuilder.Entity<Company>()
            .HasKey(c => c.Company_Name);
        modelBuilder.Entity<Company>()
            .ToTable("Company");

        modelBuilder.Entity<CvPdf>()
            .HasKey(c => c.cv_id);
        modelBuilder.Entity<CvPdf>()
            .ToTable("CV_as_PDF");

        modelBuilder.Entity<Test>()
            .HasKey(t => t.test_id);
        modelBuilder.Entity<Test>()
            .ToTable("Test");
        modelBuilder.Entity<Test>()
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(t => t.company_name);

        modelBuilder.Entity<Questions>()
            .HasKey(q => q.question_id);
        modelBuilder.Entity<Questions>()
            .ToTable("Questions");
        modelBuilder.Entity<Questions>()
            .HasOne<Test>()
            .WithMany()
            .HasForeignKey(q => q.test_id);

        modelBuilder.Entity<UserTest>()
            .HasKey(ut => new { ut.Userid, ut.Testtest_id });
        modelBuilder.Entity<UserTest>()
            .ToTable("User_Test");
        modelBuilder.Entity<UserTest>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(ut => ut.Userid);
        modelBuilder.Entity<UserTest>()
            .HasOne<Test>()
            .WithMany()
            .HasForeignKey(ut => ut.Testtest_id);

        modelBuilder.Entity<Test_Results>()
            .HasKey(tr => tr.result_id);
        modelBuilder.Entity<Test_Results>()
            .ToTable("Test_Results");
        modelBuilder.Entity<Test_Results>()
            .Property(tr => tr.Userid)
            .HasColumnName("user_id");
        modelBuilder.Entity<Test_Results>()
            .Property(tr => tr.Testtest_id)
            .HasColumnName("test_id");
        modelBuilder.Entity<Test_Results>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(tr => tr.Userid);
        modelBuilder.Entity<Test_Results>()
            .HasOne<Test>()
            .WithMany()
            .HasForeignKey(tr => tr.Testtest_id);

        // CV_Summarised configuration
        modelBuilder.Entity<CV_Summarised>()
            .HasKey(c => c.cv_sum_id);
            
        modelBuilder.Entity<CV_Summarised>()
            .Property(c => c.cv_sum_id)
            .UseIdentityColumn()
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<CV_Summarised>()
            .ToTable("CV_Summarised");
            
        modelBuilder.Entity<CV_Summarised>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.user_id);
            
        modelBuilder.Entity<CV_Summarised>()
            .HasOne<CvPdf>()
            .WithMany()
            .HasForeignKey(c => c.cv_id);
            
        modelBuilder.Entity<CV_Summarised>()
            .HasIndex(c => c.user_id)
            .IsUnique();
    }
}

class SuperUser
{
    [Key]
    public string SuperUseremail { get; set; }
    public string Password { get; set; }
}

class User
{
    [Key]
    public int Id { get; set; }
    public string Full_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Company_Name { get; set; }
}

class Company
{
    [Key]
    public string Company_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Nr_of_accounts { get; set; }
    public string SuperUseremail { get; set; }
}

public class LoginRequest
{
    public string SuperUseremail { get; set; }
    public string Password { get; set; }
}


public class LoginRequestUser
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CvPdf
{
    [Key]
    public int cv_id { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string file_name { get; set; }

    [Column(TypeName = "bigint")]
    public long file_size { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] file_data { get; set; }
}

public class CvPdfUploadRequest
{
    [Required]
    public int cv_id { get; set; }

    [Required]
    [MaxLength(255)]
    public string file_name { get; set; }

    [Required]
    public string file_data { get; set; }
}

public class Test
{
    [Key]
    public int test_id { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(35)")]
    public string test_name { get; set; }
    
    [Required]
    public int no_of_questions { get; set; }
    
    [Required]
    public int time_limit { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(35)")]
    public string company_name { get; set; }
}

public class Questions
{
    [Key]
    public int question_id { get; set; }
    
    public int test_id { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(2048)")]
    public string question_text { get; set; }
    
    [Column(TypeName = "varchar(255)")]
    public string possible_answer_1 { get; set; }
    
    [Column(TypeName = "varchar(255)")]
    public string possible_answer_2 { get; set; }
    
    [Column(TypeName = "varchar(255)")]
    public string possible_answer_3 { get; set; }
    
    [Column(TypeName = "varchar(255)")]
    public string possible_answer_4 { get; set; }
    
    [Required]
    [Column(TypeName = "varchar(255)")]
    public string correct_answer { get; set; }
}

public class UserTest
{
    public int Userid { get; set; }
    public int Testtest_id { get; set; }
}

public class Test_Results
{
    [Key]
    public int result_id { get; set; }
    public int Userid { get; set; }
    public int Testtest_id { get; set; }
    public DateTime completion_date { get; set; }
    public int score { get; set; }
    public int total_questions { get; set; }
}

public class CV_Summarised
{
    [Key]
    public int cv_sum_id { get; set; }
    
    [Required]
    public int user_id { get; set; }
    
    [Required]
    public int cv_id { get; set; }
    
    [MaxLength(1000)]
    public string soft_skills { get; set; }
    
    [MaxLength(1000)]
    public string hard_skills { get; set; }
}