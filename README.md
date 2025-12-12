# DonClub Backend (ASP.NET Core + Clean Architecture)

The **DonClub Backend** is a complete, production-ready REST API built using **ASP.NET Core**, **Entity Framework Core**, and **Clean Architecture**.  
It powers the DonClub gaming-arena management platform and provides:

- OTP Authentication  
- JWT Access & Refresh Tokens  
- User & Role Management  
- Wallet & Transactions  
- Missions & Badges (Achievements Engine)  
- Session Management  
- Incident Reporting & Review  
- Notification System  
- Global Error Handling  
- SQL Server Persistence  

This README documents the backend architecture, setup, modules, and development guidelines.

---

# 🏗️ Architecture Overview

The project follows **Clean Architecture**:

DonClub.Api → Presentation Layer (Controllers, Middleware)
DonClub.Application → Application Layer (Use cases, Interfaces, DTOs)
DonClub.Domain → Entities, Enums, Core Business Rules
DonClub.Infrastructure → Persistence, EF Core, External Services

markdown
Copy code

### Key Principles

- **Separation of concerns**
- **Dependency inversion**
- **Testability**
- **Framework independence**

Controllers only depend on **Application** abstractions.  
Infrastructure implements the abstractions from Application.  

---

# 📁 Project Structure

/DonClub.Api
Controllers
Middlewares
Dependency Injection
Program.cs
appsettings.json

/DonClub.Application
Interfaces
DTOs
Services (Achievement Engine)
Common abstractions

/DonClub.Domain
Entities (User, Wallet, Missions, Sessions, Incidents...)
Enums
Base classes (Audit fields)

/DonClub.Infrastructure
DbContext (DonClubDbContext)
EF Configurations
Repositories
Auth (JwtTokenGenerator)
OTP / SMS Sender
WalletService
MissionService
AchievementService
NotificationService
IncidentService

yaml
Copy code

---

# 🧩 Backend Modules

Below is a detailed breakdown of all backend features and services.

---

# 🔐 Authentication & Authorization Module

## OTP Login Flow

1. User requests OTP → Rate-limit checked  
2. OTP stored in DB (SmsOtp table)  
3. OTP sent via SMS provider (dummy SMS sender for dev)  
4. User verifies OTP  
5. User created if first login  
6. Default role: **Player**  
7. JWT Access & Refresh tokens generated  
8. Refresh token stored  

## JWT Details

Token contains:

sub = userId
ClaimTypes.NameIdentifier = userId
ClaimTypes.MobilePhone = phone
ClaimTypes.Role = multiple roles
exp
iss
aud

yaml
Copy code

## Refresh Token Flow

- Stored in `RefreshTokens` table  
- Revoked upon regeneration  
- Expiration: 7 days  

## OTP Rate Limiting

Admin-configurable:

| Setting | Description |
|---------|-------------|
| IsEnabled | Toggle OTP rate limiting |
| MaxRequestsPerWindow | Number of OTP requests allowed |
| WindowMinutes | Window duration |

---

# 👤 User & Role Module

### Roles

- **SuperUser**
- **Admin**
- **Manager**
- **Player**

Users can have multiple roles.

Endpoints expose:

- User profile  
- User levels  
- Assigned missions & badges  
- Wallet summary  

---

# 💰 Wallet & Transaction Module

Each user has one Wallet.

### Wallet

Id
UserId
Balance
IsLocked
CreatedAtUtc
UpdatedAtUtc

shell
Copy code

### WalletTransaction

Id
WalletId
Amount
BalanceAfter
Direction (Credit/Debit)
Type (Reward, ManualCredit, ManualDebit)
Description
RelatedUserId
RelatedSessionId

yaml
Copy code

### Events that modify wallet:

- Mission completion  
- Badge reward  
- Manual credit  
- Manual debit  

WalletService guarantees:

- Atomic updates  
- Balance integrity  
- Transaction history  
- Notifications for credited/debited amounts  

---

# 🏆 Missions, Badges & Achievements Engine

One of the core features of DonClub.

## MissionDefinition

Code
Name
TargetValue
ConditionJson
RewardWalletAmount

sql
Copy code

Each user has corresponding `UserMission` records.

## BadgeDefinition

Name
Score
ConditionJson
RewardWalletAmount

yaml
Copy code

Users receive badges via `PlayerBadge`.

## AchievementService (Central Engine)

