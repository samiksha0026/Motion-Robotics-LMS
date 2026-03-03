"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { 
  Clock, AlertCircle, CheckCircle, XCircle, Send,
  BookOpen, Image as ImageIcon, Edit3, Trophy,
  ChevronRight, ArrowLeft, Eye, EyeOff, Loader2
} from "lucide-react";

import { API_BASE_URL } from '@/lib/api-config';

interface Answer {
  answer: string | number;
  type: string;
}

interface ScoreData {
  total: number;
  max: number;
  percentage: number;
}

interface ExamQuestion {
  id: number;
  question: string;
  options?: string[];
  correctAnswer?: number;
  marks: number;
  imageUrl?: string;
  imageAlt?: string;
  answer?: string;
  acceptedAnswers?: string[];
}

interface ExamSection {
  id: string;
  title: string;
  type: string;
  totalQuestions: number;
  marks: number;
  questions: ExamQuestion[];
}

interface ExamData {
  id: number;
  title: string;
  level: string;
  totalQuestions: number;
  duration: number;
  passingScore: number;
  sections: ExamSection[];
}

// Exam data for all 6 robotics levels
const examDataByLevel: Record<string, ExamData> = {
  "Mech-Tech": {
    id: 1,
    title: "Mech-Tech Level - Final Examination",
    level: "Mech-Tech (Mechanical Technology)",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "What type of simple machine is a screwdriver?", options: ["Lever", "Wheel and Axle", "Inclined Plane", "Pulley"], correctAnswer: 1, marks: 4 },
          { id: 2, question: "Which gear arrangement increases speed but decreases torque?", options: ["Large gear driving small gear", "Small gear driving large gear", "Same size gears", "Bevel gears"], correctAnswer: 0, marks: 4 },
          { id: 3, question: "What is the primary function of a bearing in a mechanical system?", options: ["Increase friction", "Reduce friction", "Generate power", "Store energy"], correctAnswer: 1, marks: 4 },
          { id: 4, question: "Which material is best suited for making robot chassis?", options: ["Glass", "Aluminum", "Paper", "Cloth"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "What does a cam mechanism convert?", options: ["Rotary motion to linear motion", "Linear motion to rotary motion", "Heat to motion", "Light to motion"], correctAnswer: 0, marks: 4 },
          { id: 6, question: "Which joint allows rotation in one axis only?", options: ["Ball joint", "Hinge joint", "Universal joint", "Sliding joint"], correctAnswer: 1, marks: 4 },
          { id: 7, question: "What is the mechanical advantage of a pulley system with 4 pulleys?", options: ["1", "2", "4", "8"], correctAnswer: 2, marks: 4 },
          { id: 8, question: "Which type of belt is used for high-power transmission?", options: ["Flat belt", "V-belt", "Round belt", "Ribbon belt"], correctAnswer: 1, marks: 4 },
          { id: 9, question: "What happens when you increase the number of teeth on a gear?", options: ["Speed increases", "Speed decreases", "No change", "Gear breaks"], correctAnswer: 1, marks: 4 },
          { id: 10, question: "Which tool is used to measure the diameter of a shaft?", options: ["Ruler", "Vernier Caliper", "Thermometer", "Voltmeter"], correctAnswer: 1, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "A _____ is used to convert rotary motion into reciprocating motion.", answer: "cam", acceptedAnswers: ["cam", "Cam", "CAM", "cam mechanism"], marks: 4 },
          { id: 12, question: "The gear ratio is calculated by dividing the number of teeth on the _____ gear by the driven gear.", answer: "driver", acceptedAnswers: ["driver", "driving", "Driver", "Driving"], marks: 4 },
          { id: 13, question: "A _____ joint allows movement in all directions.", answer: "ball", acceptedAnswers: ["ball", "Ball", "ball and socket", "spherical"], marks: 4 },
          { id: 14, question: "The force multiplied by distance is called _____.", answer: "torque", acceptedAnswers: ["torque", "Torque", "TORQUE", "moment"], marks: 4 },
          { id: 15, question: "_____ are used to fasten two parts together temporarily.", answer: "screws", acceptedAnswers: ["screws", "Screws", "bolts", "Bolts", "nuts and bolts"], marks: 4 }
        ]
      }
    ]
  },
  "Electronics": {
    id: 2,
    title: "Electronics Level - Final Examination",
    level: "Electronics",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "What is the unit of electrical resistance?", options: ["Volt", "Ampere", "Ohm", "Watt"], correctAnswer: 2, marks: 4 },
          { id: 2, question: "Which component stores electrical energy in an electric field?", options: ["Resistor", "Capacitor", "Inductor", "Diode"], correctAnswer: 1, marks: 4 },
          { id: 3, question: "What color bands represent a 470Ω resistor?", options: ["Yellow, Violet, Brown", "Yellow, Violet, Red", "Orange, Violet, Brown", "Red, Violet, Brown"], correctAnswer: 0, marks: 4 },
          { id: 4, question: "An LED needs a _____ to prevent burning out.", options: ["Capacitor", "Resistor", "Inductor", "Transistor"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "What does a buzzer do in a circuit?", options: ["Produces light", "Produces sound", "Stores energy", "Measures voltage"], correctAnswer: 1, marks: 4 },
          { id: 6, question: "Which terminal of an LED is longer?", options: ["Cathode (negative)", "Anode (positive)", "Ground", "Both are equal"], correctAnswer: 1, marks: 4 },
          { id: 7, question: "What is the typical forward voltage drop of a red LED?", options: ["0.5V", "1.2V", "1.8-2.2V", "5V"], correctAnswer: 2, marks: 4 },
          { id: 8, question: "Which law states V = I × R?", options: ["Kirchhoff's Law", "Ohm's Law", "Faraday's Law", "Newton's Law"], correctAnswer: 1, marks: 4 },
          { id: 9, question: "What happens when resistors are connected in series?", options: ["Total resistance decreases", "Total resistance increases", "Total resistance stays same", "Circuit breaks"], correctAnswer: 1, marks: 4 },
          { id: 10, question: "A switch in OFF position creates a _____ circuit.", options: ["Closed", "Open", "Short", "Complete"], correctAnswer: 1, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "The positive terminal of a battery is called the _____.", answer: "anode", acceptedAnswers: ["anode", "Anode", "ANODE", "positive"], marks: 4 },
          { id: 12, question: "Current flows from _____ to negative in a circuit.", answer: "positive", acceptedAnswers: ["positive", "Positive", "POSITIVE", "+"], marks: 4 },
          { id: 13, question: "A _____ is used to control the flow of current in one direction only.", answer: "diode", acceptedAnswers: ["diode", "Diode", "DIODE"], marks: 4 },
          { id: 14, question: "The unit of current is _____.", answer: "ampere", acceptedAnswers: ["ampere", "Ampere", "amp", "Amp", "A"], marks: 4 },
          { id: 15, question: "_____ is the opposition to the flow of electric current.", answer: "resistance", acceptedAnswers: ["resistance", "Resistance", "RESISTANCE"], marks: 4 }
        ]
      }
    ]
  },
  "Electro-Mechanical": {
    id: 3,
    title: "Electro-Mechanical Level - Final Examination",
    level: "Electro-Mechanical",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "Which motor type is best for precise position control?", options: ["DC Motor", "Servo Motor", "AC Motor", "Stepper Motor"], correctAnswer: 1, marks: 4 },
          { id: 2, question: "What component is used to drive a DC motor with microcontroller?", options: ["Resistor", "Motor Driver (L298N/L293D)", "Capacitor", "LED"], correctAnswer: 1, marks: 4 },
          { id: 3, question: "A relay is used to:", options: ["Measure voltage", "Control high power devices with low power signals", "Store energy", "Generate light"], correctAnswer: 1, marks: 4 },
          { id: 4, question: "What is PWM used for in motor control?", options: ["Changing direction", "Controlling speed", "Measuring current", "Generating sound"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "Which sensor detects when a robot hits an obstacle?", options: ["Light sensor", "Bump/Touch sensor", "Temperature sensor", "Humidity sensor"], correctAnswer: 1, marks: 4 },
          { id: 6, question: "A solenoid converts:", options: ["Electrical energy to mechanical motion", "Light to electricity", "Sound to electricity", "Heat to motion"], correctAnswer: 0, marks: 4 },
          { id: 7, question: "What is the typical operating voltage of a small DC motor?", options: ["1V", "3-12V", "220V", "1000V"], correctAnswer: 1, marks: 4 },
          { id: 8, question: "To reverse a DC motor direction, you need to:", options: ["Increase voltage", "Decrease current", "Swap the power connections", "Add more resistors"], correctAnswer: 2, marks: 4 },
          { id: 9, question: "What protects a circuit from motor back-EMF?", options: ["Resistor", "Flyback diode", "LED", "Switch"], correctAnswer: 1, marks: 4 },
          { id: 10, question: "A gear motor provides:", options: ["Higher speed, lower torque", "Lower speed, higher torque", "Same speed and torque", "No mechanical advantage"], correctAnswer: 1, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "A servo motor can rotate to a specific _____ based on the control signal.", answer: "angle", acceptedAnswers: ["angle", "Angle", "position", "Position"], marks: 4 },
          { id: 12, question: "The L298N is a dual H-bridge motor _____ IC.", answer: "driver", acceptedAnswers: ["driver", "Driver", "DRIVER"], marks: 4 },
          { id: 13, question: "PWM stands for Pulse Width _____.", answer: "Modulation", acceptedAnswers: ["Modulation", "modulation", "MODULATION"], marks: 4 },
          { id: 14, question: "A _____ switch is activated when physical contact is made.", answer: "limit", acceptedAnswers: ["limit", "Limit", "touch", "Touch", "bump", "Bump"], marks: 4 },
          { id: 15, question: "The rotating part of a motor is called the _____.", answer: "rotor", acceptedAnswers: ["rotor", "Rotor", "ROTOR", "armature"], marks: 4 }
        ]
      }
    ]
  },
  "Digi-Coding": {
    id: 4,
    title: "Digi-Coding Level - Final Examination",
    level: "Digi-Coding (Digital Coding)",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "What does Arduino use to control the brightness of an LED?", options: ["Digital Signal", "PWM (Pulse Width Modulation)", "Analog Input", "Serial Communication"], correctAnswer: 1, marks: 4 },
          { id: 2, question: "Which programming structure is used to repeat a block of code multiple times?", options: ["If-else statement", "Switch case", "Loop (for/while)", "Function"], correctAnswer: 2, marks: 4 },
          { id: 3, question: "In Arduino, what does 'void setup()' function do?", options: ["Runs continuously", "Runs only once at startup", "Handles errors", "Stores variables"], correctAnswer: 1, marks: 4 },
          { id: 4, question: "Which function reads digital input from a pin?", options: ["analogRead()", "digitalRead()", "Serial.read()", "pinMode()"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "What is the correct way to set pin 13 as output?", options: ["pinMode(13, INPUT)", "pinMode(13, OUTPUT)", "digitalWrite(13, HIGH)", "digitalRead(13)"], correctAnswer: 1, marks: 4 },
          { id: 6, question: "What value does digitalWrite(pin, HIGH) set?", options: ["0V", "2.5V", "5V", "3.3V"], correctAnswer: 2, marks: 4 },
          { id: 7, question: "The delay(1000) function pauses execution for:", options: ["1 second", "1 minute", "1 millisecond", "1 hour"], correctAnswer: 0, marks: 4 },
          { id: 8, question: "Which symbol is used for comments in Arduino code?", options: ["#", "//", "**", "%%"], correctAnswer: 1, marks: 4 },
          { id: 9, question: "What does 'int' mean in Arduino programming?", options: ["Internet", "Integer (whole number)", "Internal", "Interface"], correctAnswer: 1, marks: 4 },
          { id: 10, question: "Serial.begin(9600) sets the:", options: ["Pin number", "Baud rate for communication", "Voltage level", "Timer interval"], correctAnswer: 1, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "The _____ function is used to read digital input from a pin in Arduino.", answer: "digitalRead", acceptedAnswers: ["digitalRead", "digitalread", "DIGITALREAD"], marks: 4 },
          { id: 12, question: "To control the speed of a DC motor, we use _____ signals.", answer: "PWM", acceptedAnswers: ["PWM", "pwm", "Pulse Width Modulation"], marks: 4 },
          { id: 13, question: "The default baud rate for Serial communication in Arduino is _____.", answer: "9600", acceptedAnswers: ["9600"], marks: 4 },
          { id: 14, question: "The _____ loop runs continuously after setup() completes.", answer: "loop", acceptedAnswers: ["loop", "Loop", "void loop", "loop()"], marks: 4 },
          { id: 15, question: "Complete the code: pinMode(13, _____); // Set pin 13 as output", answer: "OUTPUT", acceptedAnswers: ["OUTPUT", "output"], marks: 4 }
        ]
      }
    ]
  },
  "Digi-Sense": {
    id: 5,
    title: "Digi-Sense Level - Final Examination",
    level: "Digi-Sense (Digital Sensors)",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "Which sensor measures distance using sound waves?", options: ["IR Sensor", "Ultrasonic Sensor", "Light Sensor", "Temperature Sensor"], correctAnswer: 1, marks: 4 },
          { id: 2, question: "An LDR (Light Dependent Resistor) changes its resistance based on:", options: ["Temperature", "Sound", "Light intensity", "Humidity"], correctAnswer: 2, marks: 4 },
          { id: 3, question: "Which sensor is best for line-following robots?", options: ["Ultrasonic sensor", "IR sensor", "Temperature sensor", "Humidity sensor"], correctAnswer: 1, marks: 4 },
          { id: 4, question: "PIR sensor is used to detect:", options: ["Light", "Motion/Heat from humans", "Sound", "Distance"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "What does an analog sensor provide?", options: ["Only HIGH or LOW", "Range of values (0-1023)", "Text data", "Images"], correctAnswer: 1, marks: 4 },
          { id: 6, question: "Which function reads analog sensor values in Arduino?", options: ["digitalRead()", "analogRead()", "Serial.read()", "analogWrite()"], correctAnswer: 1, marks: 4 },
          { id: 7, question: "The DHT11 sensor measures:", options: ["Distance and speed", "Temperature and humidity", "Light and sound", "Pressure and altitude"], correctAnswer: 1, marks: 4 },
          { id: 8, question: "What is the range of values from analogRead()?", options: ["0-100", "0-255", "0-1023", "0-5000"], correctAnswer: 2, marks: 4 },
          { id: 9, question: "A touch sensor works similar to a:", options: ["Resistor", "Capacitor", "Push button", "LED"], correctAnswer: 2, marks: 4 },
          { id: 10, question: "Which sensor can detect different colors?", options: ["Ultrasonic", "Color sensor (TCS3200)", "PIR sensor", "Sound sensor"], correctAnswer: 1, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "An ultrasonic sensor uses _____ waves to measure distance.", answer: "sound", acceptedAnswers: ["sound", "Sound", "ultrasonic", "Ultrasonic"], marks: 4 },
          { id: 12, question: "IR stands for _____.", answer: "Infrared", acceptedAnswers: ["Infrared", "infrared", "INFRARED"], marks: 4 },
          { id: 13, question: "The _____ sensor can detect motion by sensing body heat.", answer: "PIR", acceptedAnswers: ["PIR", "pir", "motion", "Motion"], marks: 4 },
          { id: 14, question: "analogRead() returns values from 0 to _____.", answer: "1023", acceptedAnswers: ["1023"], marks: 4 },
          { id: 15, question: "LDR stands for Light _____ Resistor.", answer: "Dependent", acceptedAnswers: ["Dependent", "dependent", "DEPENDENT"], marks: 4 }
        ]
      }
    ]
  },
  "Wireless & IOT": {
    id: 6,
    title: "Wireless & IOT Level - Final Examination",
    level: "Wireless & IOT (Internet of Things)",
    totalQuestions: 15,
    duration: 45,
    passingScore: 70,
    sections: [
      {
        id: "section-a",
        title: "Section A: Multiple Choice Questions",
        type: "mcq",
        totalQuestions: 10,
        marks: 40,
        questions: [
          { id: 1, question: "What does IOT stand for?", options: ["Internet of Things", "Input Output Technology", "Integrated Online Terminal", "Internal Operating Tool"], correctAnswer: 0, marks: 4 },
          { id: 2, question: "Which module is commonly used for WiFi in Arduino projects?", options: ["HC-05", "ESP8266/ESP32", "NRF24L01", "SIM800"], correctAnswer: 1, marks: 4 },
          { id: 3, question: "Bluetooth module HC-05 uses which type of communication?", options: ["WiFi", "Bluetooth Serial", "Infrared", "Zigbee"], correctAnswer: 1, marks: 4 },
          { id: 4, question: "What protocol is commonly used for IOT communication?", options: ["FTP", "MQTT", "POP3", "SMTP"], correctAnswer: 1, marks: 4 },
          { id: 5, question: "Which platform is popular for IOT dashboards?", options: ["Microsoft Word", "Blynk/ThingSpeak", "Photoshop", "Excel"], correctAnswer: 1, marks: 4 },
          { id: 6, question: "The ESP8266 can connect to:", options: ["Only Bluetooth", "Only wired networks", "WiFi networks", "Satellite networks"], correctAnswer: 2, marks: 4 },
          { id: 7, question: "What does MQTT stand for?", options: ["Message Queue Telemetry Transport", "Multiple Query Transfer Tool", "Mobile Quick Text Transfer", "Micro Quality Test Tool"], correctAnswer: 0, marks: 4 },
          { id: 8, question: "A smart home system is an example of:", options: ["Traditional computing", "IOT application", "Offline system", "Manual control"], correctAnswer: 1, marks: 4 },
          { id: 9, question: "Which component allows remote control of devices via smartphone?", options: ["Simple switch", "WiFi/Bluetooth module", "Battery", "Resistor"], correctAnswer: 1, marks: 4 },
          { id: 10, question: "Cloud computing in IOT helps to:", options: ["Store and process data remotely", "Decrease speed", "Remove internet need", "Stop automation"], correctAnswer: 0, marks: 4 }
        ]
      },
      {
        id: "section-b",
        title: "Section B: Fill in the Blanks",
        type: "fillblank",
        totalQuestions: 5,
        marks: 20,
        questions: [
          { id: 11, question: "IOT stands for Internet of _____.", answer: "Things", acceptedAnswers: ["Things", "things", "THINGS"], marks: 4 },
          { id: 12, question: "_____ is a popular IOT protocol for lightweight messaging.", answer: "MQTT", acceptedAnswers: ["MQTT", "mqtt"], marks: 4 },
          { id: 13, question: "The ESP8266 is a _____ module used for IOT projects.", answer: "WiFi", acceptedAnswers: ["WiFi", "wifi", "WIFI", "Wi-Fi"], marks: 4 },
          { id: 14, question: "HC-05 is a _____ communication module.", answer: "Bluetooth", acceptedAnswers: ["Bluetooth", "bluetooth", "BLUETOOTH"], marks: 4 },
          { id: 15, question: "Data from IOT devices is often stored in the _____.", answer: "cloud", acceptedAnswers: ["cloud", "Cloud", "CLOUD"], marks: 4 }
        ]
      }
    ]
  }
};

