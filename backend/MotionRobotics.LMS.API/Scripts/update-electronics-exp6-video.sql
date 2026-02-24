-- Update Electronics Experiment 6 with Demo Video URL
-- Run this against your database to add the demo video for the buzzer experiment

UPDATE Experiments 
SET DemoVideoUrl = '/videos/electronics-exp6-buzzer.mp4',
    UpdatedAt = GETUTCDATE()
WHERE Title LIKE '%EXP 6: Switching ON/OFF Buzzer%'
  AND RoboticsLevelId IN (SELECT Id FROM RoboticsLevels WHERE LevelNumber = 2);

-- Verify the update
SELECT Id, Title, DemoVideoUrl, RoboticsLevelId 
FROM Experiments 
WHERE Title LIKE '%EXP 6%' AND DemoVideoUrl IS NOT NULL;
