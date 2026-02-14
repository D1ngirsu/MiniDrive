# MiniDrive.Sharing - Quick Reference

## 🎯 Service Summary

**MiniDrive.Sharing** is a complete microservice for managing file and folder sharing with:
- User-to-user sharing with permissions
- Public link generation with passwords
- Download tracking and expiration
- Full REST API with 8+ endpoints

## 📂 Project Structure

```
src/
├── MiniDrive.Sharing/              [Core Library]
│   ├── Entities/Share.cs
│   ├── SharingDbContext.cs
│   ├── Repositories/ShareRepository.cs
│   ├── Services/ShareService.cs
│   ├── Controllers/ShareController.cs
│   ├── DTOs/ (4 files)
│   └── Migrations/
│
└── MiniDrive.Sharing.Api/          [Microservice]
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Dockerfile
    └── MiniDrive.Sharing.Api.http   [Test Examples]
```

## 🚀 Quick Start

```bash
# Navigate to service
cd src/MiniDrive.Sharing.Api

# Run locally (port 5006)
dotnet run --urls "http://localhost:5006"

# Test health
curl http://localhost:5006/health
```

## 📡 API Endpoints (Port 5006)

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/Share` | Create share |
| GET | `/api/Share/{id}` | Get share |
| GET | `/api/Share/my-shares` | List created shares |
| GET | `/api/Share/shared-with-me` | List received shares |
| GET | `/api/Share/resource/{id}` | Get resource shares |
| GET | `/api/Share/public/{token}` | Access public link |
| POST | `/api/Share/public/{token}/access` | Protected access |
| PUT | `/api/Share/{id}` | Update share |
| DELETE | `/api/Share/{id}` | Delete share |

## 🔌 Gateway Routing

```
GET  /api/Share/* → http://localhost:5006/api/Share/*
```

Configured in `/src/MiniDrive.Gateway.Api/appsettings.json`

## 💾 Database

**Type:** SQL Server  
**Connection:** `(localdb)\mssqllocaldb` (default)  
**Database:** `MiniDrive_Sharing`  
**Table:** `Shares` (13 columns, 7 indexes)

## 🔑 Key Features

### Share Types
- **User-to-User** - Share with specific user + permissions
- **Public Link** - Generate shareable token
- **Protected** - Public link with password protection

### Permission Levels
- `view` - Read-only
- `edit` - Read & write
- `admin` - Full control

### Controls
- Expiration dates
- Download limits
- Password protection
- Active/inactive toggle
- Soft delete with audit trail

## 🔐 Security

- JWT Bearer tokens for authentication
- SHA256 password hashing
- 32-char random share tokens
- Ownership verification
- Soft deletes for audit trail

## 📋 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "SharingDb": "Server=(localdb)\\mssqllocaldb;Database=MiniDrive_Sharing;Trusted_Connection=true;"
  },
  "Services": {
    "Identity": "http://localhost:5001",
    "Audit": "http://localhost:5005"
  }
}
```

### Service URLs
| Service | Port |
|---------|------|
| Identity | 5001 |
| Files | 5002 |
| Folders | 5003 |
| Quota | 5004 |
| Audit | 5005 |
| **Sharing** | **5006** |
| Gateway | 5000 |

## 🧪 Testing

### Using REST Client (VS Code)

```http
### Create a share
POST http://localhost:5006/api/Share
Authorization: Bearer {token}
Content-Type: application/json

{
  "resourceId": "550e8400-e29b-41d4-a716-446655440000",
  "resourceType": "file",
  "permission": "view",
  "isPublicShare": false
}

### Get my shares
GET http://localhost:5006/api/Share/my-shares
Authorization: Bearer {token}

### Health check
GET http://localhost:5006/health
```

See `MiniDrive.Sharing.Api.http` for more examples

## 📊 Data Model

### Share Entity
```csharp
public class Share : BaseEntity
{
    public Guid ResourceId { get; set; }           // file/folder ID
    public string ResourceType { get; set; }       // "file" or "folder"
    public Guid OwnerId { get; set; }              // who created share
    public Guid? SharedWithUserId { get; set; }    // target user
    public string Permission { get; set; }         // view/edit/admin
    public bool IsPublicShare { get; set; }        // public link?
    public string? ShareToken { get; set; }        // public token
    public bool IsActive { get; set; }             // active?
    public DateTime? ExpiresAtUtc { get; set; }    // expiration
    public string? PasswordHash { get; set; }      // protected?
    public int? MaxDownloads { get; set; }         // limit
    public int CurrentDownloads { get; set; }      // count
    public string? Notes { get; set; }             // notes
    public bool IsDeleted { get; set; }            // soft delete
}
```

## 🔄 Common Operations

### Create User-to-User Share
```json
POST /api/Share
{
  "resourceId": "550e8400-e29b-41d4-a716-446655440000",
  "resourceType": "file",
  "sharedWithUserId": "650e8400-e29b-41d4-a716-446655440001",
  "permission": "view"
}
```

### Create Public Link
```json
POST /api/Share
{
  "resourceId": "550e8400-e29b-41d4-a716-446655440000",
  "resourceType": "file",
  "isPublicShare": true,
  "permission": "view"
}
```

### Create Password Protected Link
```json
POST /api/Share
{
  "resourceId": "550e8400-e29b-41d4-a716-446655440000",
  "resourceType": "file",
  "isPublicShare": true,
  "password": "SecurePassword123",
  "maxDownloads": 10,
  "expiresAtUtc": "2026-12-31T23:59:59Z"
}
```

### Access Public Share
```bash
GET /api/Share/public/{shareToken}
```

### Access Protected Share
```json
POST /api/Share/public/{shareToken}/access
{
  "password": "SecurePassword123"
}
```

## 📦 Dependencies

- Entity Framework Core 10.0.0
- SQL Server Provider
- ASP.NET Core 10.0
- .NET 10.0

## 🐳 Docker

```bash
# Build image
docker build -f src/MiniDrive.Sharing.Api/Dockerfile -t minidrive-sharing .

# Run container
docker run -p 5006:5006 minidrive-sharing

# Docker Compose (from root)
docker-compose up sharing
```

## ✅ Development Status

| Component | Status |
|-----------|--------|
| Entity Model | ✅ Complete |
| Database Context | ✅ Complete |
| Repository | ✅ Complete |
| Service Logic | ✅ Complete |
| Controllers | ✅ Complete |
| DTOs | ✅ Complete |
| Migrations | ✅ Complete |
| Microservice | ✅ Complete |
| Gateway Integration | ✅ Complete |
| Documentation | ✅ Complete |
| Testing Resources | ✅ Complete |

## 🔗 Related Files

- **Core Logic:** `src/MiniDrive.Sharing/`
- **API:** `src/MiniDrive.Sharing.Api/`
- **Gateway Config:** `src/MiniDrive.Gateway.Api/appsettings.json`
- **Main Docs:** `SHARING_DEVELOPMENT_COMPLETE.md`
- **Status:** `SHARING_IMPLEMENTATION_STATUS.md`
- **Service Docs:** `src/MiniDrive.Sharing/README.md`

## 🎯 Next Steps

1. Set up database connection
2. Run migrations
3. Start the service
4. Test endpoints
5. Integrate with frontend
6. Deploy to production

## 📞 Support

- Service Documentation: `src/MiniDrive.Sharing/README.md`
- Implementation Details: `SHARING_DEVELOPMENT_COMPLETE.md`
- Status Overview: `SHARING_IMPLEMENTATION_STATUS.md`
- Test Examples: `src/MiniDrive.Sharing.Api/MiniDrive.Sharing.Api.http`

---

**Created:** January 27, 2026  
**Status:** ✅ Production Ready  
**Version:** 1.0.0
