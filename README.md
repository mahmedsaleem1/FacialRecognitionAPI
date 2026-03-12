# Facial Recognition Attendance API

A high-performance .NET 10 Web API for tracking employee attendance using facial recognition. Designed for internee/employee onboarding and automated attendance marking via face detection and verification.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Development Mode](#development-mode)
- [Project Structure](#project-structure)
- [License](#license)

---

## Features

- **Employee Onboarding** — Register employees with a face photo; auto-generates a unique employee code (`INT-20250225-A3F7`).
- **Face-Based Attendance** — Check in / check out by submitting a live face image. Supports both:
  - **1:1 Verification** — Pass an `employeeId` for direct comparison.
  - **1:N Identification** — Omit the ID and the system searches all active employees.
- **Cloudinary Image Storage** — Reference face photos are uploaded to Cloudinary with smart-crop transforms.
- **AES-256-GCM Encrypted Embeddings** — Face embeddings are encrypted at rest with unique nonce/tag per record.
- **Admin Dashboard Analytics** — Daily, weekly, and monthly attendance summaries with per-department breakdowns.
- **ONNX-Based Face Processing** — UltraFace detection + ArcFace recognition models running via ONNX Runtime.
- **Structured Logging** — Serilog with console + rolling file sinks, enriched with machine name and thread ID.
- **Global Error Handling** — Centralized exception middleware returning consistent `ApiResponse` payloads.
- **Rate Limiting** — Per-IP fixed-window rate limiter (100 requests/minute).
- **Repository Pattern** — Generic + specialized repositories with clean separation of concerns.
- **Development Mode** — Hash-based pseudo-embeddings when ONNX models are not available.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers                          │
│  EmployeeController │ AttendanceController │ Analytics  │
├─────────────────────────────────────────────────────────┤
│                    Services                             │
│  EmployeeService │ AttendanceService │ AnalyticsService │
│  CloudinaryService │ FacialRecognitionService           │
│  EncryptionService                                      │
├─────────────────────────────────────────────────────────┤
│                   Repositories                          │
│  EmployeeRepository │ AttendanceRepository │ Generic    │
├─────────────────────────────────────────────────────────┤
│              Entity Framework Core 10                   │
│                   SQL Server                            │
└─────────────────────────────────────────────────────────┘
```

- **Controllers** — Thin HTTP layer; validation, routing, response shaping.
- **Services** — Business logic, face processing, encryption, Cloudinary uploads.
- **Repositories** — Data access with EF Core; generic base + specialized queries.
- **Middleware** — Global exception handling, Serilog request logging.

---

## Technology Stack

| Component | Technology |
|---|---|
| Framework | .NET 10 (ASP.NET Core) |
| Database | SQL Server + EF Core 10 |
| Face Detection | UltraFace ONNX model |
| Face Recognition | ArcFace ONNX model |
| ML Runtime | Microsoft.ML.OnnxRuntime 1.22.0 |
| Image Processing | SixLabors.ImageSharp 3.1.12 |
| Image Storage | Cloudinary (CloudinaryDotNet 1.28.0) |
| Encryption | AES-256-GCM (System.Security.Cryptography) |
| Logging | Serilog.AspNetCore 10.0.0 |
| Similarity | SIMD-accelerated cosine similarity |

---

## Database Schema

### Employees

| Column | Type | Description |
|---|---|---|
| Id | `uniqueidentifier` | Primary key (NEWSEQUENTIALID) |
| EmployeeCode | `nvarchar(20)` | Auto-generated unique code |
| FullName | `nvarchar(150)` | Employee name |
| Email | `nvarchar(256)` | Unique email address |
| Phone | `nvarchar(20)` | Optional phone |
| Department | `nvarchar(100)` | Department name |
| Position | `nvarchar(100)` | Job title |
| CloudinaryImageUrl | `nvarchar(1000)` | Reference face photo URL |
| EncryptedEmbedding | `varbinary(4096)` | AES-256-GCM encrypted face embedding |
| EncryptionIv | `varbinary(16)` | Unique nonce per record |
| EncryptionTag | `varbinary(32)` | GCM authentication tag |
| IsActive | `bit` | Soft-delete flag |
| JoinDate | `date` | Employee start date |
| CreatedAt / UpdatedAt | `datetime2` | Audit timestamps |

### AttendanceRecords

| Column | Type | Description |
|---|---|---|
| Id | `uniqueidentifier` | Primary key |
| EmployeeId | `uniqueidentifier` | FK → Employees |
| Date | `date` | Attendance date |
| CheckInTime | `time` | Check-in timestamp |
| CheckOutTime | `time` | Check-out timestamp (nullable) |
| CheckInSimilarityScore | `float` | Face match confidence at check-in |
| CheckOutSimilarityScore | `float` | Face match confidence at check-out |
| Status | `nvarchar(20)` | Present, Late, HalfDay, Absent, Excused |
| Notes | `nvarchar(500)` | Optional notes |

**Constraint:** Unique composite index on `(EmployeeId, Date)` — one check-in per employee per day.

---

## API Endpoints

### Employee Management

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/employee` | Onboard a new employee with face image |
| `GET` | `/api/employee` | List all active employees |
| `GET` | `/api/employee/{id}` | Get employee by ID |
| `GET` | `/api/employee/code/{code}` | Get employee by employee code |
| `PUT` | `/api/employee/{id}` | Update employee profile |
| `PUT` | `/api/employee/{id}/face` | Update employee face image |
| `PATCH` | `/api/employee/{id}/deactivate` | Soft-delete (deactivate) |
| `DELETE` | `/api/employee/{id}` | Permanently delete employee |

### Attendance

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/attendance/check-in` | Mark check-in via face image |
| `POST` | `/api/attendance/check-out` | Mark check-out via face image |
| `GET` | `/api/attendance/today` | Today's attendance for all employees |
| `GET` | `/api/attendance/today/{employeeId}` | Today's attendance for one employee |
| `GET` | `/api/attendance` | Paginated records with filters |

**Query parameters for `GET /api/attendance`:** `from`, `to`, `department`, `employeeId`, `page`, `pageSize`

### Analytics (Admin Dashboard)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/analytics/dashboard` | Overview: totals, today's stats, averages, departments |
| `GET` | `/api/analytics/daily?date=` | Daily attendance summary |
| `GET` | `/api/analytics/weekly?weekStart=` | Weekly summary (Mon–Fri) |
| `GET` | `/api/analytics/monthly?year=&month=&department=` | Monthly reports per employee |
| `GET` | `/api/analytics/employee/{id}/monthly?year=&month=` | Single employee monthly report |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (or SQL Server Express / LocalDB)
- [Cloudinary account](https://cloudinary.com/) (free tier works)
- *(Optional)* ONNX face models — not needed in Development Mode

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/FacialRecognitionAPI.git
   cd FacialRecognitionAPI
   ```

2. **Configure settings**

   Edit `appsettings.Development.json` with your values:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=FacialRecognitionDb_Dev;Trusted_Connection=true;TrustServerCertificate=true"
     },
     "Cloudinary": {
       "CloudName": "your-cloud-name",
       "ApiKey": "your-api-key",
       "ApiSecret": "your-api-secret"
     },
     "EncryptionSettings": {
       "AesKey": "<base64-encoded-32-byte-key>"
     }
   }
   ```

   Generate an AES key:
   ```bash
   dotnet script -e "Console.WriteLine(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)))"
   ```
   Or use any Base64-encoded 32-byte value.

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the API**
   ```bash
   dotnet run
   ```

5. **Test the API**
   ```
   https://localhost:5001/api/analytics/dashboard
   https://localhost:5001/health
   ```

---

## Configuration

### `appsettings.json` sections

| Section | Purpose |
|---|---|
| `ConnectionStrings` | SQL Server connection string |
| `Cloudinary` | CloudName, ApiKey, ApiSecret, UploadFolder |
| `EncryptionSettings` | AES-256 key (Base64) for embedding encryption |
| `FaceRecognition` | ONNX model paths, thresholds, development mode toggle |
| `Attendance` | WorkdayStart, GraceMinutes, HalfDayThresholdMinutes, WorkdayEnd |
| `Serilog` | Log levels and overrides |

### Attendance Status Logic

| Status | Condition |
|---|---|
| **Present** | Check-in within `WorkdayStart + GraceMinutes` |
| **Late** | Check-in after grace period but within `HalfDayThresholdMinutes` |
| **HalfDay** | Check-in after `HalfDayThresholdMinutes` past start |
| **Absent** | No check-in recorded for the day |
| **Excused** | Manually set via admin |

---

## Development Mode

When `FaceRecognition:UseDevelopmentMode` is `true`:

- ONNX models are **not required**.
- Face embeddings are generated using a deterministic hash of the image bytes.
- All face comparisons still work (same image → same embedding → high similarity).
- Useful for local development, testing, and CI/CD pipelines.

Set in `appsettings.Development.json`:
```json
{
  "FaceRecognition": {
    "UseDevelopmentMode": true
  }
}
```

---

## Project Structure

```
FacialRecognitionAPI/
├── Configuration/
│   ├── AttendanceSettings.cs
│   ├── CloudinarySettings.cs
│   ├── EncryptionSettings.cs
│   └── FaceRecognitionSettings.cs
├── Controllers/
│   ├── AnalyticsController.cs
│   ├── AttendanceController.cs
│   └── EmployeeController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── AttendanceRecordConfiguration.cs
│   │   └── EmployeeConfiguration.cs
│   └── Migrations/
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs
├── Models/
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   ├── AttendanceQueryParams.cs
│   │   │   ├── CheckOutRequest.cs
│   │   │   ├── MarkAttendanceRequest.cs
│   │   │   ├── OnboardEmployeeRequest.cs
│   │   │   ├── UpdateEmployeeFaceRequest.cs
│   │   │   └── UpdateEmployeeRequest.cs
│   │   └── Responses/
│   │       ├── ApiResponse.cs
│   │       ├── AttendanceResponse.cs
│   │       ├── DailyAttendanceSummary.cs
│   │       ├── DashboardOverviewResponse.cs
│   │       ├── EmployeeMonthlyReport.cs
│   │       ├── EmployeeResponse.cs
│   │       ├── MarkAttendanceResponse.cs
│   │       └── PagedResult.cs
│   └── Entities/
│       ├── AttendanceRecord.cs
│       └── Employee.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IAttendanceRepository.cs
│   │   ├── IEmployeeRepository.cs
│   │   └── IRepository.cs
│   ├── AttendanceRepository.cs
│   ├── EmployeeRepository.cs
│   └── Repository.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAnalyticsService.cs
│   │   ├── IAttendanceService.cs
│   │   ├── ICloudinaryService.cs
│   │   ├── IEmployeeService.cs
│   │   ├── IEncryptionService.cs
│   │   └── IFacialRecognitionService.cs
│   ├── AnalyticsService.cs
│   ├── AttendanceService.cs
│   ├── CloudinaryService.cs
│   ├── EmployeeService.cs
│   ├── EncryptionService.cs
│   └── FacialRecognitionService.cs
├── OnnxModels/                  (gitignored)
├── Logs/                        (gitignored)
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── FacialRecognitionAPI.csproj
```

---

## Dashboard API Reference (for React Frontend)

**Base URL:** `http://localhost:{port}/api/dashboard`
**All responses are JSON with `camelCase` property names. Null fields are omitted.**
**All date query params use `yyyy-MM-dd` format (e.g. `2026-03-12`).**
**All datetime strings in responses use ISO 8601 (e.g. `2026-03-12T08:30:00.0000000Z`).**
**CORS is fully open — any origin, method, header allowed.**

---

### 1. GET `/api/dashboard/summary`

**Purpose:** Main dashboard overview card — totals, today's stats, recent activity.
**Query params:** None

**Response 200:**
```json
{
  "totalEmployees": 50,
  "todayPresentCount": 42,
  "todayAbsentCount": 8,
  "todayAttendanceRate": 84.0,
  "newEmployeesThisMonth": 3,
  "averageAttendanceRateLast30Days": 87.5,
  "recentActivity": [
    {
      "employeeId": "guid-string",
      "fullName": "John Doe",
      "department": "Engineering",
      "markedAt": "2026-03-12T08:30:00.0000000Z",
      "status": "present"
    }
  ]
}
```
`recentActivity` contains up to 10 most recent attendance marks.

---

### 2. GET `/api/dashboard/employees`

**Purpose:** Paginated employee list with search & filter. For employee management table.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `search` | string? | null | Searches by name or email (case-insensitive) |
| `department` | string? | null | Filter by exact department name (case-insensitive) |
| `page` | int | 1 | Page number (min 1) |
| `pageSize` | int | 20 | Items per page (min 1, max 100) |

**Response 200:**
```json
{
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "employees": [
    {
      "uuid": "guid-string",
      "fullName": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890",
      "department": "Engineering",
      "position": "Senior Developer",
      "joinDate": "2025-06-15",
      "createdAt": "2025-06-15T10:00:00.0000000Z",
      "totalAttendanceDays": 180,
      "attendanceRate": 92.31,
      "lastAttendanceDate": "2026-03-12T08:30:00.0000000Z"
    }
  ]
}
```
- `attendanceRate` — percentage (0–100) from join date to today, working days only (Mon–Fri).
- `lastAttendanceDate` — null if employee has never marked attendance.

---

### 3. GET `/api/dashboard/employees/{id}`

**Purpose:** Single employee detail card/modal.
**Path param:** `id` — Employee GUID

**Response 200:**
```json
{
  "uuid": "guid-string",
  "fullName": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "department": "Engineering",
  "position": "Senior Developer",
  "joinDate": "2025-06-15",
  "createdAt": "2025-06-15T10:00:00.0000000Z",
  "totalAttendanceDays": 180,
  "attendanceRate": 92.31,
  "lastAttendanceDate": "2026-03-12T08:30:00.0000000Z"
}
```
**Error 404:** `{ "message": "Employee not found." }`

---

### 4. GET `/api/dashboard/employees/by-department`

**Purpose:** Department breakdown for pie/bar chart or summary cards.
**Query params:** None

**Response 200:**
```json
{
  "departments": [
    {
      "department": "Engineering",
      "employeeCount": 20,
      "averageAttendanceRate": 0,
      "todayPresentCount": 17,
      "todayAbsentCount": 3
    },
    {
      "department": "Unassigned",
      "employeeCount": 2,
      "averageAttendanceRate": 0,
      "todayPresentCount": 1,
      "todayAbsentCount": 1
    }
  ]
}
```
Employees without a department show as `"Unassigned"`.

---

### 5. DELETE `/api/dashboard/attendance/{id}`

**Purpose:** Delete an attendance record by its ID.
**Path param:** `id` — Attendance record GUID

**Response 204:** No content (success, empty body).
**Error 404:** `{ "message": "Attendance record not found." }`

---

### 6. GET `/api/dashboard/attendance/overview`

**Purpose:** Daily attendance breakdown for a date range. Good for bar charts.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 6 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |

**Response 200:**
```json
{
  "fromDate": "2026-03-06",
  "toDate": "2026-03-12",
  "totalWorkingDays": 5,
  "totalEmployees": 50,
  "averageAttendanceRate": 85.6,
  "dailySummaries": [
    {
      "date": "2026-03-06",
      "presentCount": 43,
      "absentCount": 7,
      "attendanceRate": 86.0
    }
  ]
}
```
Weekends (Sat/Sun) are excluded from `dailySummaries`.

---

### 7. GET `/api/dashboard/attendance/monthly`

**Purpose:** Full month report with per-employee breakdown. For monthly report table.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `year` | int? | current year | Year |
| `month` | int? | current month | Month (1–12) |

**Response 200:**
```json
{
  "year": 2026,
  "month": 3,
  "totalWorkingDays": 22,
  "totalEmployees": 50,
  "averageAttendanceRate": 87.3,
  "records": [
    {
      "employeeId": "guid-string",
      "fullName": "John Doe",
      "department": "Engineering",
      "daysPresent": 20,
      "daysAbsent": 2,
      "attendanceRate": 90.91
    }
  ]
}
```
Records sorted by `daysPresent` descending. `totalWorkingDays` capped to today if month hasn't ended.

---

### 8. GET `/api/dashboard/attendance/employee/{id}`

**Purpose:** Individual employee attendance calendar/history view.
**Path param:** `id` — Employee GUID

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 29 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |

**Response 200:**
```json
{
  "employeeId": "guid-string",
  "fullName": "John Doe",
  "department": "Engineering",
  "totalDaysPresent": 18,
  "attendanceRate": 90.0,
  "history": [
    {
      "date": "2026-03-12",
      "markedAt": "2026-03-12T08:30:00.0000000Z",
      "status": "present"
    }
  ]
}
```
History sorted newest first. **Error 404:** `{ "message": "Employee not found." }`

---

### 9. GET `/api/dashboard/analytics/attendance-trend`

**Purpose:** Line/area chart data points for attendance over time.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 29 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |

**Response 200:**
```json
{
  "fromDate": "2026-02-11",
  "toDate": "2026-03-12",
  "dataPoints": [
    {
      "date": "2026-02-11",
      "presentCount": 40,
      "totalEmployees": 50,
      "attendanceRate": 80.0
    }
  ]
}
```
Weekends excluded. One data point per working day.

---

### 10. GET `/api/dashboard/analytics/department-stats`

**Purpose:** Department-wise analytics. For grouped bar chart or comparison table.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 29 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |

**Response 200:**
```json
{
  "departments": [
    {
      "department": "Engineering",
      "employeeCount": 20,
      "averageAttendanceRate": 91.5,
      "todayPresentCount": 18,
      "todayAbsentCount": 2
    }
  ]
}
```
`averageAttendanceRate` is over the full date range. `todayPresentCount`/`todayAbsentCount` are always for today.

---

### 11. GET `/api/dashboard/analytics/top-attendees`

**Purpose:** Leaderboard / top performers widget.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 29 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |
| `count` | int | 10 | Number of top employees (min 1, max 100) |

**Response 200:**
```json
{
  "fromDate": "2026-02-11",
  "toDate": "2026-03-12",
  "rankings": [
    {
      "rank": 1,
      "employeeId": "guid-string",
      "fullName": "Jane Smith",
      "department": "Engineering",
      "daysPresent": 20,
      "totalWorkingDays": 20,
      "attendanceRate": 100.0
    }
  ]
}
```

---

### 12. GET `/api/dashboard/analytics/low-attendees`

**Purpose:** Flagged employees / at-risk list.

**Query params:**

| Param | Type | Default | Description |
|---|---|---|---|
| `from` | string? | 29 days before `to` | Start date `yyyy-MM-dd` |
| `to` | string? | today | End date `yyyy-MM-dd` |
| `threshold` | double | 75 | Percentage threshold (0–100). Returns employees BELOW this rate |

**Response 200:**
```json
{
  "fromDate": "2026-02-11",
  "toDate": "2026-03-12",
  "threshold": 75.0,
  "employees": [
    {
      "rank": 1,
      "employeeId": "guid-string",
      "fullName": "Low Performer",
      "department": "Sales",
      "daysPresent": 10,
      "totalWorkingDays": 20,
      "attendanceRate": 50.0
    }
  ]
}
```
Sorted by attendance rate ascending (worst first). `rank` is 1-indexed.

---

### Error Responses (all endpoints)

| Status | Body |
|---|---|
| **400** | `{ "message": "description of what's wrong" }` |
| **404** | `{ "message": "Employee not found." }` |
| **429** | Rate limit exceeded (100 req/min per IP) |
| **500** | `{ "message": "An unexpected error occurred." }` |

---

## License

This project is provided as-is for educational and internal use.
