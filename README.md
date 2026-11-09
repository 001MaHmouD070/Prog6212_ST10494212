# RaceDay Event Management System

![Build and Test](https://github.com/001MaHmouD070/Prog6212_ST10494212/actions/workflows/dotnet-tests.yml/badge.svg)

## Part 3 - MVC Web Application

This is the final part of the PROG6212 Portfolio of Evidence. Part 3 introduces a fully functional ASP.NET Core MVC frontend that consumes the Part 2 REST API.

### Features
- **Role-Based UI:** Dedicated dashboards for Organisers and Participants.
- **Session Management:** Custom `CookieHandler` bridges the API's server-side session to the MVC browser cookies.
- **Azure Blob Storage:** Organisers can upload event banners (implemented via a simulated local Azure Blob service).
- **Docker Support:** Fully containerized using Docker and Docker Compose.

## Docker Instructions

To run the entire application using Docker, ensure you have Docker Desktop installed and run the following command in the root directory:

```bash
docker-compose up --build

# Prog6212_ST10494212
System planning and Database
# RaceDay - Event Management System

## Overview
RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. This is Part 1 of the Portfolio of Evidence for PROG6212 - Programming 2B.

## System Roles
The system supports two distinct user roles:

### 1. Organiser
- Create, edit, and delete events
- Manage event categories
- Capture participant results
- View all event enrolments

### 2. Participant
- Create an account
- Browse upcoming events
- Enter events by selecting a category
- View their own enrolments
- Track their personal results

## Project Structure

