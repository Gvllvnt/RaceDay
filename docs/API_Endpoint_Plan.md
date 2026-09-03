# RaceDay API Endpoint Plan

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/auth/register | Register new participant account | None (Public) | {email, password, fullName} | 201 Created - User object |
| POST | /api/auth/login | Authenticate user and return JWT token | None (Public) | {email, password} | 200 OK - {token, user} |

## Users

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/users/profile | Get current user's profile | Any (Logged in) | None | 200 OK - User object |
| PUT | /api/users/profile | Update current user's profile | Any (Logged in) | {fullName, email} | 200 OK - Updated user |
| GET | /api/users/{id}/enrolments | Get participant's enrolments | Participant (own) or Organiser | None | 200 OK - List of enrolments |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/events | Get all events | None (Public) | None | 200 OK - List of events |
| GET | /api/events/{id} | Get single event with categories | None (Public) | None | 200 OK - Event object |
| POST | /api/events | Create new event | Organiser | {title, description, eventDate, location, routeInfo, weatherInfo} | 201 Created - Event object |
| PUT | /api/events/{id} | Update event | Organiser (owner) | {title, description, eventDate, location, routeInfo, weatherInfo, isActive} | 200 OK - Updated event |
| DELETE | /api/events/{id} | Delete event (soft delete) | Organiser (owner) | None | 204 No Content |
| GET | /api/events/{id}/weather | Get weather info for event | None (Public) | None | 200 OK - Weather info |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/events/{eventId}/categories | Get all categories for event | None (Public) | None | 200 OK - List of categories |
| POST | /api/events/{eventId}/categories | Add category to event | Organiser (owner) | {name, distanceKm, startTime, entryFee, maxParticipants} | 201 Created - Category object |
| PUT | /api/categories/{id} | Update category | Organiser (owner) | {name, distanceKm, startTime, entryFee, maxParticipants} | 200 OK - Updated category |
| DELETE | /api/categories/{id} | Delete category | Organiser (owner) | None | 204 No Content |

## Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/events/{eventId}/enrol | Participant enrols in event | Participant | {categoryId} | 201 Created - Enrolment object |
| GET | /api/enrolments/{id} | Get specific enrolment | Participant (own) or Organiser | None | 200 OK - Enrolment object |
| PUT | /api/enrolments/{id}/status | Update enrolment status | Organiser (owner) | {status} | 200 OK - Updated enrolment |
| GET | /api/events/{eventId}/enrolments | Get all enrolments for event | Organiser (owner) | None | 200 OK - List of enrolments |
| DELETE | /api/enrolments/{id} | Cancel enrolment | Participant (own) or Organiser | None | 204 No Content |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/enrolments/{enrolmentId}/results | Add result for participant | Organiser (owner) | {finishTime, position, pace} | 201 Created - Result object |
| PUT | /api/results/{id} | Update participant result | Organiser (owner) | {finishTime, position, pace} | 200 OK - Updated result |
| GET | /api/events/{eventId}/results | Get all results for event | Organiser (owner) | None | 200 OK - List of results |
| GET | /api/participants/{id}/results | Get participant's result history | Participant (own) or Organiser | None | 200 OK - List of results |

## Utility Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/events/upcoming | Get upcoming events | None (Public) | None | 200 OK - List of events |
| GET | /api/events/search | Search events by title/location | None (Public) | None | 200 OK - List of events |
| GET | /api/categories/{id}/enrolments/count | Get enrolment count for category | None (Public) | None | 200 OK - {categoryId, count, maxParticipants} |