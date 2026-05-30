# FinOpsFlow
A full-stack enterprise-style **Financial Operations Workflow System** built with ASP.NET Core 9.
Designed as a back-office request tracker for financial operations teams.
🔗 **Live Demo:** https://finopsflow-ap-e8bqd3bwbygaf8c4.centralus-01.azurewebsites.net
> Demo credentials:
> - Admin: `admin@finopsflow.com` / `Admin123!`
> - Manager: `manager@finopsflow.com` / `Manager123!`
> - Operations: `ops@finopsflow.com` / `Ops1234!`
> - Auditor: `auditor@finopsflow.com` / `Auditor123!`
---
## Tech Stack
| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 9, Razor Pages |
| ORM | Entity Framework Core 9 |
| Auth | ASP.NET Core Identity, Role-based authorization |
| Database | SQL Server (local) / Azure SQL Serverless (production) |
| Storage | Local file system (dev) / Azure Blob Storage (production) |
| CI/CD | GitHub Actions |
| Hosting | Azure App Service (F1) |
| Frontend | Bootstrap 5, Chart.js |
---
## Features
### Requests & Workflow
- Create, edit, and manage operational requests
- Workflow statuses: New → In Review → Waiting for Info → Approved → Rejected → Completed
- Priority levels: Low / Medium / High / Critical
- Category assignment, due dates, assignee management
### Role-Based Access Control
| Role | Permissions |
|------|------------|
| Admin | Full access |
| Manager | Edit requests, change status, assign owners, view audit log |
| Operations User | Create and view requests, add comments |
| Auditor | Read-only access |
### Comments & Attachments
- Per-request discussion thread
- File attachments (PDF, Word, Excel, images, CSV)
- Automatic storage switching: local dev vs Azure Blob Storage production
### Audit Trail
- Automatic field-level change tracking via EF Core SaveChangesAsync override
- Tracks: status changes, field updates, comments, file uploads
- Admin audit log page with filtering by user, action, date range
### Dashboard
- Summary cards: open requests, overdue, completed this month, assigned to me
- Status distribution chart (Chart.js)
- Recent activity feed
- Assignee workload visualization (Manager/Admin only)
### Search & Filtering
- Filter by status, priority, category, assignee
- Date range filtering
- Keyword search across title and description
- Overdue-only toggle
---
## Architecture
The solution follows a 3-project layered architecture:
    FinOpsFlow/
    ├── FinOpsFlow.Core/            # Domain entities, enums, interfaces, DTOs
    ├── FinOpsFlow.Infrastructure/  # EF Core DbContext, services, Azure Blob Storage
    └── FinOpsFlow.Web/             # Razor Pages, Identity UI, GitHub Actions workflow
**Core** — pure domain logic, no framework dependencies  
**Infrastructure** — data access and external services (EF Core, Azure SDK)  
**Web** — presentation layer, page models, application configuration
---
## CI/CD Pipeline
Push to main triggers GitHub Actions:
1. Build (.NET 9, Release configuration)
2. Publish artifact
3. Deploy to Azure App Service
Workflow file: `.github/workflows/deploy.yml`  
Secrets: GitHub Repository Secrets + Azure App Service Environment Variables
---
## Local Setup
### Prerequisites
- .NET 9 SDK
- SQL Server or LocalDB
- Visual Studio 2022+ or VS Code
### Run locally
    git clone https://github.com/AlexeyBPopov/FinOpsFlow.git
    cd FinOpsFlow
    dotnet restore
    dotnet ef database update --project FinOpsFlow.Infrastructure --startup-project FinOpsFlow.Web
    dotnet run --project FinOpsFlow.Web
Database and seed data (users, roles, categories) are created automatically on first run.
### Configuration
`appsettings.json` contains the local SQL Server connection string.  
Production secrets are stored in Azure App Service environment variables — never committed to source control.
---
## Database Schema
| Table | Description |
|-------|-------------|
| Requests | Core workflow entities with status, priority, category |
| Categories | Request classification |
| Comments | Per-request discussion thread |
| Attachments | File metadata (content stored in Blob or local) |
| AuditLogs | Automatic field-level change history |
| AspNetUsers | Extended Identity users with department info |
---
## Key Implementation Details
**Automatic Audit Trail** — `ApplicationDbContext.SaveChangesAsync()` override uses EF Core `ChangeTracker` to detect and log field-level changes on Request entities without any manual logging in services.
**Storage Abstraction** — `IFileStorageService` interface with two implementations: `LocalFileStorageService` (development) and `AzureBlobStorageService` (production). Switched automatically based on connection string presence.
**Retry Resilience** — `EnableRetryOnFailure()` configured for Azure SQL Serverless auto-pause handling.