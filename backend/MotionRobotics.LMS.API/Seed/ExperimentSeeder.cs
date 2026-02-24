using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;

namespace MotionRobotics.LMS.API.Seed
{
    /// <summary>
    /// Seeds experiments for each robotics level based on the official curriculum.
    /// </summary>
    public static class ExperimentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if RoboticsLevels exist
            var levels = await context.RoboticsLevels.ToListAsync();
            if (!levels.Any())
            {
                Console.WriteLine("   ! Cannot seed experiments - RoboticsLevels not found. Run AdminSeeder first.");
                return;
            }

            // Update demo video URLs for existing experiments (always run this)
            await UpdateDemoVideoUrlsAsync(context, levels);

            // Only seed new experiments if none exist
            if (context.Experiments.Any())
            {
                Console.WriteLine("   - Experiments already seeded.");
                return;
            }

            var experiments = new List<Experiment>();

            // ========== LEVEL 1: MECH TECH (44 experiments) ==========
            var level1 = levels.FirstOrDefault(l => l.LevelNumber == 1);
            if (level1 != null)
            {
                var mechTechExperiments = new[]
                {
                    "Component Introduction",
                    "Alphabet A – Model Making",
                    "Shape Formation",
                    "Animal Character – 1",
                    "Animal Character – 2",
                    "Animal Character – 3",
                    "Animal Character – 4",
                    "Draw Your Favourite Animal Character",
                    "Animal Character – 5",
                    "Animal Character – 6",
                    "Animal Character – 7",
                    "Animal Character – 8",
                    "Fighter Jet",
                    "Animal Character – 10",
                    "Windmill Prototype",
                    "How Do Windmills Produce Energy?",
                    "About Fighter Jet",
                    "4W Tank – 1",
                    "4W Tank – 2",
                    "4W Rover – 3",
                    "4W Rover – 4",
                    "2W Rover",
                    "3W Rover",
                    "MR Bike",
                    "Bike Prototype",
                    "4W Rover – 5",
                    "4W Rover – 6",
                    "Chandrayaan-3 Rover (Pragyan)",
                    "4W Rover – 7",
                    "4W Rover – 8",
                    "4W Rover – 9",
                    "4W Rover – 10",
                    "Combat Rover – 1",
                    "Combat Rover – 2",
                    "MR Off Road 4W",
                    "Prototype MR Off Road 4W",
                    "MR Off Road 4W Crane",
                    "4W Open Rover – 1",
                    "4W Rover – 1",
                    "4W Rover – 3",
                    "4W Rover – 4"
                };

                for (int i = 0; i < mechTechExperiments.Length; i++)
                {
                    experiments.Add(new Experiment
                    {
                        RoboticsLevelId = level1.Id,
                        SequenceOrder = i + 1,
                        Title = mechTechExperiments[i],
                        Description = $"Level 1 - Mech Tech: {mechTechExperiments[i]}",
                        Objective = $"Learn and build: {mechTechExperiments[i]}",
                        EstimatedMinutes = 30,
                        IsActive = true
                    });
                }
                Console.WriteLine($"   - Added {mechTechExperiments.Length} experiments for Level 1: Mech Tech");
            }

