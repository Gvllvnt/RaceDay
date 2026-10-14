```markdown
# 🏃 RaceDay - Event Management System

## Overview
RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day.

This project is built progressively across three parts:
- **Part 1:** System Planning & Database Design
- **Part 2:** RESTful API Development (Current)
- **Part 3:** MVC Web Application (Upcoming)

---

## 📁 Project Structure

```

RaceDay/
├── docs/                       # Part 1: Planning documents
│   ├── ERD.png                 # Entity Relationship Diagram
│   ├── API_Endpoint_Plan.md    # API specification
│   ├── RaceDay_Schema.sql      # Database creation script
│   └── green-build-part2.png   # CI/CD green build screenshot
├── RaceDay.API/                # Part 2: RESTful API
│   ├── Controllers/            # API endpoints
│   ├── Models/                 # Domain entities
│   ├── DTOs/                   # Data transfer objects
│   ├── Data/                   # EF Core DbContext
│   ├── Services/               # Business logic
│   └── Migrations/             # EF Core migrations
├── RaceDay.Tests/              # Part 2: Unit tests
│   ├── JarredAuthTests.cs
│   ├── JarredEventTests.cs
│   ├── JarredEnrolmentTests.cs
│   └── JarredRoleTests.cs
├── .github/workflows/ci.yml    # CI/CD pipeline
├── .gitignore
├── RaceDay.sln
└── README.md

```

---

## 👥 Two User Roles

### 🎯 Organiser
- Create, update, and delete events
- Manage event categories (distances, fees, capacity)
- View all enrolments for their events
- Capture participant results (finish times, positions)

### 🏃 Participant
- Register and log in
- Browse and search events
- Enrol in event categories
- View personal enrolment history
- Track personal results and performance

**Role-based access is enforced at the API level using JWT authentication.**

---

## 🛠️ Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server (or LocalDB)
- Visual Studio 2022

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/Gvllvnt/RaceDay.git
   cd RaceDay
```

2. Restore packages
   ```bash
   dotnet restore RaceDay.sln
   ```
3. Update database
   ```bash
   cd RaceDay.API
   dotnet ef database update
   ```
4. Run the API
   ```bash
   dotnet run --project RaceDay.API
   ```
   Swagger opens at: https://localhost:7XXX (check console for the port)
5. Run tests
   ```bash
   dotnet test RaceDay.sln
   ```

---

🧪 Seed Data for Testing

Organiser Login

· Email: john@raceday.co.za
· Password: Password123!

Participant Login

· Email: thabo@gmail.com
· Password: Password123!

---

🧪 Unit Tests

All unit tests are named after Jarred (because Jarred loves unit tests!). Tests cover:

· Authentication: Registration, login, password hashing, duplicate emails
· Events: Create, update, soft delete, filtering
· Enrolments: Enrol, status updates, deletion
· Role-based access: Participant/Organiser boundaries

Run tests with:

```bash
dotnet test RaceDay.sln
```

---

📸 CI/CD Green Build

docs/green-build-part2.png

The CI/CD pipeline is configured in .github/workflows/ci.yml and runs on every push. It:

1. Restores dependencies
2. Builds the solution
3. Runs all unit tests
4. Validates the /docs folder exists

---


📚 API Documentation

The full API is testable via Swagger UI when the app is running. All endpoints from the Part 1 plan have been implemented.

Key Endpoint Groups

| Group | Method | Endpoint | Purpose |
|---|---|---|---|
| **Auth** | POST | `/api/auth/register` | Register a new user |
| | POST | `/api/auth/login` | Log in and get a token |
| | GET | `/api/auth/me` | Get the current authenticated user |
| **Users** | GET | `/api/users/profile` | Get own profile |
| | PUT | `/api/users/profile` | Update own profile |
| | GET | `/api/users/{id}/enrolments` | List a user's enrolments |
| **Events** | GET | `/api/events` | List events |
| | GET | `/api/events/{id}` | Get one event |
| | POST | `/api/events` | Create an event |
| | PUT | `/api/events/{id}` | Update an event |
| | DELETE | `/api/events/{id}` | Delete an event |
| | GET | `/api/events/upcoming` | List upcoming events |
| | GET | `/api/events/search` | Search events |
| **Categories** | GET | `/api/events/{eventId}/categories` | List an event's categories |
| | POST | `/api/events/{eventId}/categories` | Add a category to an event |
| | PUT | `/api/categories/{id}` | Update a category |
| | DELETE | `/api/categories/{id}` | Delete a category |
| **Enrolments** | POST | `/api/events/{eventId}/enrol` | Enrol in an event |
| | GET | `/api/enrolments/{id}` | Get one enrolment |
| | GET | `/api/events/{eventId}/enrolments` | List an event's enrolments |
| | PUT | `/api/enrolments/{id}/status` | Update enrolment status |
| | DELETE | `/api/enrolments/{id}` | Cancel/remove an enrolment |
| **Results** | POST | `/api/enrolments/{enrolmentId}/results` | Record a result for an enrolment |
| | PUT | `/api/results/{id}` | Update a result |
| | GET | `/api/events/{eventId}/results` | List an event's results |
| | GET | `/api/participants/{id}/results` | List a participant's results |


Authentication

The API uses JWT Bearer tokens. After logging in:

1. Copy the token from the login response
2. In Swagger, click Authorize
3. Paste: Bearer YOUR_TOKEN_HERE

---

🗄️ Database Schema

The database uses the following entities:

· Users — Organisers and Participants (with role discriminator)
· Events — Road running, walking, and cycling events
· Categories — Event categories (distance/age groups)
· Enrolments — Participant registrations
· Results — Participant finish times and positions

See docs/ERD.png for the full diagram.


📝 Author

Thamanda Sobekwa
ST10460780
ST10460780@rcconnect.edu.za
9 October 2026