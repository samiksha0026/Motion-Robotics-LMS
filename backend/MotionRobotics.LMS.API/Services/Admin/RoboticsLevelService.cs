using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IRoboticsLevelService
    {
        Task<List<RoboticsLevelDto>> GetAllLevelsAsync();
        Task<RoboticsLevelDetailDto?> GetLevelWithExperimentsAsync(int levelId);
        Task<bool> UpdateSyllabusUrlAsync(int levelId, string syllabusUrl);
        Task SeedSyllabusUrlsAsync();
        Task SeedSampleExperimentsAsync();
    }

    public class RoboticsLevelService : IRoboticsLevelService
    {
        private readonly ApplicationDbContext _context;

        public RoboticsLevelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoboticsLevelDto>> GetAllLevelsAsync()
        {
            return await _context.RoboticsLevels
                .OrderBy(rl => rl.LevelNumber)
                .Select(rl => new RoboticsLevelDto
                {
                    Id = rl.Id,
                    LevelNumber = rl.LevelNumber,
                    Name = rl.Name,
                    Description = rl.Description,
                    SyllabusUrl = rl.SyllabusUrl,
                    IsActive = rl.IsActive,
                    TotalExperiments = rl.Experiments.Count(e => e.IsActive)
                })
                .ToListAsync();
        }

        public async Task<RoboticsLevelDetailDto?> GetLevelWithExperimentsAsync(int levelId)
        {
            var level = await _context.RoboticsLevels
                .Include(rl => rl.Experiments.Where(e => e.IsActive).OrderBy(e => e.SequenceOrder))
                .FirstOrDefaultAsync(rl => rl.Id == levelId);

            if (level == null)
                return null;

            return new RoboticsLevelDetailDto
            {
                Id = level.Id,
                LevelNumber = level.LevelNumber,
                Name = level.Name,
                Description = level.Description,
                SyllabusUrl = level.SyllabusUrl,
                IsActive = level.IsActive,
                TotalExperiments = level.Experiments.Count,
                Experiments = level.Experiments.Select(e => new ExperimentSummaryDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    SequenceOrder = e.SequenceOrder,
                    EstimatedMinutes = e.EstimatedMinutes,
                    IsActive = e.IsActive
                }).ToList()
            };
        }

        public async Task<bool> UpdateSyllabusUrlAsync(int levelId, string syllabusUrl)
        {
            var level = await _context.RoboticsLevels.FindAsync(levelId);
            if (level == null)
                return false;

            level.SyllabusUrl = syllabusUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SeedSyllabusUrlsAsync()
        {
            var syllabusMap = new Dictionary<int, string>
            {
                { 1, "/syllabus/Mech Tech Level-1.pdf" },
                { 2, "/syllabus/Electronics Level 2.pdf" },
                { 3, "/syllabus/Electro Mechanical Level-3.pdf" },
                { 4, "/syllabus/Digi-Tech Coding Level 4.pdf" },
                { 5, "/syllabus/Digi sense Level 5.pdf" },
                { 6, "/syllabus/WIRELESS IOT pdf.pdf" }
            };

            foreach (var kvp in syllabusMap)
            {
                var level = await _context.RoboticsLevels.FirstOrDefaultAsync(l => l.LevelNumber == kvp.Key);
                if (level != null && string.IsNullOrEmpty(level.SyllabusUrl))
                {
                    level.SyllabusUrl = kvp.Value;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task SeedSampleExperimentsAsync()
        {
            // Check if experiments already exist
            if (await _context.Experiments.AnyAsync())
                return;

            var experiments = new List<Models.Experiment>
            {
                // Level 1 - Mech Tech
                new() { RoboticsLevelId = 1, SequenceOrder = 1, Title = "Introduction to Mechanics", Description = "Learn basic mechanical concepts", Objective = "Understand gears and pulleys", EstimatedMinutes = 30 },
                new() { RoboticsLevelId = 1, SequenceOrder = 2, Title = "Simple Machines", Description = "Build simple machines", Objective = "Create lever and pulley systems", EstimatedMinutes = 45 },
                new() { RoboticsLevelId = 1, SequenceOrder = 3, Title = "Motor Basics", Description = "Introduction to motors", Objective = "Connect and control DC motors", EstimatedMinutes = 40 },

                // Level 2 - Electronics
                new() { RoboticsLevelId = 2, SequenceOrder = 1, Title = "Introduction to Electronics", Description = "Learn basic electronic components", Objective = "Identify resistors, capacitors, and LEDs", EstimatedMinutes = 30 },
                new() { RoboticsLevelId = 2, SequenceOrder = 2, Title = "LED Circuit", Description = "Build your first LED circuit", Objective = "Create a simple LED circuit with resistor", EstimatedMinutes = 35 },
                new() { RoboticsLevelId = 2, SequenceOrder = 3, Title = "Series & Parallel Circuits", Description = "Understanding circuit configurations", Objective = "Build series and parallel LED circuits", EstimatedMinutes = 45 },
                new() { RoboticsLevelId = 2, SequenceOrder = 4, Title = "Switches & Buttons", Description = "Adding user control to circuits", Objective = "Use switches and push buttons", EstimatedMinutes = 40 },
                new() { RoboticsLevelId = 2, SequenceOrder = 5, Title = "Buzzer Circuit", Description = "Creating sound with electronics", Objective = "Build a buzzer alarm circuit", EstimatedMinutes = 35 },

                // Level 3 - Electro Mechanical
                new() { RoboticsLevelId = 3, SequenceOrder = 1, Title = "Motor Control Basics", Description = "Control motors with electronics", Objective = "Use transistors to control motors", EstimatedMinutes = 45 },
                new() { RoboticsLevelId = 3, SequenceOrder = 2, Title = "H-Bridge Motor Driver", Description = "Bidirectional motor control", Objective = "Build H-bridge for motor direction control", EstimatedMinutes = 50 },
                new() { RoboticsLevelId = 3, SequenceOrder = 3, Title = "Servo Motors", Description = "Precision motor control", Objective = "Control servo motor positions", EstimatedMinutes = 40 },

                // Level 4 - Digi-Coding
                new() { RoboticsLevelId = 4, SequenceOrder = 1, Title = "Introduction to Arduino", Description = "First steps with Arduino", Objective = "Setup Arduino IDE and blink LED", EstimatedMinutes = 45 },
                new() { RoboticsLevelId = 4, SequenceOrder = 2, Title = "Digital Inputs & Outputs", Description = "Reading and writing digital signals", Objective = "Use buttons and LEDs with code", EstimatedMinutes = 50 },
                new() { RoboticsLevelId = 4, SequenceOrder = 3, Title = "Analog Sensors", Description = "Reading analog values", Objective = "Use potentiometer and light sensor", EstimatedMinutes = 45 },

                // Level 5 - Digi-Sense
                new() { RoboticsLevelId = 5, SequenceOrder = 1, Title = "Temperature Sensor", Description = "Measuring temperature", Objective = "Read temperature with DHT11 sensor", EstimatedMinutes = 40 },
                new() { RoboticsLevelId = 5, SequenceOrder = 2, Title = "Ultrasonic Distance", Description = "Distance measurement", Objective = "Use ultrasonic sensor for distance", EstimatedMinutes = 45 },
                new() { RoboticsLevelId = 5, SequenceOrder = 3, Title = "IR Sensors", Description = "Infrared detection", Objective = "Obstacle detection with IR sensor", EstimatedMinutes = 40 },

                // Level 6 - Wireless & IOT
                new() { RoboticsLevelId = 6, SequenceOrder = 1, Title = "Bluetooth Communication", Description = "Wireless with Bluetooth", Objective = "Control robot via Bluetooth", EstimatedMinutes = 50 },
                new() { RoboticsLevelId = 6, SequenceOrder = 2, Title = "WiFi Basics", Description = "Internet connectivity", Objective = "Connect ESP8266 to WiFi", EstimatedMinutes = 55 },
                new() { RoboticsLevelId = 6, SequenceOrder = 3, Title = "IoT Dashboard", Description = "Cloud monitoring", Objective = "Send sensor data to cloud dashboard", EstimatedMinutes = 60 }
            };

            _context.Experiments.AddRange(experiments);
            await _context.SaveChangesAsync();
        }
    }
}
