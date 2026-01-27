# MiniDrive Sharing Service

The Sharing Service is a microservice responsible for managing file and folder sharing functionality in MiniDrive. It supports both user-to-user sharing with granular permissions and public link sharing with optional password protection.

## Features

### User-to-User Sharing
- Share files and folders with specific users
- Fine-grained permission control (view, edit, admin)
- Expiration dates for shares
- Track who resources are shared with

### Public Link Sharing
- Generate public shareable links for files and folders
- Optional password protection
- Download limit tracking
- Automatic expiration after date
- Token-based access without authentication

### Share Management
- Create, update, and delete shares
- List shares created by a user
- List shares shared with a user
- View shares for a specific resource
- Verify and manage share permissions

## Architecture

### Components

1. **Share Entity** - Represents a sharing relationship
2. **SharingDbContext** - Entity Framework Core database context
3. **ShareRepository** - Data access layer
4. **ShareService** - Business logic and operations
5. **ShareController** - API endpoints

### Database Schema

The Shares table stores:
- `Id` - Share identifier
- `ResourceId` - File or folder ID
- `ResourceType` - "file" or "folder"
- `OwnerId` - User who created the share
- `SharedWithUserId` - Target user (null for public shares)
- `Permission` - "view", "edit", or "admin"
- `IsPublicShare` - Whether it's a public link
- `ShareToken` - Public token for link shares
- `IsActive` - Whether share is currently active
- `ExpiresAtUtc` - Optional expiration date
- `PasswordHash` - SHA256 hash of password (for public shares)
- `MaxDownloads` - Download limit for public shares
- `CurrentDownloads` - Current download count
- `Notes` - Optional notes about the share
- Timestamps - CreatedAtUtc, UpdatedAtUtc

## API Endpoints

### Create Share
```
POST /api/Share
Authorization: Bearer {token}
Content-Type: application/json

{
  "resourceId": "guid",
  "resourceType": "file",        // or "folder"
  "sharedWithUserId": "guid",    // null for public shares
  "permission": "view",          // view, edit, admin
  "isPublicShare": false,
  "password": "optional",
  "expiresAtUtc": "2026-12-31T23:59:59Z",
  "maxDownloads": 10,
  "notes": "optional notes"
}
```

### Get Share
```
GET /api/Share/{id}
Authorization: Bearer {token}
```

### Get My Shares
```
GET /api/Share/my-shares
Authorization: Bearer {token}
```

### Get Shares Shared With Me
```
GET /api/Share/shared-with-me
Authorization: Bearer {token}
```

### Get Resource Shares
```
GET /api/Share/resource/{resourceId}?resourceType=file
Authorization: Bearer {token}
```

### Get Public Share
```
GET /api/Share/public/{token}
```

### Access Protected Public Share
```
POST /api/Share/public/{token}/access
Content-Type: application/json

{
  "password": "SecurePassword123"
}
```

### Update Share
```
PUT /api/Share/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "permission": "edit",
  "isActive": true,
  "expiresAtUtc": "2026-06-30T23:59:59Z",
  "password": "newPassword",  // or empty string to remove
  "maxDownloads": 20,
  "notes": "updated notes"
}
```

### Delete Share
```
DELETE /api/Share/{id}
Authorization: Bearer {token}
```

## Service Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "SharingDb": "Server=(localdb)\\mssqllocaldb;Database=MiniDrive_Sharing;Trusted_Connection=true;"
  },
  "Services": {
    "Identity": "http://localhost:5001",
    "Audit": "http://localhost:5005"
  },
  "Redis": {
    "Connection": "localhost:6379"
  }
}
```

## Permissions

Permission levels control what actions a shared user can perform:

- **view** - Read-only access to the resource
- **edit** - Can read and modify the resource
- **admin** - Full control including sharing and deletion

## Public Links

Public links are ideal for:
- One-time file downloads
- Sharing with external users
- Secure file distribution with passwords
- Limited distribution with download counts
- Time-limited access

## Running the Service

### Locally
```bash
cd src/MiniDrive.Sharing.Api
dotnet run --urls "http://localhost:5006"
```

### Docker
```bash
docker build -f src/MiniDrive.Sharing.Api/Dockerfile -t minidrive-sharing .
docker run -p 5006:5006 minidrive-sharing
```

### Docker Compose
```bash
docker-compose up sharing
```

## Dependencies

- **Identity Service** - User authentication and validation
- **Audit Service** - Audit logging (future enhancement)
- **Database** - SQL Server for persistence
- **Redis** - Caching (optional)

## Testing

Use the provided `MiniDrive.Sharing.Api.http` file with REST Client extension:

```http
### Create a share
POST http://localhost:5006/api/Share
Authorization: Bearer {token}
Content-Type: application/json

{
  "resourceId": "123e4567-e89b-12d3-a456-426614174000",
  "resourceType": "file",
  "sharedWithUserId": "650e8400-e29b-41d4-a716-446655440001",
  "permission": "view"
}
```

## Security Considerations

1. **Token Validation** - Share tokens are 32-character alphanumeric strings
2. **Password Hashing** - Passwords are hashed with SHA256
3. **Expiration** - Expired shares automatically become inactive
4. **Download Limits** - Public shares can have download count restrictions
5. **Soft Deletes** - Shares are soft-deleted for audit trail
6. **Ownership Verification** - Only owners can modify their shares

## Future Enhancements

- Share audit logging
- Share invitation emails
- Bulk share operations
- Share analytics
- Advanced permission models
- Share comments/activity tracking
