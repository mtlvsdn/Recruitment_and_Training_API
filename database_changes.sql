-- Drop existing table if it exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Test_Results')
BEGIN
    DROP TABLE Test_Results;
END
GO

-- Create the Test_Results table with correct column names
CREATE TABLE Test_Results (
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    test_id INT NOT NULL,
    completion_date DATETIME NOT NULL,
    score INT NOT NULL,
    total_questions INT NOT NULL,
    CONSTRAINT FK_TestResults_User FOREIGN KEY (user_id) REFERENCES [User](Id),
    CONSTRAINT FK_TestResults_Test FOREIGN KEY (test_id) REFERENCES Test(test_id)
);
GO

-- Create table for storing individual question results
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Question_Results')
BEGIN
    CREATE TABLE Question_Results (
        question_result_id INT IDENTITY(1,1) PRIMARY KEY,
        result_id INT NOT NULL,
        question_id INT NOT NULL,
        selected_answer VARCHAR(255) NOT NULL,
        is_correct BIT NOT NULL,
        CONSTRAINT FK_QuestionResults_TestResults FOREIGN KEY (result_id) REFERENCES Test_Results(result_id),
        CONSTRAINT FK_QuestionResults_Questions FOREIGN KEY (question_id) REFERENCES Questions(question_id)
    );
END
GO

-- Create indexes for better performance
CREATE INDEX IX_TestResults_UserId ON Test_Results(user_id);
CREATE INDEX IX_TestResults_TestId ON Test_Results(test_id);
GO

-- Check if Question_Results table exists before creating indexes
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Question_Results')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QuestionResults_ResultId')
        CREATE INDEX IX_QuestionResults_ResultId ON Question_Results(result_id);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QuestionResults_QuestionId')
        CREATE INDEX IX_QuestionResults_QuestionId ON Question_Results(question_id);
END

-- Create remaining indexes for better performance
-- First verify the column name in Test_Results table
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Test_Results';
GO

-- Show us the exact column names in the Test_Results table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Test_Results'
ORDER BY ORDINAL_POSITION;
GO

-- First, drop any existing foreign key constraints
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TestResults_User')
    ALTER TABLE Test_Results DROP CONSTRAINT FK_TestResults_User;
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TestResults_Test')
    ALTER TABLE Test_Results DROP CONSTRAINT FK_TestResults_Test;

-- Drop existing indexes if they exist
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TestResults_UserId')
    DROP INDEX IX_TestResults_UserId ON Test_Results;
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TestResults_TestId')
    DROP INDEX IX_TestResults_TestId ON Test_Results;

-- Add missing columns
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Test_Results' AND COLUMN_NAME = 'user_id')
    ALTER TABLE Test_Results ADD user_id INT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Test_Results' AND COLUMN_NAME = 'completion_date')
    ALTER TABLE Test_Results ADD completion_date DATETIME NOT NULL DEFAULT GETDATE();

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Test_Results' AND COLUMN_NAME = 'total_questions')
    ALTER TABLE Test_Results ADD total_questions INT NOT NULL DEFAULT 0;

-- Modify existing score column to be INT instead of VARCHAR
ALTER TABLE Test_Results ALTER COLUMN score INT NOT NULL;

-- Add foreign key constraints
ALTER TABLE Test_Results 
ADD CONSTRAINT FK_TestResults_User FOREIGN KEY (user_id) REFERENCES [User](Id);

ALTER TABLE Test_Results 
ADD CONSTRAINT FK_TestResults_Test FOREIGN KEY (test_id) REFERENCES Test(test_id);

-- Create indexes for better performance
CREATE INDEX IX_TestResults_UserId ON Test_Results(user_id);
CREATE INDEX IX_TestResults_TestId ON Test_Results(test_id); 