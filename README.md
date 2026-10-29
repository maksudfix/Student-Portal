# Student Portal 1.0

A full-stack, responsive Student Portal built with ASP.NET Core MVC, Entity Framework Core, SQL Server, HTML, CSS and JavaScript. The application provides authentication, role-based access control, student profile management, course management and enrollment, class and exam schedule management, and an administrative dashboard.

## Features

* **Authentication & Authorization**

  * ASP.NET Core Identity integration
  * Role-based access control with `Admin` and `Student` roles
  * Login and registration
  * Access-denied handling

* **Role-Based Access Control**

  * Administrative access for portal management
  * Protected routes and authorization policies
  * Separate student and admin functionality

* **Student Profile Management**

  * Create, view, and update student profiles
  * Personal and academic information management
  * Profile details available to the student and admin

* **Course Management**

  * Create, read, update, and delete courses
  * Course details and information management
  * Organize courses for easier browsing

* **Course Enrollment**

  * Students can browse available courses
  * Enroll in and drop courses
  * View enrolled courses
  * Admin enrollment overview

* **Class & Exam Schedules**

  * View class timetables
  * View upcoming exam dates and details
  * Admin can create and manage class and exam schedules

* **Admin Dashboard**

  * Centralized portal management dashboard
  * Student and course overview
  * Enrollment information
  * Class and exam schedule management

* **ViewModel-Based Architecture**

  * Dedicated ViewModels for forms and UI operations
  * Separates user input from application entities
  * Supports cleaner model binding and validation

* **Responsive UI**

  * Razor Views with Bootstrap
  * Custom CSS and JavaScript
  * Responsive layouts for different screen sizes

### Architecture Overview

**Controllers**
Handle HTTP requests, application flow, authorization, and communication between Views, ViewModels, Identity, and the database.

**ViewModels**
Provide dedicated models for operations such as student registration, login, profile editing, course creation/editing, enrollment, and schedule management.

**Entity Framework Core**
Provides database access through `AppDbContext` and manages the application's data model and migrations.

**ASP.NET Core Identity**
Handles authentication, authorization, roles, password management, and account-related functionality.

**Razor Views**
Provide the presentation layer using Razor, HTML, CSS, JavaScript, Bootstrap, and custom UI components.


## Prerequisites

Make sure the following are installed:

* [.NET SDK](https://dotnet.microsoft.com/download) (match the target framework of the project)
* SQL Server, SQL Server Express, or SQL Server LocalDB
* Visual Studio or VS Code with the C# Dev Kit
* Entity Framework Core CLI (`dotnet-ef`)

> **Target Framework:** `.NET 10` *(update if different)*
> **Install From Manage NuGet Packages:**
  Microsoft.AspNetCore.Identity.EntityFrameworkCore Version="10.0.0"
  Microsoft.EntityFrameworkCore.SqlServer Version="10.0.0"
  Microsoft.EntityFrameworkCore.Tools Version="10.0.0"

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/maksudfix/Student-Portal.git
cd Student-Portal
```

### 2. Configure the Database

Update the connection string in `appsettings.json` according to your SQL Server environment.

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS; Database=student_portal_db; Trusted_Connection=True; TrustServerCertificate=True;"
  }
}
```

### 3. Apply Entity Framework Migrations

To create a new migration:

```powershell
Add-Migration Initial
```

To apply the migration:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

## Project Goals

Student Portal was built to demonstrate practical experience with:

* ASP.NET Core MVC application development
* C# and object-oriented programming
* Entity Framework Core
* SQL Server database integration
* ASP.NET Core Identity
* Authentication and authorization
* Role-based access control
* CRUD operations
* Student profile management
* Course and enrollment management
* Class and exam schedule management
* ViewModels and model binding
* Razor Views
* Database migrations
* Responsive web development

## Future Improvements

* **Grades & Transcripts**
  * Allow admins to record grades
  * Let students view results and transcripts

* **Notifications & Messaging**
  * Exam and class reminders
  * Messaging between students and administrators

* **Architecture & Clean Code**
  * Refactor controllers to follow SOLID principles
  * Add centralized exception handling
  * Implement structured logging
  * Improve service-layer architecture

* **Performance & Scale**
  * Optimize database queries
  * Add pagination for large datasets
  * Improve search performance
    
## Author
Maksud Mubin (Trex Development)
https://github.com/maksudfix


## Author

Maksud Mubin (Trex Development)

GitHub: https://github.com/maksudfix
