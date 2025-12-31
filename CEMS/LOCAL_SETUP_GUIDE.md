# Running Community Event Management System Locally

Complete guide to run this application on your Windows PC.

---

## Prerequisites

### 1. Install .NET 8.0 SDK
Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

After installation, verify by opening Command Prompt:
```bash
dotnet --version
```
Should show: `8.0.x`

### 2. Install PostgreSQL
Download and install from: https://www.postgresql.org/download/windows/

During installation:
- **Remember the password** you set for the `postgres` user
- Default port: `5432`
- Default username: `postgres`

---

## Setup Steps

### Step 1: Extract the Project

Extract the project zip file to a folder on your PC (e.g., `C:\Projects\CommunityEvents`)

### Step 2: Create the Database

1. Open **pgAdmin** (installed with PostgreSQL) or **Command Prompt**
2. Create a new database called `CommunityEvents`:

**Using pgAdmin:**
- Right-click "Databases" -> "Create" -> "Database"
- Name: `CommunityEvents`
- Click "Save"

**Using Command Prompt:**
```bash
"C:\Program Files\PostgreSQL\16\bin\psql.exe" -U postgres
```
Then enter:
```sql
CREATE DATABASE CommunityEvents;
\q
```

### Step 3: Configure Connection String

Open `Program.cs` in the project folder and find these lines (around line 28-32):

```csharp
var host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "CommunityEvents";
var username = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
```

Replace with your credentials:

```csharp
var host = "localhost";
var port = "5432";
var database = "CommunityEvents";
var username = "postgres";
var password = "YOUR_PASSWORD_HERE";  // Enter your PostgreSQL password
```

And add the connection string line after:
```csharp
var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
```

### Step 4: Restore Packages and Run

Open Command Prompt or PowerShell in the project folder:

```bash
cd C:\Projects\CommunityEvents

# Restore NuGet packages
dotnet restore

# Run the application
dotnet run --urls="http://localhost:5000"
```

### Step 5: Access the Application

Open your browser and go to:
```
http://localhost:5000
```

---

## Admin Login

- **Username:** `admin`
- **Password:** `admin123`

Access the admin panel at: `http://localhost:5000/Auth/Login`

---

## Alternative: Using Environment Variables

Instead of editing `Program.cs`, you can set environment variables:

**Windows Command Prompt:**
```bash
set PGHOST=localhost
set PGPORT=5432
set PGDATABASE=CommunityEvents
set PGUSER=postgres
set PGPASSWORD=your_password
dotnet run --urls="http://localhost:5000"
```

**PowerShell:**
```powershell
$env:PGHOST="localhost"
$env:PGPORT="5432"
$env:PGDATABASE="CommunityEvents"
$env:PGUSER="postgres"
$env:PGPASSWORD="your_password"
dotnet run --urls="http://localhost:5000"
```

---

## Using Visual Studio 2022 (Optional)

1. Open the `.csproj` file with Visual Studio 2022
2. Right-click the project -> "Properties"
3. Go to "Debug" -> "General" -> "Open debug launch profiles UI"
4. Add environment variables there
5. Press F5 to run

---

## Troubleshooting

### "Connection refused" error
- Make sure PostgreSQL service is running
- Open Windows Services (services.msc) and look for "postgresql-x64-16"
- Start it if it's stopped

### "Password authentication failed"
- Double-check your PostgreSQL password in Program.cs
- Try resetting your PostgreSQL password using pgAdmin

### "Database does not exist"
- Make sure you created the `CommunityEvents` database (Step 2)

### "Port already in use"
- Change the port: `dotnet run --urls="http://localhost:5001"`

### Entity Framework errors
- The database tables are created automatically on first run
- If issues persist, drop and recreate the database

---

## Project Structure

```
CommunityEvents/
|-- Controllers/          # MVC Controllers (handles user requests)
|-- Models/               # Data Models (defines database entities)
|-- Services/             # Business Logic Layer
|-- Views/                # Razor Views (HTML templates)
|-- wwwroot/              # Static files (CSS, JS, images)
|-- Data/                 # Database Context
|-- Filters/              # Action Filters
|-- Exceptions/           # Custom Exception Classes
|-- Program.cs            # Application entry point
|-- CommunityEventSystem.csproj  # Project file
```

---

## Application Features

1. **Browse Events** - View upcoming community events with details
2. **Register for Events** - Users can sign up (registration requires admin approval)
3. **Admin Dashboard** - Manage events, venues, activities, and participants
4. **Registration Approval** - Admin reviews and approves/rejects user registrations
5. **Secure Authentication** - Cookie-based authentication with session management

---

## Security Features

- Cookie-based authentication with HttpOnly and Secure flags
- CSRF protection on all forms
- Role-based authorization for admin routes
- Security headers (X-Frame-Options, X-Content-Type-Options, X-XSS-Protection)
- Input validation on all forms

---

## Need Help?

If you encounter any issues, check:
1. .NET SDK is installed: `dotnet --version`
2. PostgreSQL is running
3. Database `CommunityEvents` exists
4. Password in Program.cs matches your PostgreSQL password