            // Add experiments for other levels when data is provided
            // Level 2: Electronics - EXPERIMENTS ONLY (37 experiments)
            var level2 = levels.FirstOrDefault(l => l.LevelNumber == 2);
            if (level2 != null)
            {
                var electronicsExperiments = new (string Title, string Description)[]
                {
                    ("EXP 1: Exploring Simple LED Circuits", "Build your first LED circuit and understand current flow"),
                    ("EXP 2: Switching ON and OFF LED", "Control an LED using a switch"),
                    ("EXP 3: ON & OFF Two Devices Using Slide Switch", "Control multiple devices with one switch"),
                    ("EXP 4: Connecting Two LED in Parallel", "Build a parallel LED circuit"),
                    ("EXP 5: Connecting Two LED in Series", "Build a series LED circuit"),
                    ("EXP 6: Switching ON/OFF Buzzer by Switch", "Control a buzzer with a switch"),
                    ("EXP 7: Explore Magnetic Reed Switch", "Build a circuit with magnetic reed switch"),
                    ("EXP 8: Connecting Resistor in Circuit", "Add resistors to your circuits"),
                    ("EXP 9: Connecting LDR in Circuit", "Build a light-sensing circuit"),
                    ("EXP 10: Controlling Brightness Using Trimmer", "Adjust LED brightness with potentiometer"),
                    ("EXP 11: Connecting DC Motor with Switch", "Control a DC motor"),
                    ("EXP 12: Controlling Speed of DC Motor", "Variable speed motor control"),
                    ("EXP 13: Charging & Discharging Capacitor", "Understand capacitor behavior"),
                    ("EXP 14: Powering LED using Solar Panel", "Solar-powered LED circuit"),
                    ("EXP 15: Electricity Generation from Mini Dynamo", "Generate electricity mechanically"),
                    ("EXP 16: Traffic Signal Project", "Build a working traffic light"),
                    ("EXP 17: LED with 3V Battery", "Simple LED on breadboard"),
                    ("EXP 18: LED with Switch on Breadboard", "Switch-controlled LED on breadboard"),
                    ("EXP 19: Buzzer with 3V Battery", "Simple buzzer circuit"),
                    ("EXP 20: Buzzer with Switch", "Switch-controlled buzzer"),
                    ("EXP 21: LED with Resistor", "Current limiting with resistor"),
                    ("EXP 22: LED with LDR", "Light-activated LED"),
                    ("EXP 23: LED with 9V & Resistor", "Higher voltage LED circuit"),
                    ("EXP 24: Three LEDs in Series", "Series LED configuration"),
                    ("EXP 25: Green LED with 9V & Resistor", "Different LED colors"),
                    ("EXP 26: Two LEDs with Slide Switch", "Multi-LED switching"),
                    ("EXP 27: LED & Buzzer with Slide Switch", "Combined output devices"),
                    ("EXP 28: Charging & Discharging Capacitor (Breadboard)", "Capacitor on breadboard"),
                    ("EXP 29: Buzz Wire Game", "Build an interactive game"),
                    ("EXP 30: Identify Conductor/Insulator using LED", "Test material conductivity"),
                    ("EXP 31: Identify Conductor/Insulator using Buzzer", "Audio conductivity test"),
                    ("EXP 32: Testing Conductivity of Salt Water", "Liquid conductivity experiment"),
                    ("EXP 33: Measuring Resistance", "Use multimeter to measure resistance"),
                    ("EXP 34: Calculating Resistance by Colour Code", "Read resistor color bands"),
                    ("EXP 35: Measuring DC Voltage", "Voltage measurement with multimeter"),
                    ("EXP 36: Continuity Test", "Check circuit continuity"),
                    ("EXP 37: Checking LED using Diode Mode", "Test LEDs with multimeter")
                };

                for (int i = 0; i < electronicsExperiments.Length; i++)
                {
                    var exp = electronicsExperiments[i];
                    var experiment = new Experiment
                    {
                        RoboticsLevelId = level2.Id,
                        SequenceOrder = i + 1,
                        Title = exp.Title,
                        Description = exp.Description,
                        Objective = $"Complete: {exp.Title}",
                        EstimatedMinutes = 30,
                        IsActive = true
                    };

                    // Add demo video for EXP 6: Switching ON/OFF Buzzer by Switch
                    if (exp.Title.Contains("EXP 6: Switching ON/OFF Buzzer"))
                    {
                        experiment.DemoVideoUrl = "/videos/electronics-exp6-buzzer.mp4";
                    }

                    experiments.Add(experiment);
                }
                Console.WriteLine($"   - Added {electronicsExperiments.Length} experiments for Level 2: Electronics");
            }

