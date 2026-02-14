# Docker Setup Guide

This guide explains how to run the MiniDrive microservices application using Docker and Docker Compose.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (or Docker Engine + Docker Compose)
- At least 4GB of available RAM
- At least 10GB of available disk space

## Quick Start

1. **Build and start all services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   # All services
   docker-compose logs -f
   
   # Specific service
   docker-compose logs -f identity-api
   ```

3. **Stop all services:**
   ```bash
   docker-compose down
   ```

4. **Stop and remove volumes (clean slate):**
   ```bash
   docker-compose down -v
   ```

## Services

The docker-compose setup includes:

- **SQL Server** (Port 1433) - Database server
- **Redis** (Port 6379) - Caching server
- **Identity API** (Port 5001) - Authentication service
- **Files API** (Port 5002) - File operations service
- **Folders API** (Port 5003) - Folder management service
- **Quota API** (Port 5004) - Storage quota service
- **Audit API** (Port 5005) - Audit logging service
- **Gateway API** (Port 5000) - API Gateway (main entry point)

## Accessing Services

- **API Gateway:** http://localhost:5000
- **Identity Service:** http://localhost:5001
- **Files Service:** http://localhost:5002
- **Folders Service:** http://localhost:5003
- **Quota Service:** http://localhost:5004
- **Audit Service:** http://localhost:5005

## Health Checks

- **Gateway Health:** http://localhost:5000/health
- **Aggregate Health:** http://localhost:5000/health/aggregate
- **Individual Service Health:**
  - http://localhost:5001/health
  - http://localhost:5002/health
  - http://localhost:5003/health
  - http://localhost:5004/health
  - http://localhost:5005/health

## Database Setup

The SQL Server container will be created automatically. The default credentials are:
- **Server:** localhost,1433 (or `sqlserver` from within Docker network)
- **Username:** sa
- **Password:** YourStrong@Passw0rd

**⚠️ Important:** Change the password in production!

### Running Migrations

After the services start, you may need to run Entity Framework migrations. You can do this by:

1. **Executing migrations from within a container:**
   ```bash
   docker-compose exec identity-api dotnet ef database update --project /src/MiniDrive.Identity
   docker-compose exec files-api dotnet ef database update --project /src/MiniDrive.Files
   docker-compose exec folders-api dotnet ef database update --project /src/MiniDrive.Folders
   docker-compose exec quota-api dotnet ef database update --project /src/MiniDrive.Quota
   docker-compose exec audit-api dotnet ef database update --project /src/MiniDrive.Audit
   ```

2. **Or using a migration script** (recommended for production)

## Configuration

### Environment Variables

You can override configuration using environment variables in `docker-compose.yml`:

- Connection strings
- Redis connection
- Service URLs
- JWT settings

### Volumes

- `sqlserver-data` - SQL Server database files
- `redis-data` - Redis persistence
- `files-storage` - File storage directory

## Building Individual Services

To build a specific service:

```bash
docker-compose build identity-api
```

## Troubleshooting

### Services won't start

1. **Check logs:**
   ```bash
   docker-compose logs [service-name]
   ```

2. **Verify SQL Server is healthy:**
   ```bash
   docker-compose ps
   ```

3. **Check if ports are already in use:**
   ```bash
   # Windows PowerShell
   netstat -ano | findstr :5000
   
   # Linux/Mac
   lsof -i :5000
   ```

### Database connection issues

- Ensure SQL Server container is healthy: `docker-compose ps`
- Check connection string format in environment variables
- Verify SQL Server is accessible: `docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"`

### Service communication issues

- Services communicate using service names (e.g., `http://identity-api:5001`)
- Ensure all services are on the same Docker network (`minidrive-network`)
- Check service dependencies in `docker-compose.yml`

## Production Considerations

Before deploying to production:

1. **Change default passwords:**
   - SQL Server SA password
   - JWT signing keys

2. **Use secrets management:**
   - Docker secrets
   - Environment variable files (`.env`)
   - External secret management (Azure Key Vault, AWS Secrets Manager, etc.)

3. **Configure proper networking:**
   - Use reverse proxy (nginx, Traefik)
   - Set up SSL/TLS certificates
   - Configure firewall rules

4. **Set up monitoring:**
   - Application Insights
   - Prometheus + Grafana
   - ELK stack

5. **Backup strategy:**
   - Regular database backups
   - File storage backups
   - Disaster recovery plan

6. **Resource limits:**
   - Add CPU and memory limits to services
   - Configure restart policies

## Development Workflow

1. **Make code changes**
2. **Rebuild affected services:**
   ```bash
   docker-compose build [service-name]
   docker-compose up -d [service-name]
   ```

3. **Or rebuild everything:**
   ```bash
   docker-compose up -d --build
   ```

## Additional Commands

- **View running containers:** `docker-compose ps`
- **Restart a service:** `docker-compose restart [service-name]`
- **Execute command in container:** `docker-compose exec [service-name] [command]`
- **Remove all containers and volumes:** `docker-compose down -v --remove-orphans`

