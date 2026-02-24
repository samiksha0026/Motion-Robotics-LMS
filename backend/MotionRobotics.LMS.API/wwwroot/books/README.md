# Digital Books Directory

## Overview

This directory contains PDF files for digital books that students can access based on their assigned robotics levels.

## File Naming Convention

Place your PDF files with these exact names to match the level-based book system:

### Required Files:

- `mech-tech.pdf` - For Mech-Tech level students
- `digi-coding.pdf` - For Digi-Coding level students
- `basic-electronics.pdf` - For Basic Electronics level students
- `electro-mechanical.pdf` - For Electro-Mechanical level students
- `digi-sense.pdf` - For Digi-Sense level students
- `wireless-iot.pdf` - For Wireless & IoT level students

## How It Works

1. When a student opens the digital books page (`/student/books`), the system:
   - Fetches the student's assigned robotics level
   - Maps the level name to the appropriate book
   - Displays the corresponding PDF for online viewing

2. The books are served through the API endpoint: `GET /api/books/{fileName}`

## Book Content Guidelines

- **File Format**: PDF only
- **File Size**: Recommended maximum 20MB for optimal loading
- **Security**: Files are served with security validations (no path traversal, PDF extension only)
- **Access**: Students can only view books online, no download option provided

## Adding New Books

1. Place the PDF file in this directory with the correct naming convention
2. The book will be automatically available to students assigned to the matching robotics level
3. Book metadata (title, author, description) is managed in the database via the Book seeder

## Troubleshooting

- **Book not loading**: Check if the PDF file exists with the exact name expected
- **Wrong book showing**: Verify the student's robotics level assignment matches the expected level name
- **PDF errors**: Ensure the PDF file is not corrupted and is a valid PDF document

## Current Issue Fixed

Previously, the system was hardcoded to always show the "Digi-Coding" book regardless of the student's level. This has been fixed to dynamically show the correct book based on the student's assigned robotics level.