            // ========== LEVEL 3: ELECTRO MECHANICAL (14 experiments) ==========
            var level3 = levels.FirstOrDefault(l => l.LevelNumber == 3);
            if (level3 != null)
            {
                var electroMechExperiments = new (string Title, string Description)[]
                {
                    ("EXP 1: Working of DPDT Switch", "Understand Double Pole Double Throw switch operation"),
                    ("EXP 2: Tri-Car Model", "Build a tri-car robotic model"),
                    ("EXP 3: Cursor Robotic Model", "Build a cursor-controlled robot"),
                    ("EXP 4: Sprinter Model", "Build a fast-moving sprinter robot"),
                    ("EXP 5: Roaster Robotic Model", "Build a rotating roaster robot"),
                    ("EXP 6: Rambler Robotic Model", "Build a rambler walking robot"),
                    ("EXP 7: Rickshaw Robotic Model", "Build a rickshaw-style robot"),
                    ("EXP 8: Bike Model", "Build a two-wheeled bike robot"),
                    ("EXP 9: Spartender Model", "Build a spartender robotic model"),
                    ("EXP 10: Making of Leading Stone (Compass)", "Create a magnetic compass"),
                    ("EXP 11: Magnetic Field Magic Experiment", "Explore magnetic field effects"),
                    ("EXP 12: Pencil Levitation using Magnet", "Magnetic levitation experiment"),
                    ("EXP 13: Making of Electromagnet using Copper Coil", "Build an electromagnet"),
                    ("EXP 14: Simple DC Motor using Copper Coil", "Build a DC motor from scratch")
                };

                for (int i = 0; i < electroMechExperiments.Length; i++)
                {
                    var exp = electroMechExperiments[i];
                    experiments.Add(new Experiment
                    {
                        RoboticsLevelId = level3.Id,
                        SequenceOrder = i + 1,
                        Title = exp.Title,
                        Description = exp.Description,
                        Objective = $"Complete: {exp.Title}",
                        EstimatedMinutes = 45,
                        IsActive = true
                    });
                }
                Console.WriteLine($"   - Added {electroMechExperiments.Length} experiments for Level 3: Electro Mechanical");
            }

            // ========== LEVEL 4: DIGI-TECH CODING (36 programs) ==========
            var level4 = levels.FirstOrDefault(l => l.LevelNumber == 4);
            if (level4 != null)
            {
                var digiCodingExperiments = new (string Title, string Description)[]
                {
                    ("Program 1: Hello World on Arduino", "Display Hello World on serial monitor"),
                    ("Program 2: Hello World using delay() function", "Using delay in programs"),
                    ("Program 3: Use of comment in coding", "Practice using comments"),
                    ("Program 4: Blinking LED using Arduino digital Signal", "Your first LED blink"),
                    ("Program 5: Controlling Brightness of LED using PWM", "LED dimming with PWM"),
                    ("Program 6: Fade in and Fade out of LED using PWM", "Smooth LED transitions"),
                    ("Program 7: Connecting two LEDs to Arduino", "Multiple LED control"),
                    ("Program 8: Blinking two LEDs at different times", "Independent LED control"),
                    ("Program 9: Traffic Light Simulation", "Build traffic light with Arduino"),
                    ("Program 10: Running LEDs Pattern", "Create LED chaser effect"),
                    ("Program 11: Push Button LED Control", "Control LED with button"),
                    ("Program 12: Toggle LED with Button", "Button toggle functionality"),
                    ("Program 13: Multiple Button LED Control", "Control multiple LEDs with buttons"),
                    ("Program 14: LED Brightness with Button", "Adjust brightness with button press"),
                    ("Program 15: Simple ON/OFF Buzzer", "Basic buzzer control"),
                    ("Program 16: Different frequency buzzer with Arduino", "Buzzer frequency control"),
                    ("Program 17: Creation of Buzzer alarm", "Build an alarm system"),
                    ("Program 18: Play a Melody using tone() function", "Music with Arduino"),
                    ("Program 19: Rotate Servo 0 to 180 forward and reverse", "Basic servo control"),
                    ("Program 20: Rotate Servo 10, 20, 30, 60, 90 degree", "Precise servo positioning"),
                    ("Program 21: Operate servo through Push button switch", "Button-controlled servo"),
                    ("Program 22: Operate servo motor through Potentiometer", "Analog servo control"),
                    ("Program 23: Operate servo motor through Joystick", "Joystick servo control"),
                    ("Program 24: Display Hello World on LCD", "LCD text display"),
                    ("Program 25: Show counter from 1 to 100 on LCD", "LCD counter display"),
                    ("Program 26: Show down counter on LCD with buzzer", "Countdown timer project"),
                    ("Program 27: Mario game using Arduino, LCD and Push button", "Build Mario game"),
                    ("Program 28: Print even numbers between 1 to 100", "Practice with loops"),
                    ("Program 29: Reading x axis and y axis value from joystick", "Joystick input reading"),
                    ("Program 30: Controlling servo using joystick", "Joystick servo integration"),
                    ("Program 31: Controlling brightness of LED using potentiometer", "Potentiometer LED control"),
                    ("Program 32: Controlling servo motor using potentiometer", "Potentiometer servo control"),
                    ("Program 33: Enter number through keypad on Serial monitor", "Keypad input basics"),
                    ("Program 34: Enter number through keypad on LCD", "Keypad LCD display"),
                    ("Program 35: Password protected lock using servo, LCD and keypad", "Security lock project"),
                    ("Program 36: Display different colors in RGB LED through for loop", "RGB LED color cycling")
                };

                for (int i = 0; i < digiCodingExperiments.Length; i++)
                {
                    var exp = digiCodingExperiments[i];
                    experiments.Add(new Experiment
                    {
                        RoboticsLevelId = level4.Id,
                        SequenceOrder = i + 1,
                        Title = exp.Title,
                        Description = exp.Description,
                        Objective = $"Complete: {exp.Title}",
                        EstimatedMinutes = 40,
                        IsActive = true
                    });
                }
                Console.WriteLine($"   - Added {digiCodingExperiments.Length} programs for Level 4: Digi-Tech Coding");
            }

