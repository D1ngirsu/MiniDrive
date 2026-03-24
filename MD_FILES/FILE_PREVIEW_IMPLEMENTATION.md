# File Preview Feature Implementation

## Overview
File preview functionality has been successfully implemented for MiniDrive. This feature allows users to get previews of their files without having to download the entire file.

## Components Implemented

### 1. **File Preview DTO** (`FilePreviewResponse.cs`)
Located in: `src/MiniDrive.Files/DTOs/FilePreviewResponse.cs`

**Key Classes:**
- `FilePreviewResponse` - Response object containing:
  - `FileId` - The ID of the file
  - `FileName` - Original filename
  - `ContentType` - MIME type
  - `Extension` - File extension
  - `SizeBytes` - File size in bytes
  - `PreviewType` - Enum indicating the type of preview
  - `IsPreviewAvailable` - Boolean indicating if preview is available
  - `PreviewContent` - The actual preview content (varies by type)
  - `PreviewDataUrl` - Data URL for images (can be used directly in img src)
  - `PreviewSize` - Size of the preview data
  - `PreviewUnavailableReason` - Message explaining why preview is unavailable

**PreviewType Enum:**
- `None` (0) - No preview available
- `Text` (1) - Text-based preview
- `Image` (2) - Image preview
- `Pdf` (3) - PDF preview information
- `Document` (4) - Document preview
- `Audio` (5) - Audio file preview
- `Video` (6) - Video file preview
- `Code` (7) - Code file preview
- `Archive` (8) - Archive preview

### 2. **File Preview Service Interface** (`IFilePreviewService.cs`)
Located in: `src/MiniDrive.Files/Services/IFilePreviewService.cs`

**Key Methods:**
```csharp
Task<FilePreviewResponse> GetPreviewAsync(
    FileEntry file,
    int maxPreviewSize = 100 * 1024,
    bool includeContent = true);

bool SupportsPreview(string contentType, string extension);

Task<string> GetImageThumbnailAsync(
    Stream fileStream,
    string contentType,
    int maxWidth = 200,
    int maxHeight = 200);
```

### 3. **File Preview Service Implementation** (`FilePreviewService.cs`)
Located in: `src/MiniDrive.Files/Services/FilePreviewService.cs`

**Supported File Types:**

**Text Files:**
- `.txt`, `.md`, `.json`, `.xml`, `.csv`, `.log`, `.html`, `.css`, `.js`, `.ts`, `.jsx`, `.tsx`, `.java`, `.cs`, `.cpp`, `.c`, `.h`, `.py`, `.rb`, `.php`, `.sql`, `.yml`, `.yaml`

**Images:**
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.svg`, `.bmp`, `.ico`

**Documents:**
- `.pdf`, `.docx`, `.doc`, `.xlsx`, `.xls`, `.pptx`, `.ppt`, `.odt`, `.ods`, `.odp`

**Video Files:**
- `.mp4`, `.avi`, `.mov`, `.mkv`, `.wmv`, `.flv`, `.webm`, `.m4v`, `.mpg`, `.mpeg`

**Audio Files:**
- `.mp3`, `.wav`, `.flac`, `.aac`, `.ogg`, `.wma`, `.m4a`, `.aiff`

**Archives:**
- `.zip`, `.rar`, `.7z`, `.tar`, `.gz`, `.bz2`, `.xz`, `.iso`

**Features:**
- Type detection based on MIME type and file extension
- Content extraction for text files (first 100KB by default)
- Base64 encoding for images
- Size limiting to prevent memory issues
- Graceful error handling for unsupported file types

### 4. **IFileService Interface Update**
Located in: `src/MiniDrive.Files/Services/IFileService.cs`

**New Method:**
```csharp
Task<Result<FilePreviewResponse>> GetFilePreviewAsync(
    Guid fileId,
    Guid ownerId,
    int maxPreviewSize = 100 * 1024,
    bool includeContent = true);
