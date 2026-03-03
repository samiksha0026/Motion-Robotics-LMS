"use client";

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { 
  BookOpen, 
  Users, 
  Award, 
  TrendingUp, 
  CheckCircle, 
  ArrowRight,
  Star,
  Menu,
  X,
  ChevronRight,
  GraduationCap,
  UserCircle,
  Shield,
  Zap,
  Sparkles,
  Globe,
  Cpu,
  CircuitBoard,
  Cog,
  Code,
  Wifi,
  Phone,
  Mail,
  MapPin,
  Bot
} from 'lucide-react';
import { ThemeToggle } from '@/components/ThemeToggle';

export default function LMSHomepage() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 50);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const features = [
    {
      icon: Cog,
      title: 'Hands-on Experiment Kits',
      description: '216+ real hardware experiments — students build robots, circuits, and IoT devices with physical kits provided to every school.',
      color: 'from-orange-500 to-red-500'
    },
    {
      icon: Code,
      title: 'Arduino & Coding Labs',
      description: 'Step-by-step Arduino programming integrated into the curriculum. Students write real code to control motors, sensors, and LEDs.',
      color: 'from-green-500 to-emerald-500'
    },
    {
      icon: Award,
      title: 'Level-wise Certificates',
      description: 'Auto-generated certificates upon completing each level. Students build a portfolio as they progress through all 6 stages.',
      color: 'from-purple-500 to-purple-600'
    },
    {
      icon: TrendingUp,
      title: 'Teacher Dashboard & Tracking',
      description: 'Teachers monitor every student\'s experiment progress and exam scores in real-time through the LMS.',
      color: 'from-blue-500 to-cyan-500'
    },
    {
      icon: BookOpen,
      title: 'Structured 6-Level Curriculum',
      description: 'From mechanical basics to wireless IoT — a progressive learning path designed for grades 1-12 with age-appropriate content.',
      color: 'from-indigo-500 to-indigo-600'
    },
    {
      icon: CircuitBoard,
      title: 'Exam & Assessment System',
      description: 'Built-in exams for each level with auto-evaluation. Teachers can track scores and identify students who need extra help.',
      color: 'from-teal-500 to-teal-600'
    }
  ];



  // Actual Motion Robotics Course Levels
  const roboticsLevels = [
    {
      level: 1,
      title: 'Mech Tech',
      subtitle: 'Mechanical Foundations',
      description: 'Introduction to mechanical systems, gears, levers, and basic robot assembly.',
      experiments: 41,
      icon: Cog,
      color: 'from-orange-500 to-red-500',
      bgColor: 'from-orange-50 to-red-50'
    },
    {
      level: 2,
      title: 'Electronics',
      subtitle: 'Circuit Fundamentals',
      description: 'Learn electronics basics, circuit design, sensors, and component integration.',
      experiments: 65,
      icon: CircuitBoard,
      color: 'from-blue-500 to-cyan-500',
      bgColor: 'from-blue-50 to-cyan-50'
    },
    {
      level: 3,
      title: 'Electro Mechanical',
      subtitle: 'Integration & Control',
      description: 'Combine mechanical and electronic systems for automated solutions.',
      experiments: 14,
      icon: Zap,
      color: 'from-purple-500 to-pink-500',
      bgColor: 'from-purple-50 to-pink-50'
    },
    {
      level: 4,
      title: 'Digi-Tech Coding',
      subtitle: 'Programming Basics',
      description: 'Introduction to programming, Arduino coding, and robot control systems.',
      experiments: 54,
      icon: Code,
      color: 'from-green-500 to-emerald-500',
      bgColor: 'from-green-50 to-emerald-50'
    },
    {
      level: 5,
      title: 'Digi Sense',
      subtitle: 'Sensors & Intelligence',
      description: 'Advanced sensor integration, data processing, and intelligent systems.',
      experiments: 26,
      icon: Cpu,
      color: 'from-indigo-500 to-violet-500',
      bgColor: 'from-indigo-50 to-violet-50'
    },
    {
      level: 6,
      title: 'Wireless IoT',
      subtitle: 'Connected Systems',
      description: 'IoT protocols, wireless communication, and cloud-connected robotics.',
      experiments: 16,
      icon: Wifi,
      color: 'from-teal-500 to-cyan-500',
      bgColor: 'from-teal-50 to-cyan-50'
    }
  ];



  return (
    <div className="min-h-screen transition-colors duration-300" style={{ backgroundColor: 'var(--background)' }}>
      {/* Top Info Bar */}
      <div className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-2 px-4 text-center text-sm hidden md:block">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div className="flex items-center gap-6">
            <a href="mailto:info@motionrobotics.in" className="flex items-center gap-2 hover:text-blue-200 transition-colors">
              <Mail className="w-4 h-4" />
              info@motionrobotics.in
            </a>
            <a href="tel:+919730480960" className="flex items-center gap-2 hover:text-blue-200 transition-colors">
              <Phone className="w-4 h-4" />
              +91 973-048-0960
            </a>
          </div>
          <div className="flex items-center gap-2">
            <BookOpen className="w-4 h-4" />
            <span className="font-medium">Learning Management System for Schools</span>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className={`fixed w-full z-50 transition-all duration-300 ${
        scrolled ? 'top-0 shadow-lg' : 'top-0 md:top-8 backdrop-blur-sm'
      }`} style={{ backgroundColor: scrolled ? 'var(--surface)' : 'rgba(var(--surface-rgb, 255, 255, 255), 0.95)' }}>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-24">
            {/* Logo */}
            <Link href="/" className="flex items-center gap-3 group">
              <Image 
                src="/images/logo.png" 
                alt="Motion Robotics" 
                width={280}
                height={72}
                className="h-16 w-auto object-contain"
                priority
              />
              <span className="hidden sm:inline-flex px-2.5 py-1 bg-gradient-to-r from-blue-600 to-indigo-600 text-white text-xs font-bold rounded-md tracking-wider uppercase shadow-md">LMS</span>
            </Link>

            {/* Desktop Navigation */}
            <div className="hidden lg:flex items-center gap-6">
              <Link href="#features" className="font-medium transition-colors relative group" style={{ color: 'var(--text-secondary)' }}>
                Features
                <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-blue-600 group-hover:w-full transition-all"></span>
              </Link>
              <Link href="#courses" className="font-medium transition-colors relative group" style={{ color: 'var(--text-secondary)' }}>
                Programs
                <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-blue-600 group-hover:w-full transition-all"></span>
              </Link>
              <Link href="#testimonials" className="font-medium transition-colors relative group" style={{ color: 'var(--text-secondary)' }}>
                Testimonials
                <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-blue-600 group-hover:w-full transition-all"></span>
              </Link>
              <Link href="#contact" className="font-medium transition-colors relative group" style={{ color: 'var(--text-secondary)' }}>
                Contact
                <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-blue-600 group-hover:w-full transition-all"></span>
              </Link>
            </div>

            {/* Login Button - Desktop */}
            <div className="hidden lg:flex items-center gap-3">
              <ThemeToggle />
              <Link 
                href="/login" 
                className="flex items-center gap-2 px-6 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 text-white hover:from-blue-700 hover:to-indigo-700 rounded-xl transition-all font-semibold shadow-lg shadow-blue-500/25 hover:shadow-blue-500/40"
              >
                Sign In
                <ArrowRight className="w-4 h-4" />
              </Link>
            </div>

            {/* Mobile Menu Button */}
            <button 
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="lg:hidden p-2 rounded-lg transition-colors"
              style={{ color: 'var(--text-primary)' }}
            >
              {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>

            {/* Theme Toggle - Mobile (always visible) */}
            <div className="lg:hidden">
              <ThemeToggle />
            </div>
          </div>

          {/* Mobile Menu */}
          {mobileMenuOpen && (
            <div className="lg:hidden py-4 border-t" style={{ backgroundColor: 'var(--surface)', borderColor: 'var(--border)' }}>
              <div className="flex flex-col gap-4">
                <Link href="#features" className="font-medium px-2 transition-colors" style={{ color: 'var(--text-secondary)' }}>
                  Features
                </Link>
                <Link href="#courses" className="font-medium px-2 transition-colors" style={{ color: 'var(--text-secondary)' }}>
                  Programs
                </Link>
                <Link href="#testimonials" className="font-medium px-2 transition-colors" style={{ color: 'var(--text-secondary)' }}>
                  Testimonials
                </Link>
                <Link href="#contact" className="font-medium px-2 transition-colors" style={{ color: 'var(--text-secondary)' }}>
                  Contact
                </Link>
                <div className="border-t pt-4 mt-2">
                  <Link 
                    href="/login" 
                    className="flex items-center justify-center gap-2 px-4 py-3.5 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-semibold shadow-lg shadow-blue-500/25"
                  >
                    Sign In to Your Account
                    <ArrowRight className="w-5 h-5" />
                  </Link>
                </div>
              </div>
            </div>
          )}
        </div>
      </nav>

      {/* Hero Section */}
      <section className="pt-36 md:pt-44 pb-16 px-4 sm:px-6 lg:px-8 bg-gradient-to-br from-slate-50 via-blue-50/50 to-indigo-50/30 relative overflow-hidden">
        <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-gradient-to-bl from-blue-100/50 to-transparent rounded-full -translate-y-1/3 translate-x-1/4"></div>
        <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-gradient-to-tr from-indigo-100/40 to-transparent rounded-full translate-y-1/3 -translate-x-1/4"></div>
        
        <div className="max-w-7xl mx-auto relative z-10">
          <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
            {/* Left — Text Content */}
            <div className="animate-fade-in-left">
              <div className="inline-flex items-center gap-2 px-4 py-2 bg-white text-blue-700 rounded-full text-sm font-semibold mb-6 border border-blue-100 shadow-sm">
                <Bot className="w-4 h-4" />
                Robotics Education for Schools
              </div>
              
              <h1 className="text-4xl md:text-5xl lg:text-[3.5rem] font-bold text-gray-900 leading-[1.15] mb-6">
                Hands-on Robotics
                <span className="block bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                  Learning for Students
                </span>
              </h1>
              
              <p className="text-lg text-gray-600 leading-relaxed mb-8 max-w-lg">
                A structured 6-level curriculum — from mechanical systems to IoT — with 216+ real experiments. Trusted by 50+ schools across India.
              </p>
              
              <div className="flex flex-wrap gap-3 mb-10">
                <Link 
                  href="/login"
                  className="px-7 py-3.5 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-semibold hover:shadow-xl hover:shadow-blue-200 transform hover:-translate-y-1 transition-all flex items-center gap-2 group"
                >
                  Start Learning
                  <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                </Link>
                <Link 
                  href="#courses"
                  className="px-7 py-3.5 bg-white text-gray-700 rounded-xl font-semibold hover:bg-gray-50 transition-all flex items-center gap-2 border border-gray-200 shadow-sm hover:shadow-md"
                >
                  View Programs
                  <ChevronRight className="w-5 h-5" />
                </Link>
              </div>

              {/* Compact stats */}
              <div className="flex flex-wrap gap-8">
                {[
                  { value: '50+', label: 'Partner Schools' },
                  { value: '10K+', label: 'Students' },
                  { value: '6', label: 'Learning Levels' },
                ].map((s, i) => (
                  <div key={i} className="text-center">
                    <div className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">{s.value}</div>
                    <div className="text-sm text-gray-500 font-medium">{s.label}</div>
                  </div>
                ))}
              </div>
            </div>

            {/* Right — Photo Grid */}
            <div className="relative animate-fade-in-right">
              <div className="relative">
                {/* Main large photo */}
                <div className="relative rounded-2xl overflow-hidden shadow-2xl aspect-[16/10] animate-pulse-glow">
                  <Image 
                    src="/images/students-robots.jpg" 
                    alt="Students building robots at Cambridge International School" 
                    fill
                    className="object-cover"
                    priority
                  />
                </div>
                
                {/* Floating second photo */}
                <div className="absolute -bottom-6 -left-6 w-48 md:w-56 rounded-2xl overflow-hidden shadow-2xl border-4 border-white animate-float">
                  <div className="aspect-[4/3] relative">
                    <Image 
                      src="/images/students-robotics.jpg" 
                      alt="Students wiring drone circuits" 
                      fill
                      className="object-cover"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-24 px-4 sm:px-6 lg:px-8 bg-white relative">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <div className="inline-flex items-center gap-2 px-4 py-2 bg-blue-50 text-blue-700 rounded-full text-sm font-semibold mb-4">
              <Sparkles className="w-4 h-4" />
              Platform Features
            </div>
            <h2 className="text-4xl lg:text-5xl font-bold mb-6 text-gray-900">
              Everything You Need to
              <span className="block mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                Excel in Robotics
              </span>
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              A comprehensive platform designed specifically for robotics education in schools
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {features.map((feature, index) => {
              const Icon = feature.icon;
              return (
                <div 
                  key={index}
                  className="group p-8 bg-gray-50 rounded-2xl hover:bg-white hover:shadow-2xl transition-all duration-500 border border-transparent hover:border-gray-100"
                >
                  <div className={`w-14 h-14 bg-gradient-to-br ${feature.color} rounded-2xl flex items-center justify-center mb-6 group-hover:scale-110 group-hover:rotate-3 transition-all duration-300`}>
                    <Icon className="w-7 h-7 text-white" />
                  </div>
                  <h3 className="text-xl font-bold mb-3 text-gray-900 group-hover:text-blue-600 transition-colors">{feature.title}</h3>
                  <p className="text-gray-600 leading-relaxed">{feature.description}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Robotics Levels / Programs Section */}
      <section id="courses" className="py-24 px-4 sm:px-6 lg:px-8 bg-gradient-to-b from-gray-50 to-white">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl lg:text-5xl font-bold mb-6 text-gray-900">
              6-Level Learning
              <span className="block mt-2 bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                Curriculum
              </span>
            </h2>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Progressive robotics education from basic mechanics to advanced IoT systems
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {roboticsLevels.map((course) => {
              const Icon = course.icon;
              return (
                <div key={course.level} className="bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-2xl transition-all duration-500 border border-gray-100 group">
                  <div className={`h-28 bg-gradient-to-br ${course.color} flex items-center justify-between px-6 relative overflow-hidden`}>
                    <div>
                      <div className="text-white/80 text-sm font-medium">Level {course.level}</div>
                      <div className="text-white text-xl font-bold">{course.title}</div>
                    </div>
                    <Icon className="w-14 h-14 text-white/30 group-hover:text-white/50 group-hover:scale-110 transition-all duration-300" />
                  </div>
                  <div className="p-6">
                    <p className="text-blue-600 font-medium text-sm mb-2">{course.subtitle}</p>
                    <p className="text-gray-600 mb-4 text-sm leading-relaxed">{course.description}</p>
                    <div className="flex items-center justify-between pt-4 border-t border-gray-100">
                      <div className="flex items-center gap-2 text-sm text-gray-500">
                        <BookOpen className="w-4 h-4" />
                        <span className="font-medium">{course.experiments} Experiments</span>
                      </div>
                      <Link 
                        href="/login"
                        className="flex items-center gap-1 text-sm font-semibold text-blue-600 hover:text-indigo-600 hover:gap-2 transition-all"
                      >
                        Start
                        <ArrowRight className="w-4 h-4" />
                      </Link>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Photo Gallery + Social Proof Section */}
      <section id="testimonials" className="py-24 px-4 sm:px-6 lg:px-8 bg-gray-900 relative overflow-hidden">
        <div className="absolute inset-0 opacity-5" style={{ backgroundImage: 'radial-gradient(circle at 2px 2px, white 1px, transparent 0)', backgroundSize: '40px 40px' }}></div>
        
        <div className="max-w-7xl mx-auto relative z-10">
          <div className="grid lg:grid-cols-2 gap-16 items-center">
            {/* Left — Photos */}
            <div className="relative">
              <div className="relative rounded-2xl overflow-hidden shadow-2xl aspect-[4/3]">
                <Image 
                  src="/images/students-robotics.jpg" 
                  alt="Students working on drone circuits" 
                  fill
                  className="object-cover"
                />
              </div>
              <div className="absolute -bottom-8 -right-8 w-48 md:w-64 rounded-2xl overflow-hidden shadow-2xl border-4 border-gray-900">
                <div className="aspect-[4/3] relative">
                  <Image 
                    src="/images/students-robots.jpg" 
                    alt="Students building robot arms" 
                    fill
                    className="object-cover"
                  />
                </div>
              </div>
            </div>
            
            {/* Right — Content */}
            <div>
              <div className="inline-flex items-center gap-2 px-4 py-2 bg-white text-blue-600 rounded-full text-sm font-bold mb-6 shadow-md">
                <GraduationCap className="w-4 h-4" />
                Trusted by Educators
              </div>
              
              <h2 className="text-4xl lg:text-5xl font-bold mb-8 leading-tight">
                <div className="bg-black/70 backdrop-blur-sm rounded-2xl p-6 inline-block">
                  <span className="text-white font-black">Real Classrooms.</span>
                  <span className="block text-yellow-300 font-black">Real Results.</span>
                </div>
              </h2>
              
              {/* Real testimonial */}
              <div className="bg-white rounded-2xl p-8 border border-gray-200 mb-8 shadow-2xl">
                <div className="flex gap-1 mb-4">
                  {[1,2,3,4,5].map((i) => (
                    <Star key={i} className="w-5 h-5 fill-yellow-400 text-yellow-400" />
                  ))}
                </div>
                <p className="text-gray-900 text-lg leading-relaxed mb-6 font-semibold">
                  &quot;The students are thoroughly enjoying the interactive modules. This program has brought a new dimension to our teaching approach.&quot;
                </p>
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-indigo-500 rounded-full flex items-center justify-center text-white font-bold text-lg shadow-lg">
                    M
                  </div>
                  <div>
                    <div className="font-bold text-gray-900 text-base">Manoj A. Savale</div>
                    <div className="text-sm text-gray-600 font-medium">Principal, Bhondawe Patil Public School</div>
                  </div>
                </div>
              </div>
              
              {/* Key points */}
              <div className="space-y-3">
                {[
                  'Physical robot kits provided to every partner school',
                  'Trained instructors guide students through experiments',
                  'LMS tracks progress and exam results',
                ].map((text, i) => (
                  <div key={i} className="flex items-center gap-3 bg-white rounded-xl p-4 shadow-lg border border-gray-200">
                    <CheckCircle className="w-6 h-6 text-emerald-500 flex-shrink-0" />
                    <span className="text-gray-900 font-semibold text-base">{text}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-r from-blue-600 to-indigo-600 relative overflow-hidden">
        <div className="absolute inset-0">
          <div className="absolute top-0 right-0 w-80 h-80 bg-white/10 rounded-full blur-3xl"></div>
          <div className="absolute bottom-0 left-0 w-80 h-80 bg-white/10 rounded-full blur-3xl"></div>
        </div>
        
        <div className="max-w-4xl mx-auto text-center relative z-10">
          <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-white mb-6">
            Ready to Get Started?
          </h2>
          <p className="text-lg text-blue-100 mb-8 max-w-2xl mx-auto">
            Join thousands of students across India learning robotics through our hands-on curriculum.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link 
              href="/login"
              className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-white text-blue-600 rounded-xl font-bold hover:shadow-2xl hover:-translate-y-1 transition-all"
            >
              Sign In to Your Account
              <ArrowRight className="w-5 h-5" />
            </Link>
            <Link 
              href="#contact"
              className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-transparent text-white border-2 border-white/40 rounded-xl font-bold hover:bg-white/10 transition-all"
            >
              <Mail className="w-5 h-5" />
              Contact Sales
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer id="contact" className="bg-gray-100 text-gray-800 border-t border-gray-200">
        {/* Main Footer Content */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-14">
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-10">
            {/* Brand Column */}
            <div className="lg:col-span-1">
              <div className="flex items-center gap-3 mb-5">
                <div className="w-11 h-11 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
                  <Bot className="w-6 h-6 text-white" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-gray-900">Motion Robotics</h3>
                  <p className="text-[11px] text-blue-600 font-semibold tracking-wider uppercase">Learning Management System</p>
                </div>
              </div>
              <p className="text-gray-600 leading-relaxed mb-5 text-sm">
                <span className="text-blue-600 font-semibold">Opportunity for Innovation</span> — Empowering the next generation through innovative robotics education across India.
              </p>
              <div className="flex gap-2">
                <a href="https://motionrobotics.in" target="_blank" className="w-9 h-9 bg-white border border-gray-200 hover:bg-blue-600 hover:border-blue-600 rounded-lg flex items-center justify-center transition-colors text-gray-500 hover:text-white shadow-sm">
                  <Globe className="w-4 h-4" />
                </a>
                <a href="mailto:info@motionrobotics.in" className="w-9 h-9 bg-white border border-gray-200 hover:bg-blue-600 hover:border-blue-600 rounded-lg flex items-center justify-center transition-colors text-gray-500 hover:text-white shadow-sm">
                  <Mail className="w-4 h-4" />
                </a>
                <a href="tel:+919730480960" className="w-9 h-9 bg-white border border-gray-200 hover:bg-blue-600 hover:border-blue-600 rounded-lg flex items-center justify-center transition-colors text-gray-500 hover:text-white shadow-sm">
                  <Phone className="w-4 h-4" />
                </a>
              </div>
            </div>

            {/* Quick Links */}
            <div>
              <h4 className="font-bold text-sm uppercase tracking-wider text-gray-900 mb-5 flex items-center gap-2">
                <span className="w-1 h-5 bg-blue-500 rounded-full"></span>
                Quick Links
              </h4>
              <ul className="space-y-2.5">
                <li>
                  <Link href="#features" className="text-gray-600 hover:text-blue-600 transition-colors flex items-center gap-2 text-sm">
                    <ChevronRight className="w-4 h-4 text-blue-500" />Features
                  </Link>
                </li>
                <li>
                  <Link href="#courses" className="text-gray-600 hover:text-blue-600 transition-colors flex items-center gap-2 text-sm">
                    <ChevronRight className="w-4 h-4 text-blue-500" />Programs
                  </Link>
                </li>
                <li>
                  <Link href="#testimonials" className="text-gray-600 hover:text-blue-600 transition-colors flex items-center gap-2 text-sm">
                    <ChevronRight className="w-4 h-4 text-blue-500" />Testimonials
                  </Link>
                </li>
                <li>
                  <Link href="#contact" className="text-gray-600 hover:text-blue-600 transition-colors flex items-center gap-2 text-sm">
                    <ChevronRight className="w-4 h-4 text-blue-500" />Contact
                  </Link>
                </li>
              </ul>
            </div>

            {/* Login */}
            <div>
              <h4 className="font-bold text-sm uppercase tracking-wider text-gray-900 mb-5 flex items-center gap-2">
                <span className="w-1 h-5 bg-indigo-500 rounded-full"></span>
                Account Access
              </h4>
              <div className="space-y-3">
                <Link 
                  href="/login" 
                  className="flex items-center justify-center gap-2 px-4 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-semibold text-sm shadow-lg shadow-blue-500/25 hover:shadow-blue-500/40 transition-all"
                >
                  Sign In
                  <ArrowRight className="w-4 h-4" />
                </Link>
                <p className="text-xs text-gray-500 text-center">
                  For Students, Teachers & Admins
                </p>
              </div>
            </div>

            {/* Contact Info */}
            <div>
              <h4 className="font-bold text-sm uppercase tracking-wider text-gray-900 mb-5 flex items-center gap-2">
                <span className="w-1 h-5 bg-purple-500 rounded-full"></span>
                Contact Us
              </h4>
              <ul className="space-y-3">
                <li className="flex items-start gap-3">
                  <div className="w-9 h-9 bg-white border border-gray-200 rounded-lg flex items-center justify-center flex-shrink-0 shadow-sm">
                    <Mail className="w-4 h-4 text-blue-500" />
                  </div>
                  <div>
                    <div className="text-gray-500 text-xs">Email</div>
                    <a href="mailto:info@motionrobotics.in" className="text-gray-900 hover:text-blue-600 transition-colors font-medium text-sm">
                      info@motionrobotics.in
                    </a>
                  </div>
                </li>
                <li className="flex items-start gap-3">
                  <div className="w-9 h-9 bg-white border border-gray-200 rounded-lg flex items-center justify-center flex-shrink-0 shadow-sm">
                    <Phone className="w-4 h-4 text-blue-500" />
                  </div>
                  <div>
                    <div className="text-gray-500 text-xs">Phone</div>
                    <a href="tel:+919730480960" className="text-gray-900 hover:text-blue-600 transition-colors font-medium text-sm">
                      +91 973-048-0960
                    </a>
                  </div>
                </li>
                <li className="flex items-start gap-3">
                  <div className="w-9 h-9 bg-white border border-gray-200 rounded-lg flex items-center justify-center flex-shrink-0 shadow-sm">
                    <MapPin className="w-4 h-4 text-blue-500" />
                  </div>
                  <div>
                    <div className="text-gray-500 text-xs">Location</div>
                    <span className="text-gray-900 font-medium text-sm">Pune, Maharashtra, India</span>
                  </div>
                </li>
              </ul>
            </div>
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="border-t border-gray-200 bg-white">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-5">
            <div className="flex flex-col md:flex-row justify-between items-center gap-3">
              <p className="text-gray-500 text-sm">
                © 2026 Motion Robotics. All rights reserved.
              </p>
              <div className="flex items-center gap-2 text-sm">
                <span className="text-gray-500">Powered by</span>
                <span className="text-blue-600 font-bold">Motion Robotics LMS</span>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