            // ========== LEVEL 5: DIGI SENSE (19 experiments) ==========
            var level5 = levels.FirstOrDefault(l => l.LevelNumber == 5);
            if (level5 != null)
            {
                var digiSenseExperiments = new (string Title, string Description)[]
                {
                    // Temperature & Humidity Sensor (DHT11)
                    ("EXP 1: Reading Temperature & Humidity", "Read sensor data with Arduino"),
                    ("EXP 2: Weather Station using DHT & Arduino", "Build a mini weather station"),
                    ("EXP 3: Temperature Alarm System", "Alert when temperature exceeds threshold"),
                    
                    // Sound Sensor Module
                    ("EXP 4: Sound Detection using Arduino", "Detect sound levels with Arduino"),
                    ("EXP 5: Clap Activated LED", "Control LED with clap sounds"),
                    ("EXP 6: Secure Door Opening System", "Sound-activated security system"),
                    
                    // IR Sensor Module
                    ("EXP 7: Object Detection", "Detect objects using IR sensor"),
                    ("EXP 8: Auto Gate System", "Automatic gate using IR detection"),
                    ("EXP 9: RPM Counter", "Measure rotation speed with IR sensor"),
                    
                    // Ultrasonic Sensor (HC-SR04)
                    ("EXP 10: Distance Measurement", "Measure distance with ultrasonic waves"),
                    ("EXP 11: Auto Gate using Ultrasonic", "Distance-based automatic gate"),
                    ("EXP 12: Radar System", "Build a simple radar display"),
                    ("EXP 13: Blind Assistance System", "Help visually impaired with distance alerts"),
                    
                    // Light Dependent Resistor (LDR)
                    ("EXP 14: Light Detection", "Detect ambient light levels"),
                    ("EXP 15: Automatic Street Light", "Light that turns on when dark"),
                    ("EXP 16: Automatic Shade System", "Auto shade based on sunlight"),
                    
                    // Flame Sensor Module
                    ("EXP 17: Fire Alert using Buzzer", "Fire detection with audio alert"),
                    ("EXP 18: Fire Alert with LCD", "Fire detection with LCD display"),
                    ("EXP 19: Fire Alert Auto Gate System", "Emergency gate system for fire")
                };

                for (int i = 0; i < digiSenseExperiments.Length; i++)
                {
                    var exp = digiSenseExperiments[i];
                    experiments.Add(new Experiment
                    {
                        RoboticsLevelId = level5.Id,
                        SequenceOrder = i + 1,
                        Title = exp.Title,
                        Description = exp.Description,
                        Objective = $"Complete: {exp.Title}",
                        EstimatedMinutes = 45,
                        IsActive = true
                    });
                }
                Console.WriteLine($"   - Added {digiSenseExperiments.Length} experiments for Level 5: Digi Sense");
            }