Event-driven workflow:

1. Update user mission progress  
2. If mission completed → reward → notification  
3. Check badge criteria → grant if applicable  
4. If badge granted → reward → notification  

### Rule Engine

`ConditionJson` defines dynamic criteria evaluated using payload events such as:

- SessionCompleted  
- VipSession  
- ManagerPerformance  
- IncidentCreated  

---

# 🎮 Sessions Module

Tracks:

- Scheduled game sessions  
- Assigned managers  
- Assigned players  
- VIP vs Normal sessions  
- Session changes and cancellation  

Notifications:

- **SessionUpdated**
- **SessionCanceled**

Session players tracked via `SessionPlayer`.

---

# 🚨 Incident Module

Managers can receive incident reports.

### Flow

1. User/Admin creates Incident  
2. Manager receives notification  
3. Admin reviews (Approve / Reject)  
4. Manager receives resolution notification  
5. Incident counts feed into KPI evaluation  

---

# 🔔 Notification System

Stored in `Notifications` table.

### Notification Types

- General  
- MissionCompleted  
- BadgeGranted  
- WalletCredited  
- SessionUpdated  
- SessionCanceled  
- IncidentCreated  
- IncidentResolved  

Endpoints allow:

- Fetching user notifications  
- Marking notifications as read  
- Admin fetching user notifications  

---

# ⚠️ Global Error Handling Middleware

Ensures consistent JSON output:

json
{
  "success": false,
  "error": {
    "code": "INVALID_OPERATION",
    "message": "Error message",
    "status": 400,
    "traceId": "..."
  }
}

Maps:

Exception	HTTP Code
InvalidOperationException	400
KeyNotFoundException	404
UnauthorizedAccessException	401

🗄️ Database Overview
Entities include:

Users, Roles, UserRoles

SmsOtps

RefreshTokens

Wallets, WalletTransactions

Missions, UserMissions

Badges, PlayerBadges

Sessions, SessionPlayers

Incidents

Notifications

SystemSettings

EF Core Code-First migrations used.

🧪 Running the Project
Prerequisites
.NET 8/10 SDK

SQL Server

NodeJS (for frontend, optional)

1. Restore Dependencies
bash
Copy code
dotnet restore
2. Update Database
bash
Copy code
dotnet ef database update --project DonClub.Infrastructure --startup-project DonClub.Api
3. Run the API
bash
Copy code
dotnet run --project DonClub.Api
API starts at:

arduino
Copy code
https://localhost:7175
http://localhost:5103
Swagger UI available at:
/swagger

🧩 Development Guidelines
Follow Clean Architecture boundaries

Keep controllers thin

Write application logic in services

Use domain models for business rules

Use DTOs for API request/response

Avoid logic inside controllers or EF models

Add unit tests for AchievementService and WalletService (recommended)

🌐 Cross-Origin Configuration (For Angular)
In Program.cs:

csharp
Copy code
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});
Activate:

csharp
Copy code
app.UseCors("AllowFrontend");
📌 Backend Roadmap (Next Steps)
Add Admin analytics endpoints (KPI, revenue, manager scoring)

Add event hooks for additional achievement types

Add audit logging

Add background worker for scheduled tasks

Add soft-delete support (optional)

📦 Deployment Notes
Use HTTPS certificate

Configure environment variables:

JWT Key

SMS Provider Key

Connection Strings

Run EF migrations on deployment target

Use reverse proxy (NGINX/IIS/Traefik)

🔥 Super Prompt for ChatGPT (Backend Context Recovery)
Use this prompt in a new ChatGPT session to reload understanding of the backend architecture:

sql
Copy code
You are ChatGPT and must continue development of the DonClub Backend project.
The backend is a Clean Architecture ASP.NET Core solution implemented with:
- OTP Authentication with rate limiting
- JWT Access + Refresh tokens
- Wallet service
- Missions and Badges with a rule engine
- AchievementService managing mission updates, badge granting, wallet rewards, and notifications
- Sessions module with update/cancel events
- Incident module with create/review flows
- Notification system with multiple types
- Global JSON error middleware
- SQL Server via EF Core Code-First

Assume all modules described above already exist and are functioning. When I ask questions or new tasks, build on top of the existing architecture without breaking it.


✔️ Conclusion
This README provides a complete overview of the DonClub Backend architecture, modules, setup, and future development plans.
