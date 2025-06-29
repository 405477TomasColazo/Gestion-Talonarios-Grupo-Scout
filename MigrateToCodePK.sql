-- Database Migration Script: Convert Tickets table to use Code as Primary Key
-- IMPORTANT: Backup your database before running this script!

-- Step 1: Check current data integrity
SELECT 'Current Tickets count:' as Info, COUNT(*) as Count FROM Tickets;
SELECT 'Tickets with duplicate codes:' as Info, COUNT(*) as Count 
FROM (
    SELECT code 
    FROM Tickets 
    GROUP BY code 
    HAVING COUNT(*) > 1
) duplicates;

-- Step 2: Check if there are any foreign key references to the id column
-- (If you have other tables that reference Tickets.id, you'll need to update those first)
SELECT 
    OBJECT_NAME(parent_object_id) AS referencing_table,
    name AS foreign_key_name,
    OBJECT_NAME(referenced_object_id) AS referenced_table
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('Tickets');

-- Step 3: Create backup table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets_Backup]') AND type in (N'U'))
BEGIN
    SELECT * INTO Tickets_Backup FROM Tickets;
    PRINT 'Backup table Tickets_Backup created successfully';
END

-- Step 4: Create new table structure with code as primary key
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tickets_New]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tickets_New (
        code INT PRIMARY KEY,
        seller_id INT NOT NULL,
        client_id INT NOT NULL,
        unit_cost DECIMAL(10,2) NOT NULL,
        traditional_qty INT NOT NULL DEFAULT 0,
        vegan_qty INT NOT NULL DEFAULT 0,
        is_paid BIT NOT NULL DEFAULT 0,
        is_delivered BIT NOT NULL DEFAULT 0,
        sold BIT NOT NULL DEFAULT 1,
        observations NVARCHAR(1000) NULL,
        withdrawal_time DATETIME2 NULL,
        sale_date DATETIME2 NOT NULL DEFAULT GETDATE(),
        payment_date DATETIME2 NULL
    );
    PRINT 'New table Tickets_New created successfully';
END

-- Step 5: Migrate data to new table structure
INSERT INTO Tickets_New (
    code, seller_id, client_id, unit_cost, 
    traditional_qty, vegan_qty, is_paid, is_delivered, 
    sold, observations, withdrawal_time, sale_date, payment_date
)
SELECT 
    code, seller_id, client_id, unit_cost,
    traditional_qty, vegan_qty, is_paid, is_delivered,
    sold, observations, withdrawal_time, sale_date, payment_date
FROM Tickets
WHERE code NOT IN (SELECT code FROM Tickets_New); -- Avoid duplicates if running multiple times

PRINT 'Data migrated to Tickets_New successfully';

-- Step 6: Verify migration
SELECT 'Original table count:' as Info, COUNT(*) as Count FROM Tickets;
SELECT 'New table count:' as Info, COUNT(*) as Count FROM Tickets_New;

-- Step 7: Instructions for final cutover (manual steps)
PRINT '=== MANUAL STEPS FOR FINAL CUTOVER ===';
PRINT '1. Stop the application';
PRINT '2. Run the following commands to replace the original table:';
PRINT '   DROP TABLE Tickets;';
PRINT '   EXEC sp_rename ''Tickets_New'', ''Tickets'';';
PRINT '3. Verify the application works correctly';
PRINT '4. If everything works, you can drop the backup table:';
PRINT '   DROP TABLE Tickets_Backup;';

-- Optional: Show the commands for immediate execution (uncomment if you're confident)
/*
-- DANGER ZONE: Uncomment only if you're ready to make the change immediately
-- and have verified everything is correct

BEGIN TRANSACTION;

-- Drop original table and rename new one
DROP TABLE Tickets;
EXEC sp_rename 'Tickets_New', 'Tickets';

-- Verify the change
SELECT 'Final table structure verification:' as Info;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Tickets'
ORDER BY ORDINAL_POSITION;

COMMIT TRANSACTION;
PRINT 'Migration completed successfully!';
*/