            // ========== LEVEL 6: WIRELESS IoT (13 experiments) ==========
            var level6 = levels.FirstOrDefault(l => l.LevelNumber == 6);
            if (level6 != null)
            {
                var wirelessIoTExperiments = new (string Title, string Description)[]
                {
                    // DC Motor & L298N Driver
                    ("EXP 1: Controlling DC Motor Rotation using L298N", "Basic motor direction control"),
                    ("EXP 2: Controlling DC Motor Speed using L298N", "PWM speed control with L298N"),
                    ("EXP 3: Controlling Two DC Motors using L298N", "Dual motor control"),
                    ("EXP 4: Programmed Automatic Robot using L298N", "Autonomous robot movements"),
                    
                    // Robot Navigation
                    ("EXP 5: Line Following Robot", "Build a line tracking robot"),
                    ("EXP 6: Edge Avoiding Robot", "Robot that detects edges and stops"),
                    
                    // Bluetooth Communication
                    ("EXP 7: ON/OFF LED using Bluetooth (Without Shield)", "Basic Bluetooth LED control"),
                    ("EXP 8: Sending Data via Bluetooth and Display on LCD", "Bluetooth data to LCD"),
                    ("EXP 9: ON/OFF LED using Switch via Bluetooth", "Remote switch control"),
                    ("EXP 10: ON/OFF LED using Smartphone via Bluetooth", "Smartphone LED control"),
                    ("EXP 11: Bluetooth Control Robot using Joystick or Smartphone", "Build a Bluetooth RC robot"),
                    
                    // NRF / Radio Communication
                    ("EXP 12: Turn ON/OFF LED using NRF Module", "Basic NRF wireless control"),
                    ("EXP 13: Wireless Robot Controlling using Joystick", "Build a wireless RC robot")
                };

                for (int i = 0; i < wirelessIoTExperiments.Length; i++)
                {
                    var exp = wirelessIoTExperiments[i];
                    experiments.Add(new Experiment
                    {
                        RoboticsLevelId = level6.Id,
                        SequenceOrder = i + 1,
                        Title = exp.Title,
                        Description = exp.Description,
                        Objective = $"Complete: {exp.Title}",
                        EstimatedMinutes = 60,
                        IsActive = true
                    });
                }
                Console.WriteLine($"   - Added {wirelessIoTExperiments.Length} experiments for Level 6: Wireless IoT");
            }

            if (experiments.Any())
            {
                context.Experiments.AddRange(experiments);
                await context.SaveChangesAsync();
                Console.WriteLine($"   - Total {experiments.Count} experiments seeded successfully.");
            }
        }

        /// <summary>
        /// Updates demo video URLs for existing experiments.
        /// Add more demo videos here as needed.
        /// </summary>
        private static async Task UpdateDemoVideoUrlsAsync(ApplicationDbContext context, List<RoboticsLevel> levels)
        {
            var updatedCount = 0;

            // Electronics Level 2: Experiment 6 - Buzzer Switch
            var electronicsLevel = levels.FirstOrDefault(l => l.LevelNumber == 2);
            if (electronicsLevel != null)
            {
                var buzzerExp = await context.Experiments
                    .FirstOrDefaultAsync(e => e.RoboticsLevelId == electronicsLevel.Id
                        && e.Title.Contains("EXP 6: Switching ON/OFF Buzzer"));

                if (buzzerExp != null && string.IsNullOrEmpty(buzzerExp.DemoVideoUrl))
                {
                    buzzerExp.DemoVideoUrl = "/videos/electronics-exp6-buzzer.mp4";
                    buzzerExp.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"   - Updated {updatedCount} experiment(s) with demo video URLs.");
            }
        }
    }
}
