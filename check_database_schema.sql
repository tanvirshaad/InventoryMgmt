-- Check if all required columns exist in the Inventories table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Inventories'
ORDER BY COLUMN_NAME;

-- Check if the table exists
SELECT COUNT(*) as TableExists
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Inventories';
