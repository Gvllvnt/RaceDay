-- ============================================================
-- DATABASE: RaceDayDB
-- DESCRIPTION: RaceDay Event Management System
-- AUTHOR: Thamanda Sobekwa
-- DATE: 17 August 2026
-- ============================================================

-- Drop database if it exists (for clean testing)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
    DROP DATABASE RaceDayDB;
GO

-- Create Database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================================
-- TABLE: Users
-- ============================================================
CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) NOT NULL CHECK (role IN ('Organiser', 'Participant')),
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- TABLE: Events
-- ============================================================
CREATE TABLE Events (
    event_id INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id INT NOT NULL,
    title NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000) NULL,
    event_date DATETIME NOT NULL,
    location NVARCHAR(200) NOT NULL,
    route_info NVARCHAR(1000) NULL,
    weather_info NVARCHAR(500) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    is_active BIT DEFAULT 1,
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (organiser_id) 
        REFERENCES Users(user_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE: Categories
-- ============================================================
CREATE TABLE Categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL,
    name NVARCHAR(100) NOT NULL,
    distance_km DECIMAL(8,2) NOT NULL CHECK (distance_km > 0),
    start_time TIME NOT NULL,
    entry_fee DECIMAL(18,2) NOT NULL CHECK (entry_fee >= 0),
    max_participants INT NOT NULL CHECK (max_participants > 0),
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Categories_Event FOREIGN KEY (event_id) 
        REFERENCES Events(event_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE: Enrolments
-- ============================================================
CREATE TABLE Enrolments (
    enrolment_id INT IDENTITY(1,1) PRIMARY KEY,
    participant_id INT NOT NULL,
    category_id INT NOT NULL,
    race_number INT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'Registered' 
        CHECK (status IN ('Registered', 'Completed', 'DNS', 'DNF')),
    enrolled_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (participant_id) 
        REFERENCES Users(user_id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (category_id) 
        REFERENCES Categories(category_id) ON DELETE CASCADE,
    -- Ensure one participant per category
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (participant_id, category_id),
    -- Race number should be unique (we'll handle event-unique via application logic)
    CONSTRAINT UQ_Enrolments_RaceNumber UNIQUE (race_number)
);
GO

-- ============================================================
-- TABLE: Results
-- ============================================================
CREATE TABLE Results (
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL,
    finish_time TIME NULL,
    position INT NULL,
    pace NVARCHAR(20) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (enrolment_id) 
        REFERENCES Enrolments(enrolment_id) ON DELETE CASCADE,
    -- One result per enrolment
    CONSTRAINT UQ_Results_Enrolment UNIQUE (enrolment_id)
);
GO

-- ============================================================
-- INDEXES for Performance
-- ============================================================
CREATE INDEX IX_Events_OrganiserId ON Events(organiser_id);
CREATE INDEX IX_Events_EventDate ON Events(event_date);
CREATE INDEX IX_Events_IsActive ON Events(is_active);
CREATE INDEX IX_Categories_EventId ON Categories(event_id);
CREATE INDEX IX_Enrolments_ParticipantId ON Enrolments(participant_id);
CREATE INDEX IX_Enrolments_CategoryId ON Enrolments(category_id);
CREATE INDEX IX_Enrolments_Status ON Enrolments(status);
CREATE INDEX IX_Enrolments_RaceNumber ON Enrolments(race_number);
CREATE INDEX IX_Results_EnrolmentId ON Results(enrolment_id);
CREATE INDEX IX_Results_Position ON Results(position);
GO

-- ============================================================
-- STORED PROCEDURE: Get Event Enrolment Count
-- ============================================================
CREATE PROCEDURE sp_GetCategoryEnrolmentCount
    @category_id INT
AS
BEGIN
    SELECT 
        c.category_id,
        c.name,
        c.max_participants,
        COUNT(e.enrolment_id) AS current_enrolments,
        (c.max_participants - COUNT(e.enrolment_id)) AS spots_remaining
    FROM Categories c
    LEFT JOIN Enrolments e ON c.category_id = e.category_id
    WHERE c.category_id = @category_id
    GROUP BY c.category_id, c.name, c.max_participants;
END;
GO

-- ============================================================
-- STORED PROCEDURE: Get Participant Results History
-- ============================================================
CREATE PROCEDURE sp_GetParticipantResults
    @participant_id INT
AS
BEGIN
    SELECT 
        u.full_name,
        e.title AS event_name,
        e.event_date,
        c.name AS category_name,
        c.distance_km,
        r.finish_time,
        r.position,
        r.pace,
        er.status
    FROM Users u
    INNER JOIN Enrolments er ON u.user_id = er.participant_id
    INNER JOIN Categories c ON er.category_id = c.category_id
    INNER JOIN Events e ON c.event_id = e.event_id
    LEFT JOIN Results r ON er.enrolment_id = r.enrolment_id
    WHERE u.user_id = @participant_id
    ORDER BY e.event_date DESC;
END;
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Insert Organisers
INSERT INTO Users (email, password_hash, full_name, role) VALUES
('john.organiser@raceday.co.za', '$2a$12$KxQxVrXyZ1ABC123DEF456...', 'John Van Der Merwe', 'Organiser'),
('sarah.organiser@raceday.co.za', '$2a$12$LmNxWrYzA2BCD234EFG567...', 'Sarah Naidoo', 'Organiser'),
('mike.organiser@raceday.co.za', '$2a$12$OpQxYrZbC3CDE345FGH678...', 'Mike Khumalo', 'Organiser');
GO

-- Insert Participants
INSERT INTO Users (email, password_hash, full_name, role) VALUES
('thabo.runner@gmail.com', '$2a$12$AbBcDeFgHiJkLmNoPqRsTu...', 'Thabo Mokoena', 'Participant'),
('lisa.athlete@yahoo.com', '$2a$12$VwXyZaBcDeFgHiJkLmNoPq...', 'Lisa Van Wyk', 'Participant'),
('james.cyclist@gmail.com', '$2a$12$RsTuVwXyZaBcDeFgHiJkLm...', 'James Botha', 'Participant'),
('zanele.walker@gmail.com', '$2a$12$NoPqRsTuVwXyZaBcDeFgHi...', 'Zanele Ndlovu', 'Participant'),
('peter.runner@gmail.com', '$2a$12$JkLmNoPqRsTuVwXyZaBcDe...', 'Peter De Villiers', 'Participant');
GO

-- Insert Events
INSERT INTO Events (organiser_id, title, description, event_date, location, route_info, weather_info, is_active) VALUES
(1, 'Comrades Marathon 2026', 
 'The Ultimate Human Race - 90km from Pietermaritzburg to Durban. South Africa''s most iconic ultra-marathon.', 
 '2026-06-01 05:30:00', 
 'Pietermaritzburg to Durban', 
 'Start at Pietermaritzburg City Hall, proceed along the N3 highway, through the Valley of a Thousand Hills, finishing at Kingsmead Stadium in Durban.', 
 'Mild temperatures, possible rain. Average 15-22°C.', 
 1),

(1, 'Cape Town Cycle Tour 2026',
 'The world''s largest timed cycle race. 109km scenic route around the Cape Peninsula.',
 '2026-03-08 06:00:00',
 'Cape Town',
 'Start at Cape Town Stadium, along the M3, through Muizenberg, Cape Point, Scarborough, Kommetjie, and finish at the University of Cape Town.',
 'Sunny, light breeze. Average 18-25°C.',
 1),

(2, 'Two Oceans Marathon 2026',
 'The world''s most beautiful marathon. 56km ultra and 21.1km half marathon along the Cape Peninsula.',
 '2026-04-15 05:45:00',
 'Cape Town',
 'Start at UCT, along Main Road, through Fish Hoek, Cape Point, Scarborough, and finish at UCT.',
 'Mild, possible mist. Average 15-22°C.',
 1),

(3, 'Soweto Marathon 2026',
 'The People''s Race - 42.2km through the streets of Soweto, celebrating South African heritage.',
 '2026-11-02 06:00:00',
 'Soweto, Johannesburg',
 'Start at FNB Stadium, through Orlando, Diepkloof, Meadowlands, and finish at the iconic Soweto Towers.',
 'Summer conditions, warm. Average 20-30°C.',
 1),

(1, 'Durban Park Run Festival 2026',
 'Community 5km fun walk and run. Family-friendly event celebrating health and fitness.',
 '2026-09-25 07:00:00',
 'Durban Botanic Gardens',
 'Loop through the Durban Botanic Gardens, along the pathways, with beautiful views of the gardens and surrounding area.',
 'Warm, humid. Average 22-28°C.',
 1);
GO

-- Insert Categories
INSERT INTO Categories (event_id, name, distance_km, start_time, entry_fee, max_participants) VALUES
-- Comrades Marathon Categories
(1, 'Elite Men (Ultra)', 90.00, '05:30', 1500.00, 150),
(1, 'Elite Women (Ultra)', 90.00, '05:30', 1500.00, 150),
(1, 'Open Men (Ultra)', 90.00, '06:00', 800.00, 5000),
(1, 'Open Women (Ultra)', 90.00, '06:00', 800.00, 4500),
(1, 'Veteran Men (50+)', 90.00, '06:15', 600.00, 2000),

-- Cape Town Cycle Tour Categories
(2, 'Elite Men (Cycle)', 109.00, '06:00', 1200.00, 200),
(2, 'Elite Women (Cycle)', 109.00, '06:00', 1200.00, 200),
(2, 'Open Men (Cycle)', 109.00, '06:30', 700.00, 10000),
(2, 'Open Women (Cycle)', 109.00, '06:30', 700.00, 8000),
(2, 'Fun Ride (Cycle)', 78.00, '07:00', 500.00, 5000),

-- Two Oceans Marathon Categories
(3, 'Ultra Marathon Men', 56.00, '05:45', 950.00, 3000),
(3, 'Ultra Marathon Women', 56.00, '05:45', 950.00, 2000),
(3, 'Half Marathon Men', 21.10, '06:15', 500.00, 4000),
(3, 'Half Marathon Women', 21.10, '06:15', 500.00, 3000),
(3, 'Trail Run (10km)', 10.00, '07:00', 350.00, 1000),

-- Soweto Marathon Categories
(4, 'Full Marathon Men', 42.20, '06:00', 650.00, 3000),
(4, 'Full Marathon Women', 42.20, '06:00', 650.00, 2000),
(4, 'Half Marathon Men', 21.10, '06:30', 400.00, 4000),
(4, 'Half Marathon Women', 21.10, '06:30', 400.00, 3000),
(4, 'Fun Walk (10km)', 10.00, '07:00', 200.00, 2000),

-- Durban Park Run Categories
(5, '5km Run', 5.00, '07:00', 50.00, 500),
(5, '5km Walk', 5.00, '07:00', 50.00, 300),
(5, 'Kids Fun Run (2km)', 2.00, '07:30', 30.00, 200);
GO

-- Insert Enrolments
DECLARE @EventID1 INT = (SELECT event_id FROM Events WHERE title = 'Comrades Marathon 2026');
DECLARE @EventID2 INT = (SELECT event_id FROM Events WHERE title = 'Cape Town Cycle Tour 2026');
DECLARE @EventID3 INT = (SELECT event_id FROM Events WHERE title = 'Two Oceans Marathon 2026');

-- Thabo enrols in Comrades Open Men
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    CASE WHEN u.user_id = 4 THEN 'Completed' ELSE 'Registered' END,
    DATEADD(DAY, -FLOOR(RAND() * 60), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'thabo.runner@gmail.com' 
  AND c.name = 'Open Men (Ultra)'
  AND c.event_id = @EventID1;

-- Lisa enrols in Two Oceans Half Marathon
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    'Registered',
    DATEADD(DAY, -FLOOR(RAND() * 45), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'lisa.athlete@yahoo.com' 
  AND c.name = 'Half Marathon Women'
  AND c.event_id = @EventID3;

-- James enrols in Cape Town Cycle Tour Open Men
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    'Registered',
    DATEADD(DAY, -FLOOR(RAND() * 30), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'james.cyclist@gmail.com' 
  AND c.name = 'Open Men (Cycle)'
  AND c.event_id = @EventID2;

-- Zanele enrols in Two Oceans Trail Run
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    'Registered',
    DATEADD(DAY, -FLOOR(RAND() * 20), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'zanele.walker@gmail.com' 
  AND c.name = 'Trail Run (10km)'
  AND c.event_id = @EventID3;

-- Peter enrols in Soweto Marathon Full Marathon
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    'Registered',
    DATEADD(DAY, -FLOOR(RAND() * 15), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'peter.runner@gmail.com' 
  AND c.name = 'Full Marathon Men'
  AND c.event_id = (SELECT event_id FROM Events WHERE title = 'Soweto Marathon 2026');

-- More enrolments for Thabo
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    CASE 
        WHEN c.name LIKE '%Half Marathon%' AND c.event_id = @EventID3 THEN 'Completed'
        ELSE 'Registered'
    END,
    DATEADD(DAY, -FLOOR(RAND() * 90), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'thabo.runner@gmail.com' 
  AND c.name IN ('Half Marathon Men', 'Fun Walk (10km)')
  AND c.event_id IN (@EventID3, (SELECT event_id FROM Events WHERE title = 'Soweto Marathon 2026'));

-- Peter also enrols in Two Oceans Half Marathon
INSERT INTO Enrolments (participant_id, category_id, race_number, status, enrolled_at)
SELECT 
    u.user_id,
    c.category_id,
    FLOOR(RAND() * 90000 + 10000) AS race_number,
    'Registered',
    DATEADD(DAY, -FLOOR(RAND() * 10), GETDATE())
FROM Users u
CROSS JOIN Categories c
WHERE u.email = 'peter.runner@gmail.com' 
  AND c.name = 'Half Marathon Men'
  AND c.event_id = @EventID3;
GO

-- Insert Results (for completed events)
INSERT INTO Results (enrolment_id, finish_time, position, pace, created_at)
SELECT 
    e.enrolment_id,
    DATEADD(MINUTE, FLOOR(RAND() * 300 + 120), '00:00:00') AS finish_time,
    FLOOR(RAND() * 500 + 1) AS position,
    CONCAT(FLOOR(RAND() * 10 + 3), ':', RIGHT('0' + CAST(FLOOR(RAND() * 60) AS VARCHAR), 2), '/km') AS pace,
    GETDATE()
FROM Enrolments e
WHERE e.status = 'Completed';
GO

-- ============================================================
-- VERIFICATION QUERIES
-- ============================================================

-- Check all tables
SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM Results;

-- View sample data
SELECT '=== Sample Data Summary ===' AS Info;

SELECT 
    u.full_name,
    u.email,
    u.role,
    COUNT(e.event_id) AS events_created
FROM Users u
LEFT JOIN Events e ON u.user_id = e.organiser_id
WHERE u.role = 'Organiser'
GROUP BY u.user_id, u.full_name, u.email, u.role;

SELECT 
    u.full_name,
    u.email,
    COUNT(en.enrolment_id) AS total_enrolments,
    COUNT(CASE WHEN en.status = 'Completed' THEN 1 END) AS completed_events
FROM Users u
LEFT JOIN Enrolments en ON u.user_id = en.participant_id
WHERE u.role = 'Participant'
GROUP BY u.user_id, u.full_name, u.email;

-- Run the stored procedure to check enrolment counts
EXEC sp_GetCategoryEnrolmentCount 1;
GO