export default function MixedExamPage() {
  const router = useRouter();
  const [timeRemaining, setTimeRemaining] = useState(2700); // 45 minutes in seconds
  const [currentSection, setCurrentSection] = useState(0);
  const [answers, setAnswers] = useState<Record<number, Answer>>({});
  const [examStarted, setExamStarted] = useState(false);
  const [examSubmitted, setExamSubmitted] = useState(false);
  const [showResults, setShowResults] = useState(false);
  const [score, setScore] = useState<ScoreData | null>(null);
  const [loading, setLoading] = useState(true);
  const [studentLevel, setStudentLevel] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Fetch student's level from API
  useEffect(() => {
    const fetchStudentLevel = async () => {
      try {
        setLoading(true);
        const token = sessionStorage.getItem('jwt');
        
        if (!token) {
          router.push('/login');
          return;
        }

        const response = await fetch(`${API_BASE_URL}/api/student/experiments`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch student data');
        }

        const data = await response.json();
        
        if (!data.level || !data.level.levelName) {
          throw new Error('No level assigned. Please contact your teacher.');
        }

        setStudentLevel(data.level.levelName);
      } catch (err) {
        console.error('Error fetching student level:', err);
        setError(err instanceof Error ? err.message : 'Failed to load exam');
      } finally {
        setLoading(false);
      }
    };

    fetchStudentLevel();
  }, [router]);

  // Get exam data based on student's level
  const examData = studentLevel ? examDataByLevel[studentLevel] : null;

  // Timer countdown
  useEffect(() => {
    if (examStarted && !examSubmitted && timeRemaining > 0) {
      const timer = setInterval(() => {
        setTimeRemaining(prev => {
          if (prev <= 1) {
            handleSubmitExam();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
      return () => clearInterval(timer);
    }
  }, [examStarted, examSubmitted, timeRemaining]);

  // Format time
  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const handleStartExam = () => {
    setExamStarted(true);
  };

  const handleAnswerChange = (questionId: number, answer: string | number, sectionType: string): void => {
    setAnswers(prev => ({
      ...prev,
      [questionId]: { answer, type: sectionType }
    }));
  };

  const handleSubmitExam = () => {
    if (!examData) return;
    
    if (!window.confirm("Are you sure you want to submit the exam? You cannot change answers after submission.")) {
      return;
    }

    // Calculate score
    let totalScore = 0;
    let maxScore = 0;

    examData.sections.forEach(section => {
      section.questions.forEach(q => {
        maxScore += q.marks;
        const userAnswer = answers[q.id];

        if (section.type === 'mcq' || section.type === 'image') {
          const question = q as any;
          if (userAnswer && userAnswer.answer === question.correctAnswer) {
            totalScore += q.marks;
          }
        } else if (section.type === 'fillblank') {
          const question = q as any;
          if (userAnswer && question.acceptedAnswers.includes((userAnswer.answer as string).trim())) {
            totalScore += q.marks;
          }
        }
      });
    });

    const percentage = Math.round((totalScore / maxScore) * 100);
    setScore({ total: totalScore, max: maxScore, percentage });
    setExamSubmitted(true);
    
    // API call to save exam results
    // saveExamResults(examData.id, answers, percentage);
  };

  const handleViewResults = () => {
    setShowResults(true);
  };

  // Loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 animate-spin text-indigo-600 mx-auto mb-4" />
          <p className="text-gray-600">Loading your exam...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error || !examData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-xl shadow-lg p-8 text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-900 mb-2">Unable to Load Exam</h2>
          <p className="text-gray-600 mb-6">{error || `No exam available for your level (${studentLevel || 'unknown'}). Please contact your teacher.`}</p>
          <button
            onClick={() => router.push('/student/dashboard')}
            className="bg-indigo-600 text-white px-6 py-2 rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  // Calculate progress
  const totalQuestions = examData.sections.reduce((sum, section) => sum + section.questions.length, 0);
  const answeredQuestions = Object.keys(answers).length;
  const progressPercentage = Math.round((answeredQuestions / totalQuestions) * 100);

  // Pre-exam instructions screen
  if (!examStarted) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-2xl w-full bg-white rounded-2xl shadow-lg border border-gray-200 overflow-hidden">
          <div className="bg-gradient-to-r from-indigo-600 to-blue-600 p-8 text-white text-center">
            <div className="bg-white/20 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4">
              <BookOpen size={40} />
            </div>
            <h1 className="text-3xl font-bold mb-2">{examData.title}</h1>
            <p className="text-indigo-100">{examData.level}</p>
          </div>

          <div className="p-8">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Examination Instructions</h2>
            
            <div className="space-y-4 mb-6">
              <div className="flex items-start gap-3 p-4 bg-blue-50 rounded-lg border border-blue-200">
                <Clock size={20} className="text-blue-600 mt-0.5 flex-shrink-0" />
                <div>
                  <p className="font-semibold text-gray-900">Duration</p>
                  <p className="text-sm text-gray-600">{examData.duration} minutes ({formatTime(timeRemaining)})</p>
                </div>
              </div>

              <div className="flex items-start gap-3 p-4 bg-green-50 rounded-lg border border-green-200">
                <CheckCircle size={20} className="text-green-600 mt-0.5 flex-shrink-0" />
                <div>
                  <p className="font-semibold text-gray-900">Total Questions</p>
                  <p className="text-sm text-gray-600">{examData.totalQuestions} questions (100 marks)</p>
                </div>
              </div>

              <div className="flex items-start gap-3 p-4 bg-purple-50 rounded-lg border border-purple-200">
                <Trophy size={20} className="text-purple-600 mt-0.5 flex-shrink-0" />
                <div>
                  <p className="font-semibold text-gray-900">Passing Score</p>
                  <p className="text-sm text-gray-600">{examData.passingScore}% or above</p>
                </div>
              </div>
            </div>

            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
              <h3 className="font-bold text-gray-900 mb-3 flex items-center gap-2">
                <AlertCircle size={20} className="text-yellow-600" />
                Important Guidelines
              </h3>
              <ul className="space-y-2 text-sm text-gray-700">
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>Read all questions carefully before answering</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>You can navigate between sections freely</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>Your progress is auto-saved every 30 seconds</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>Exam will auto-submit when time expires</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>Once submitted, answers cannot be changed</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-yellow-600">•</span>
                  <span>For fill-in-the-blank questions, spelling matters</span>
                </li>
              </ul>
            </div>

            <div className="bg-gray-100 rounded-lg p-4 mb-6">
              <h3 className="font-bold text-gray-900 mb-3">Exam Structure</h3>
              <div className="space-y-2 text-sm">
                {examData.sections.map((section, idx) => (
                  <div key={section.id} className="flex justify-between text-gray-700">
                    <span>{section.title}</span>
                    <span className="font-medium">{section.totalQuestions} Questions ({section.marks} marks)</span>
                  </div>
                ))}
              </div>
            </div>

            <button 
              onClick={handleStartExam}
              className="w-full bg-gradient-to-r from-indigo-600 to-blue-600 text-white font-bold py-4 px-6 rounded-lg hover:from-indigo-700 hover:to-blue-700 transition-all transform hover:scale-[1.02] flex items-center justify-center gap-2"
            >
              Start Examination
              <ChevronRight size={20} />
            </button>
          </div>
        </div>
      </div>
    );
  }

  // Results screen
  if (examSubmitted && showResults && score) {
    const isPassed = score.percentage >= examData.passingScore;
    
    return (
      <div className="min-h-screen bg-gray-50 p-4">
        <div className="max-w-4xl mx-auto">
          <div className="bg-white rounded-2xl shadow-lg border border-gray-200 overflow-hidden mb-6">
            <div className={`p-8 text-white text-center ${isPassed ? 'bg-gradient-to-r from-green-600 to-emerald-600' : 'bg-gradient-to-r from-red-600 to-orange-600'}`}>
              <div className="bg-white/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-4">
                {isPassed ? <Trophy size={48} /> : <XCircle size={48} />}
              </div>
              <h1 className="text-3xl font-bold mb-2">
                {isPassed ? 'Congratulations! 🎉' : 'Keep Practicing! 💪'}
              </h1>
              <p className="text-lg opacity-90">
                {isPassed ? 'You have passed the examination!' : 'You need more practice to pass'}
              </p>
            </div>

            <div className="p-8">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                <div className="bg-blue-50 rounded-xl p-6 text-center border border-blue-200">
                  <p className="text-sm text-gray-600 mb-2">Your Score</p>
                  <p className="text-4xl font-bold text-blue-600">{score.percentage}%</p>
                  <p className="text-sm text-gray-600 mt-2">{score.total} / {score.max} marks</p>
                </div>

                <div className="bg-purple-50 rounded-xl p-6 text-center border border-purple-200">
                  <p className="text-sm text-gray-600 mb-2">Questions Answered</p>
                  <p className="text-4xl font-bold text-purple-600">{answeredQuestions}</p>
                  <p className="text-sm text-gray-600 mt-2">out of {totalQuestions}</p>
                </div>

                <div className={`rounded-xl p-6 text-center border ${isPassed ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'}`}>
                  <p className="text-sm text-gray-600 mb-2">Result</p>
                  <p className={`text-4xl font-bold ${isPassed ? 'text-green-600' : 'text-red-600'}`}>
                    {isPassed ? 'PASS' : 'FAIL'}
                  </p>
                  <p className="text-sm text-gray-600 mt-2">Required: {examData.passingScore}%</p>
                </div>
              </div>

              <div className="bg-gray-50 rounded-xl p-6 mb-6">
                <h3 className="font-bold text-gray-900 mb-4">Section-wise Performance</h3>
                {examData.sections.map(section => {
                  const sectionQuestions = section.questions.length;
                  const sectionAnswered = section.questions.filter(q => answers[q.id]).length;
                  const sectionPercentage = Math.round((sectionAnswered / sectionQuestions) * 100);

                  return (
                    <div key={section.id} className="mb-4 last:mb-0">
                      <div className="flex justify-between items-center mb-2">
                        <span className="font-medium text-gray-700">{section.title}</span>
                        <span className="text-sm text-gray-600">{sectionAnswered}/{sectionQuestions} answered</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div 
                          className="bg-indigo-600 h-2 rounded-full transition-all"
                          style={{ width: `${sectionPercentage}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>

              {isPassed && (
                <div className="bg-gradient-to-r from-yellow-50 to-orange-50 border border-yellow-200 rounded-xl p-6 text-center mb-6">
                  <Trophy size={32} className="text-yellow-600 mx-auto mb-3" />
                  <h3 className="font-bold text-gray-900 mb-2">Certificate Available!</h3>
                  <p className="text-sm text-gray-600 mb-4">Download your certificate of completion</p>
                  <button className="px-6 py-3 bg-gradient-to-r from-yellow-500 to-orange-500 text-white rounded-lg hover:from-yellow-600 hover:to-orange-600 font-medium">
                    Download Certificate
                  </button>
                </div>
              )}

              <div className="flex gap-4">
                <button 
                  onClick={() => window.location.href = '/student/dashboard'}
                  className="flex-1 px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium"
                >
                  Back to Dashboard
                </button>
                {!isPassed && (
                  <button 
                    onClick={() => window.location.reload()}
                    className="flex-1 px-6 py-3 bg-white border-2 border-indigo-600 text-indigo-600 rounded-lg hover:bg-indigo-50 font-medium"
                  >
                    Retake Exam
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Submission confirmation screen
  if (examSubmitted && !showResults) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-2xl shadow-lg border border-gray-200 p-8 text-center">
          <div className="bg-green-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
            <CheckCircle size={40} className="text-green-600" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-3">Exam Submitted Successfully!</h2>
          <p className="text-gray-600 mb-6">Your answers have been recorded. Click below to view your results.</p>
          <button 
            onClick={handleViewResults}
            className="w-full bg-indigo-600 text-white font-bold py-3 px-6 rounded-lg hover:bg-indigo-700"
          >
            View Results
          </button>
        </div>
      </div>
    );
  }

  const currentSectionData = examData.sections[currentSection];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header with Timer */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
          <div>
            <h1 className="font-bold text-gray-900">{examData.title}</h1>
            <p className="text-xs text-gray-500">{examData.level}</p>
          </div>

          <div className="flex items-center gap-4">
            <div className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium ${
              timeRemaining < 300 ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'
            }`}>
              <Clock size={20} />
              <span>{formatTime(timeRemaining)}</span>
            </div>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto p-4 lg:p-6">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Sidebar - Section Navigation */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4 sticky top-24">
              <h3 className="font-bold text-gray-900 mb-4">Sections</h3>
              <div className="space-y-2 mb-6">
                {examData.sections.map((section, idx) => {
                  const sectionAnswered = section.questions.filter(q => answers[q.id]).length;
                  const sectionTotal = section.questions.length;
                  
                  return (
                    <button
                      key={section.id}
                      onClick={() => setCurrentSection(idx)}
                      className={`w-full text-left p-3 rounded-lg transition-colors ${
                        currentSection === idx
                          ? 'bg-indigo-50 border-2 border-indigo-600'
                          : 'bg-gray-50 border-2 border-transparent hover:bg-gray-100'
                      }`}
                    >
                      <div className="flex items-center justify-between mb-1">
                        <span className={`text-sm font-medium ${
                          currentSection === idx ? 'text-indigo-600' : 'text-gray-700'
                        }`}>
                          Section {String.fromCharCode(65 + idx)}
                        </span>
                        <span className="text-xs text-gray-500">
                          {sectionAnswered}/{sectionTotal}
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-1.5">
                        <div 
                          className="bg-indigo-600 h-1.5 rounded-full transition-all"
                          style={{ width: `${(sectionAnswered / sectionTotal) * 100}%` }}
                        />
                      </div>
                    </button>
                  );
                })}
              </div>

              <div className="border-t border-gray-200 pt-4">
                <div className="bg-gray-50 rounded-lg p-3 mb-4">
                  <div className="flex justify-between items-center mb-2">
                    <span className="text-sm text-gray-600">Progress</span>
                    <span className="text-sm font-bold text-gray-900">{progressPercentage}%</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-green-600 h-2 rounded-full transition-all"
                      style={{ width: `${progressPercentage}%` }}
                    />
                  </div>
                  <p className="text-xs text-gray-500 mt-2">
                    {answeredQuestions} of {totalQuestions} answered
                  </p>
                </div>

                <button 
                  onClick={handleSubmitExam}
                  className="w-full bg-gradient-to-r from-green-600 to-emerald-600 text-white font-bold py-3 px-4 rounded-lg hover:from-green-700 hover:to-emerald-700 transition-all flex items-center justify-center gap-2"
                >
                  <Send size={20} />
                  Submit Exam
                </button>
              </div>
            </div>
          </div>

          {/* Main Content - Questions */}
          <div className="lg:col-span-3">
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
              <div className="bg-gradient-to-r from-indigo-600 to-blue-600 p-6 text-white">
                <h2 className="text-xl font-bold mb-2">{currentSectionData.title}</h2>
                <p className="text-indigo-100 text-sm">
                  {currentSectionData.totalQuestions} Questions • {currentSectionData.marks} Marks
                </p>
              </div>

              <div className="p-6 space-y-8">
                {currentSectionData.questions.map((question, qIdx) => (
                  <div key={question.id} className="border-b border-gray-200 pb-6 last:border-b-0">
                    <div className="flex items-start gap-4">
                      <div className="bg-indigo-100 text-indigo-600 font-bold w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0">
                        {question.id}
                      </div>
                      <div className="flex-1">
                        <div className="flex justify-between items-start mb-3">
                          <h3 className="font-semibold text-gray-900 text-lg flex-1">
                            {question.question}
                          </h3>
                          <span className="text-sm font-medium text-indigo-600 ml-4">
                            {question.marks} marks
                          </span>
                        </div>

                        {/* Image for image-based questions */}
                        {currentSectionData.type === 'image' && (question as any).imageUrl && (
                          <div className="mb-4 bg-gray-100 rounded-lg p-4 border border-gray-200">
                            <div className="bg-white rounded-lg p-6 flex items-center justify-center min-h-[200px]">
                              <div className="text-center">
                                <ImageIcon size={48} className="text-gray-400 mx-auto mb-3" />
                                <p className="text-sm text-gray-500">{(question as any).imageAlt}</p>
                                <p className="text-xs text-gray-400 mt-1">Image will be displayed here</p>
                              </div>
                            </div>
                          </div>
                        )}

                        {/* MCQ and Image-based options */}
                        {(currentSectionData.type === 'mcq' || currentSectionData.type === 'image') && (
                          <div className="space-y-3">
                            {(question as any).options.map((option: string, optIdx: number) => (
                              <label
                                key={optIdx}
                                className={`flex items-center gap-3 p-4 rounded-lg border-2 cursor-pointer transition-all ${
                                  answers[question.id]?.answer === optIdx
                                    ? 'border-indigo-600 bg-indigo-50'
                                    : 'border-gray-200 bg-gray-50 hover:border-indigo-300 hover:bg-gray-100'
                                }`}
                              >
                                <input
                                  type="radio"
                                  name={`question-${question.id}`}
                                  value={optIdx}
                                  checked={answers[question.id]?.answer === optIdx}
                                  onChange={() => handleAnswerChange(question.id, optIdx, currentSectionData.type)}
                                  className="w-5 h-5 text-indigo-600 focus:ring-2 focus:ring-indigo-500"
                                />
                                <span className={`flex-1 font-medium ${
                                  answers[question.id]?.answer === optIdx
                                    ? 'text-indigo-700'
                                    : 'text-gray-700'
                                }`}>
                                  {String.fromCharCode(65 + optIdx)}. {option}
                                </span>
                              </label>
                            ))}
                          </div>
                        )}

                        {/* Fill in the blank input */}
                        {currentSectionData.type === 'fillblank' && (
                          <div>
                            <input
                              type="text"
                              value={(answers[question.id] as Answer | undefined)?.answer || ''}
                              onChange={(e) => handleAnswerChange(question.id, e.target.value, currentSectionData.type)}
                              placeholder="Type your answer here..."
                              className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent font-medium"
                            />
                            <p className="text-xs text-gray-500 mt-2">
                              Note: Spelling and capitalization matter
                            </p>
                          </div>
                        )}

                        {answers[question.id] && (
                          <div className="mt-3 flex items-center gap-2 text-sm text-green-600">
                            <CheckCircle size={16} />
                            <span>Answer saved</span>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {/* Section Navigation */}
              <div className="bg-gray-50 border-t border-gray-200 p-4 flex justify-between">
                <button
                  onClick={() => setCurrentSection(Math.max(0, currentSection - 1))}
                  disabled={currentSection === 0}
                  className={`flex items-center gap-2 px-6 py-2 rounded-lg font-medium transition-colors ${
                    currentSection === 0
                      ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
                      : 'bg-white border border-gray-300 text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <ArrowLeft size={20} />
                  Previous Section
                </button>

                <button
                  onClick={() => setCurrentSection(Math.min(examData.sections.length - 1, currentSection + 1))}
                  disabled={currentSection === examData.sections.length - 1}
                  className={`flex items-center gap-2 px-6 py-2 rounded-lg font-medium transition-colors ${
                    currentSection === examData.sections.length - 1
                      ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
                      : 'bg-indigo-600 text-white hover:bg-indigo-700'
                  }`}
                >
                  Next Section
                  <ChevronRight size={20} />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}