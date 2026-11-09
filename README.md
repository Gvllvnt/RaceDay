```markdown
# 🏃 RaceDay - Event Management System

## Overview
RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day.

This project is built progressively across three parts:
- **Part 1:** System Planning & Database Design
- **Part 2:** RESTful API Development
- **Part 3:** MVC Web Application, Azure Blob Storage and Docker (Current)

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
├── RaceDay.Web/                # Part 3: ASP.NET Core MVC front end
│   ├── Controllers/            # One controller per page group
│   ├── Infrastructure/         # API client, session helpers, auth filter
│   ├── Models/                 # API response/request shapes
│   ├── ViewModels/             # Form + page view models (validation)
│   ├── Views/                  # Razor views (Bootstrap 5)
│   ├── wwwroot/                # site.css, site.js
│   ├── Dockerfile
│   └── .dockerignore
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

## 🌐 Part 3: MVC Web Application

The MVC app (`RaceDay.Web`) is a thin front end. **It has no database access and no business rules of its own.** Every read and write goes to the Part 2 API over HTTP, including image uploads (the API stores them in Azure Blob Storage; the MVC app never talks to Azure).

### How it works
- **Authentication:** the Register/Login pages call the API. The returned JWT, user id, name and role are kept in the server-side **session**. The token is attached to every API call as a Bearer header.
- **Role-based navigation:** Organisers and Participants see different menus and pages. A `[RequireLogin("Role")]` filter redirects anonymous users to Login and wrong-role users to an Access Denied page. Logging out clears the session and returns to Login.
- **Graceful errors:** forms are validated client- and server-side; API failures become friendly messages (never raw errors or stack traces); unknown pages show a branded 404.

### Pages
| Role | Page | Route |
|---|---|---|
| Anyone | Upcoming events with filters (search, type, location) | `/` |
| Anyone | Event details + categories | `/Events/Details/{id}` |
| Anyone | Register / Log in | `/Account/Register`, `/Account/Login` |
| Participant | Enrol (button on event page), My Enrolments (colour-coded status) | `/Enrolments` |
| Participant | My Results (event, date, category, time, position e.g. "47th out of 312") | `/Results` |
| Any user | Profile (edit details, upload profile picture) | `/Profile` |
| Organiser | Dashboard (events, enrolments per event, upcoming dates) | `/Organiser/Dashboard` |
| Organiser | Events: create / edit / delete + banner upload | `/EventManagement` |
| Organiser | Categories per event | `/CategoryManagement?eventId=` |
| Organiser | Enrolments per event (+ change status, filter by category) | `/EnrolmentManagement?eventId=` |
| Organiser | Results entry/publish (after the event day) | `/ResultManagement?eventId=` |

**Status colours:** Confirmed = green, Pending/Registered = amber, Completed = blue, DNF = red, DNS = grey.

### API changes made for Part 3
| Change | Why |
|---|---|
| `POST /api/events/{id}/banner` (multipart) | Banner upload to Azure Blob Storage (Organiser owner) |
| `POST /api/users/profile/picture` (multipart) | Profile picture upload to Azure Blob Storage |
| `GET /api/events/mine` | Organiser dashboard: own events + enrolment totals |
| `GET /api/events` now supports `search` and `upcomingOnly` | Home page filtering |
| `BannerUrl` on events, `ProfilePictureUrl` on users | Stores the blob URL |
| Results include `TotalFinishers`; pace auto-calculated | "47th out of 312 finishers" |
| Enrolment statuses `Pending` and `Confirmed` added | Colour-coded statuses |
| Profile email update returns 409 if the email is taken | Avoids a 500 on duplicate emails |

---

## ⚙️ Running Part 3

### 1. Database (one-off, because two columns were added to the API)
```bash
dotnet ef migrations add Part3_BlobStorage --project RaceDay.API
dotnet ef database update --project RaceDay.API
```

### 2. Azure Blob Storage
1. Create a Storage Account in the Azure portal. Under **Configuration**, set *Allow Blob anonymous access* to **Enabled** (images are served from a public-read container).
2. Copy a connection string from **Access keys**.
3. Store it as a user secret (never commit it):
```bash
cd RaceDay.API
dotnet user-secrets init
dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>"
```
The container `raceday-images` is created automatically on first upload. For offline development you can use [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) with the connection string `UseDevelopmentStorage=true`.

### 3. Run locally
```bash
# Terminal 1 - the API (http://localhost:5206)
dotnet run --project RaceDay.API --launch-profile http

# Terminal 2 - the MVC app (http://localhost:5100)
dotnet run --project RaceDay.Web
```
The API address is read from `ApiSettings:BaseUrl` in `RaceDay.Web/appsettings.json`.

### 4. Run in Docker
The API must be reachable from inside the container, so start it on all interfaces:
```bash
dotnet run --project RaceDay.API --urls "http://0.0.0.0:5206"
```
Then build and start the MVC container with a single command:
```bash
docker build -t raceday-web ./RaceDay.Web && docker run --rm -p 8080:8080 --add-host=host.docker.internal:host-gateway -e ApiSettings__BaseUrl=http://host.docker.internal:5206 raceday-web
```
Open **http://localhost:8080**. The container behaves the same as the local run; only the API address differs (set through the `ApiSettings__BaseUrl` environment variable).

### Demo walkthrough
1. Log in as the organiser (`john@raceday.co.za`) and create an event dated **today** with a banner image.
2. Add a category (e.g. "Open 10 km").
3. Log out, register or log in as a participant (`thabo@gmail.com`), open the event and **Enrol**.
4. Back as the organiser: **Enrolments**, set the status to *Confirmed*, then **Results** and publish a finish time and position.
5. As the participant: **My Results** shows the time, position out of total finishers and pace.

*(Seed events are dated earlier in 2026, so they show as past events. Results can only be captured on or after the event date.)*

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
4. Builds the Part 3 MVC project
5. Builds the Part 3 Docker image
6. Validates the /docs folder exists

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
| | POST | `/api/users/profile/picture` | Upload profile picture (Azure Blob) |
| | GET | `/api/users/{id}/enrolments` | List a user's enrolments |
| **Events** | GET | `/api/events` | List events |
| | GET | `/api/events/{id}` | Get one event |
| | POST | `/api/events` | Create an event |
| | PUT | `/api/events/{id}` | Update an event |
| | DELETE | `/api/events/{id}` | Delete an event |
| | GET | `/api/events/upcoming` | List upcoming events |
| | GET | `/api/events/search` | Search events |
| | GET | `/api/events/mine` | Organiser's own events with totals |
| | POST | `/api/events/{id}/banner` | Upload event banner (Azure Blob) |
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