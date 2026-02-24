-- CLEAR OLD EXPERIMENTS AND RESEED WITH EXPERIMENTS ONLY 
-- Run this to remove theory topics and keep only experiments

-- IMPORTANT: This will delete ALL experiments and related data
-- It will be reseeded when you restart the backend

-- Step 1: Delete all student progress (foreign key constraint)
DELETE FROM StudentProgress;
PRINT 'Deleted all student progress records';

-- Step 2: Delete all class experiment unlocks (foreign key constraint)  
DELETE FROM ClassExperimentUnlocks;
PRINT 'Deleted all class experiment unlocks';

-- Step 3: Delete all experiments
DELETE FROM Experiments;
PRINT 'Deleted all experiments';

-- Step 4: Reset identity seeds
DBCC CHECKIDENT ('StudentProgress', RESEED, 0);
DBCC CHECKIDENT ('ClassExperimentUnlocks', RESEED, 0);
DBCC CHECKIDENT ('Experiments', RESEED, 0);

PRINT '';
PRINT '========================================';
PRINT 'All experiments cleared!';
PRINT 'Restart the backend to reseed with EXPERIMENTS ONLY';
PRINT '';
PRINT 'New experiment counts (no theory topics):';
PRINT '  Level 1 (Mech Tech): 41 experiments';
PRINT '  Level 2 (Electronics): 37 experiments';
PRINT '  Level 3 (Electro Mechanical): 14 experiments';
PRINT '  Level 4 (Digi-Tech Coding): 36 programs';
PRINT '  Level 5 (Digi Sense): 19 experiments';
PRINT '  Level 6 (Wireless IoT): 13 experiments';
PRINT '  TOTAL: 160 experiments';
PRINT '========================================';