```

### 5. **FileService Implementation Update**
Located in: `src/MiniDrive.Files/Services/FileService.cs`

**Changes:**
- Added `IFilePreviewService` dependency injection
- Implemented `GetFilePreviewAsync` method that:
  - Verifies file ownership
  - Delegates preview generation to `FilePreviewService`
  - Returns result with proper error handling

### 6. **FileController Endpoint**
Located in: `src/MiniDrive.Files.Api/Controllers/FileController.cs`

**New Endpoint:**
```http
GET /api/files/{id}/preview
```

**Query Parameters:**
- `includeContent` (bool, default: true) - Whether to include actual preview content
- `maxPreviewSize` (int, default: 102400) - Maximum preview size in bytes

**Response:** `FilePreviewResponse` object

**Example Requests:**
```
GET /api/files/123e4567-e89b-12d3-a456-426614174000/preview
GET /api/files/123e4567-e89b-12d3-a456-426614174000/preview?includeContent=false
GET /api/files/123e4567-e89b-12d3-a456-426614174000/preview?maxPreviewSize=50000
```

### 7. **Service Registration**
Located in: `src/MiniDrive.Files.Api/Program.cs`

**Added:**
```csharp
builder.Services.AddScoped<IFilePreviewService, FilePreviewService>();
```

### 8. **Integration Tests**
Located in: `test/IntegrationTests/FilePreviewTests.cs`

**Test Cases:**
- Endpoint availability
- Query parameter support
- Authorization requirements
- Content-Type validation
- HTTP method support
- Error handling
- File type support verification

## Usage Examples

### Get Full Preview with Content
```http
GET /api/files/{fileId}/preview?includeContent=true&maxPreviewSize=102400
Authorization: Bearer {token}
```

**Response (Text File):**
```json
{
  "fileId": "123e4567-e89b-12d3-a456-426614174000",
  "fileName": "example.txt",
  "contentType": "text/plain",
  "extension": ".txt",
  "sizeBytes": 1024,
  "previewType": 1,
  "isPreviewAvailable": true,
  "previewContent": "File content here...",
  "previewDataUrl": null,
  "previewSize": 1024,
  "previewUnavailableReason": null
}
```

### Get Metadata Only
```http
GET /api/files/{fileId}/preview?includeContent=false
Authorization: Bearer {token}
```

**Response:**
```json
{
  "fileId": "123e4567-e89b-12d3-a456-426614174000",
  "fileName": "image.jpg",
  "contentType": "image/jpeg",
  "extension": ".jpg",
  "sizeBytes": 2048000,
  "previewType": 2,
  "isPreviewAvailable": true,
  "previewContent": null,
  "previewDataUrl": null,
  "previewSize": 2048000,
  "previewUnavailableReason": null
}
```

### Image Preview with Base64 Data URL
```http
GET /api/files/{fileId}/preview?includeContent=true&maxPreviewSize=5242880
Authorization: Bearer {token}
```

**Response:**
```json
{
  "fileId": "123e4567-e89b-12d3-a456-426614174000",
  "fileName": "photo.jpg",
  "contentType": "image/jpeg",
  "extension": ".jpg",
  "sizeBytes": 2048000,
  "previewType": 2,
  "isPreviewAvailable": true,
  "previewContent": "data:image/jpeg;base64,/9j/4AAQSkZJRg...",
  "previewDataUrl": "data:image/jpeg;base64,/9j/4AAQSkZJRg...",
  "previewSize": 2048000,
  "previewUnavailableReason": null
}
```

## Security Considerations

1. **Authorization:** All preview endpoints require valid Bearer token authentication
2. **Ownership Verification:** User can only preview their own files
3. **Size Limiting:** Configurable max preview size to prevent memory exhaustion
4. **Type Validation:** File type detection prevents execution of dangerous file types
5. **Safe Encoding:** Image data is safely base64 encoded

## Performance Considerations

1. **Default Limits:**
   - Max preview size: 100KB (configurable)
   - Images up to 5MB can be previewed as data URLs
   
2. **Streaming:** Uses streamed reading to avoid loading entire files into memory

3. **Caching:** Can be further optimized with Redis caching for frequently previewed files

## Future Enhancements

1. **Document Preview:** Implement server-side PDF/Office document rendering
2. **Code Syntax Highlighting:** Add syntax highlighting for code files
3. **Thumbnail Generation:** Generate and cache thumbnails for images
4. **Advanced Analytics:** Track preview usage statistics
5. **Client-side Preview Rendering:** Integrate with PDF.js, Office.js for rich previews
6. **Batch Previews:** Support bulk preview requests
7. **Preview Caching:** Cache previews in Redis for frequently accessed files

## Files Modified/Created

**Created:**
- `src/MiniDrive.Files/DTOs/FilePreviewResponse.cs`
- `src/MiniDrive.Files/Services/IFilePreviewService.cs`
- `src/MiniDrive.Files/Services/FilePreviewService.cs`
- `test/IntegrationTests/FilePreviewTests.cs`

**Modified:**
- `src/MiniDrive.Files/Services/IFileService.cs` - Added GetFilePreviewAsync method
- `src/MiniDrive.Files/Services/FileService.cs` - Implemented GetFilePreviewAsync and added service dependency
- `src/MiniDrive.Files.Api/Controllers/FileController.cs` - Added preview endpoint
- `src/MiniDrive.Files.Api/Program.cs` - Registered FilePreviewService

## Compilation Status

✅ **Build Successful** - All core functionality compiles without errors.

The file preview feature is now ready for use.
