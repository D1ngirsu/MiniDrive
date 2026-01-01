# Microservices Architecture Setup

This document describes the microservices architecture migration from the monolithic MiniDrive application.

## Architecture Overview

The application has been split into the following microservices:

1. **Identity Service** (Port 5001) - Authentication and user management
2. **Files Service** (Port 5002) - File operations (upload, download, delete)
3. **Folders Service** (Port 5003) - Folder management
4. **Quota Service** (Port 5004) - Storage quota management
5. **Audit Service** (Port 5005) - Audit logging
6. **API Gateway** (Port 5000) - Routes requests to appropriate microservices

## Service Dependencies

- **Files Service** depends on: Identity, Quota, Audit
- **Folders Service** depends on: Identity
- **Quota Service** - Standalone
- **Audit Service** - Standalone
- **Identity Service** - Standalone

## Inter-Service Communication

Services communicate via HTTP using client libraries in `MiniDrive.Clients`:
- `IIdentityClient` - For authentication/authorization
- `IQuotaClient` - For quota checks and updates
- `IAuditClient` - For audit logging

## Running the Services

### Option 1: Individual Services

Run each service separately:

```bash
# Terminal 1 - Identity Service
cd src/MiniDrive.Identity.Api
dotnet run --urls "http://localhost:5001"

# Terminal 2 - Files Service
cd src/MiniDrive.Files.Api
dotnet run --urls "http://localhost:5002"

# Terminal 3 - Folders Service
cd src/MiniDrive.Folders.Api
dotnet run --urls "http://localhost:5003"

# Terminal 4 - Quota Service
cd src/MiniDrive.Quota.Api
dotnet run --urls "http://localhost:5004"

# Terminal 5 - Audit Service
cd src/MiniDrive.Audit.Api
dotnet run --urls "http://localhost:5005"

# Terminal 6 - API Gateway
cd src/MiniDrive.Gateway.Api
dotnet run --urls "http://localhost:5000"
```

### Option 2: Docker Compose

A `docker-compose.yml` file is provided for running all services together.

## Configuration

Each service has its own `appsettings.json` with:
- Database connection strings
- Redis configuration
- Service URLs for inter-service communication

Update the `Services` section in each service's configuration to point to the correct service URLs.

## API Gateway

The API Gateway routes requests to the appropriate microservice:
- `/api/Auth/*` → Identity Service
- `/api/File/*` → Files Service
- `/api/Folder/*` → Folders Service
- `/api/Quota/*` → Quota Service
- `/api/Audit/*` → Audit Service

## Migration Notes

1. **Authentication**: Services now use `IIdentityClient` instead of direct `AuthServices` dependency
2. **Quota Management**: Files service uses `IQuotaClient` via adapter pattern
3. **Audit Logging**: Services use `IAuditClient` for audit operations
4. **Database**: Each service maintains its own database (already separated in the original architecture)

## Health Checks

Each service exposes a health check endpoint:
- `GET /health` - Returns service health status

The API Gateway provides:
- `GET /health` - Gateway health status
- `GET /health/aggregate` - Aggregated health status of all downstream services

Example:
```bash
curl http://localhost:5000/health/aggregate
```

## Next Steps

1. ✅ Complete setup for Folders, Quota, and Audit API services
2. ✅ Implement API Gateway routing logic
3. Add service discovery (optional - for production)
4. ✅ Add health checks for each service
5. Set up monitoring and logging aggregation
6. Configure load balancing (for production)

