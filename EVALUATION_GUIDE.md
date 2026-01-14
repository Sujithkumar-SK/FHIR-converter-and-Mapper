# FHIR Data Converter - Evaluation Guide

## 📋 Project Summary

**Project Name:** FHIR Data Converter & Mapper  
**Purpose:** Healthcare interoperability platform that converts medical data (CSV/JSON/CCDA) to FHIR R4 standard for secure hospital-to-hospital data sharing  
**Architecture:** .NET 10 Clean Architecture + Angular 17 Frontend

---

## 🏗️ Architecture Overview

### **5-Layer Clean Architecture**

```
┌─────────────────────────────────────────┐
│  Kanini.Api (Presentation Layer)       │  ← Controllers, JWT Auth, Swagger
├─────────────────────────────────────────┤
│  Kanini.Application (Business Layer)   │  ← Services, DTOs, Validation, FHIR Conversion
├─────────────────────────────────────────┤
│  Kanini.Data (Data Access Layer)       │  ← Repositories, DbContext, ADO.NET
├─────────────────────────────────────────┤
│  Kanini.Domain (Core Layer)            │  ← Entities, Enums, Business Models
├─────────────────────────────────────────┤
│  Kanini.Common (Shared Layer)          │  ← Encryption, MagicStrings, Utilities
└─────────────────────────────────────────┘
```

**Why Clean Architecture?**
- Separation of concerns - each layer has single responsibility
- Testability - business logic isolated from infrastructure
- Maintainability - changes in one layer don't affect others
- Dependency inversion - core doesn't depend on external frameworks

---

## 🎯 Core Features Implemented

### **1. User Management & Authentication**
- **3 Role-Based Users:** Admin, Hospital, Clinic
- **JWT Authentication:** Token-based auth with HTTP-only cookies
- **OTP Verification:** Email-based registration with 5-minute expiry
- **Password Security:** SHA-256 hashing for passwords

**Key Files:**
- `AuthController.cs` - Login, Register, OTP verification endpoints
- `AuthService.cs` - Business logic for authentication
- `JwtService.cs` - JWT token generation and validation

**Flow:**
```
User Registration → Generate OTP → Send Email → Verify OTP → Create User + Organization → Login
```

### **2. FHIR Conversion Engine**
- **Supported Formats:** CSV, JSON, CCDA → FHIR R4
- **Standards Compliance:** LOINC codes for observations, UCUM for units
- **Real-time Processing:** In-memory conversion, no PHI storage

**Key Files:**
- `FhirConverter.cs` - Core FHIR conversion logic
- `TerminologyMappingService.cs` - LOINC/UCUM mappings (30+ lab tests)
- `CsvParser.cs`, `JsonParser.cs`, `CcdaParser.cs` - Format parsers

**Conversion Flow:**
```
Upload File → Detect Fields → Map to FHIR → Convert → Generate Bundle → Download
```

**FHIR Standards Applied:**
- Patient.id format: `patient-{identifier}`
- Observation.code uses LOINC (e.g., HB → 718-7)
- Observation.valueQuantity uses UCUM (e.g., g/dL)
- Bundle.type = collection (1 Patient + N Observations)

### **3. Data Request Workflow**
- **Hospital-to-Hospital Sharing:** Request patient data from other organizations
- **Approval System:** Source hospital approves/rejects requests
- **Auto-Expiration:** Requests expire after 7 days

**Key Files:**
- `DataRequestsController.cs` - CRUD operations for requests
- `DataRequestService.cs` - Business logic for workflow

**Workflow:**
```
Hospital B searches patient → Creates request → Hospital A receives → Approves → Hospital B uploads file → Converts to FHIR
```

### **4. Security Implementation**

#### **Encryption Strategy**
- **AES-256 Encryption:** For PII (names, emails, phone numbers)
- **SHA-256 Hashing:** For searchable fields (patient matching)
- **Dual Storage:** Both encrypted (retrieval) and hashed (searching)

**Key Files:**
- `EncryptionService.cs` - AES-256 encrypt/decrypt, SHA-256 hashing
- `EncryptionInterceptor.cs` - Auto-encrypt on save, auto-decrypt on read
- `[Encrypted]` attribute - Marks fields for automatic encryption

**Example:**
```csharp
[Encrypted]
public string LastName { get; set; }  // Auto-encrypted in DB

public string LastNameHash { get; set; }  // For searching
```

### **5. File Management**
- **Upload Validation:** Max 50MB, allowed types (.csv, .json, .xml)
- **Temporary Storage:** Files expire after 1 hour
- **Auto-Cleanup:** Background service removes expired files

**Key Files:**
- `FileUploadService.cs` - Handle file uploads
- `FileValidationService.cs` - Validate format and size
- `TempFileManager.cs` - Manage temporary file storage

---

## 📊 Database Design

### **Core Entities**

#### **1. User**
```csharp
- UserId (Guid, PK)
- Email (unique, indexed)
- PasswordHash (SHA-256)
- Role (Admin/Hospital/Clinic)
- OrganizationId (FK)
- IsActive, LastLogin
```

#### **2. Organization**
```csharp
- OrganizationId (Guid, PK)
- Name (encrypted)
- Type (Hospital/Clinic)
- ContactEmail, ContactPhone (encrypted)
```

#### **3. PatientIdentifier** (Dual Storage)
```csharp
- GlobalPatientId (cross-hospital identifier)
- LocalPatientId (encrypted)
- FirstName, LastName (encrypted)
- FirstNameHash, LastNameHash (for searching)
- DateOfBirth, DateOfBirthHash
```

#### **4. DataRequest**
```csharp
- RequestId (Guid, PK)
- GlobalPatientId
- RequestingOrganizationId, SourceOrganizationId
- Status (Pending/Approved/Rejected/Completed)
- ExpiresAt (7 days)
```

#### **5. ConversionJob**
```csharp
- JobId (Guid, PK)
- UserId, RequestId
- InputFormat (CSV/JSON/CCDA)
- Status (Processing/Completed/Failed)
- PatientsCount, ObservationsCount
- ProcessingTimeMs
```

---

## 🛠️ Development Standards (12 Rules)

### **Rule 1: Exception Handling**
✅ **All operations wrapped in try-catch**
```csharp
public async Task<Result<UserDto>> GetUserAsync(int userId)
{
    try
    {
        // Implementation
        return Result.Success(userDto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user {UserId}", userId);
        return Result.Failure("Failed to retrieve user");
    }
}
```

### **Rule 2: ADO.NET for Reads, EF Core for Writes**
✅ **Read operations use stored procedures with ADO.NET**
```csharp
// Read with ADO.NET
var users = await _dbReader.QueryAsync<User>("sp_GetAllUsers");

// Write with EF Core
_context.Users.Add(user);
await _context.SaveChangesAsync();
```

### **Rule 3: MagicStrings Concept**
✅ **All string literals centralized in MagicStrings.cs**
```csharp
public static class MagicStrings
{
    public static class StoredProcedures
    {
        public const string GetAllUsers = "sp_GetAllUsers";
    }
    
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found";
    }
}
```

### **Rule 4: Stored Procedures for ADO.NET**
✅ **All read operations use stored procedures**
```sql
CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SELECT * FROM Users WHERE IsActive = 1
    ORDER BY CreatedOn DESC  -- Recent first (Rule 9)
END
```

### **Rule 5: Separate AutoMapper Profiles**
✅ **One profile per entity**
```csharp
// UserMappingProfile.cs
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<CreateUserRequestDto, User>();
    }
}
```

### **Rule 6: Real-time Validation**
✅ **FluentValidation for all DTOs**
```csharp
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail);
    }
}
```

### **Rule 7: Recent Records First**
✅ **ORDER BY CreatedOn DESC in all stored procedures**

### **Rule 8: File-based Logging**
✅ **Serilog with separate log files**
```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.File("Logs/fhir-converter-{Date}.txt")
        .WriteTo.File("Logs/errors-{Date}.txt", 
            restrictedToMinimumLevel: LogEventLevel.Error);
});
```

### **Rule 9: No DTOs in Repository**
✅ **Repositories work only with entities**
```csharp
// ✅ Correct
public interface IUserRepository
{
    Task<User> GetByIdAsync(int userId);
}

// ❌ Wrong
public interface IUserRepository
{
    Task<UserDto> GetByIdAsync(int userId);
}
```

### **Rule 10: Fixed Entity Structure**
✅ **No breaking changes to entities post-migration**

---

## 🔄 Key Workflows

### **1. User Registration Flow**
```
1. User enters email, password, organization details
2. System generates 6-digit OTP
3. OTP sent to email (expires in 5 minutes)
4. User enters OTP
5. System validates OTP
6. Creates Organization (with encrypted data)
7. Creates User (linked to organization)
8. Returns success response
```

### **2. File Conversion Flow**
```
1. User uploads CSV/JSON/CCDA file
2. System validates file (size, format, extension)
3. Stores in temporary folder (expires in 1 hour)
4. Detects fields automatically
5. User maps fields to FHIR resources
6. System converts to FHIR R4 format
7. Applies LOINC codes (observations)
8. Applies UCUM units (quantities)
9. Generates FHIR Bundle (1 Patient + N Observations)
10. User downloads JSON bundle
11. System logs conversion (no PHI stored)
12. Auto-cleanup after 1 hour
```

### **3. Data Request Flow**
```
1. Hospital B searches patient by name/DOB
2. System hashes search terms
3. Queries PatientIdentifier table (hash matching)
4. Returns GlobalPatientId + available sources
5. Hospital B creates data request to Hospital A
6. Hospital A receives notification
7. Hospital A approves/rejects request
8. If approved, Hospital A uploads patient file
9. System converts to FHIR
10. Hospital B downloads FHIR bundle
11. Request marked as completed
12. Auto-expires after 7 days
```

---

## 🔐 Security Features

### **1. Data Protection**
- **No PHI Storage:** Only identifiers stored, medical data processed in-memory
- **Encryption at Rest:** AES-256 for all PII fields
- **Encryption in Transit:** HTTPS for all API calls
- **Auto-Expiration:** Files (1 hour), Requests (7 days), Patient maps (30 days)

### **2. Authentication & Authorization**
- **JWT Tokens:** 24-hour expiry, HTTP-only cookies
- **Role-Based Access:** Admin, Hospital, Clinic permissions
- **Password Security:** SHA-256 hashing (no plain text storage)

### **3. Audit Trail**
- **Comprehensive Logging:** All operations logged with Serilog
- **No PHI in Logs:** Only IDs and metadata logged
- **Separate Error Logs:** Critical errors in dedicated file

---

## 📈 Performance Optimizations

### **1. Database**
- **Indexed Hashes:** Fast patient searches on hashed fields
- **Composite Indexes:** Multi-field queries optimized
- **ADO.NET for Reads:** Faster than EF Core for read-heavy operations
- **Stored Procedures:** Pre-compiled, optimized queries

### **2. Memory Management**
- **Stateless Processing:** No memory leaks from stored data
- **Stream Processing:** Large files handled efficiently
- **Async Operations:** Non-blocking I/O throughout

### **3. Caching**
- **Terminology Mappings:** LOINC/UCUM cached in memory
- **Connection Pooling:** Efficient database connections

---

## 🧪 Testing Strategy

### **Unit Tests**
- Service layer logic
- FHIR conversion accuracy
- Encryption/decryption
- Validation rules

### **Integration Tests**
- API endpoints
- Database operations
- File upload/download
- FHIR bundle generation

---

## 📦 Technology Stack

### **Backend**
- .NET 10
- Entity Framework Core 10
- ADO.NET (for reads)
- SQL Server
- HL7.Fhir.R4 SDK
- AutoMapper
- Serilog
- JWT Authentication

### **Frontend**
- Angular 17
- TypeScript
- RxJS
- Angular Material (UI components)
- HttpClient (API calls)

---

## 🚀 API Endpoints

### **Authentication**
- `POST /api/auth/register-with-otp` - Send OTP for registration
- `POST /api/auth/verify-registration-otp` - Verify OTP and create user
- `POST /api/auth/login` - User login

### **Patients**
- `GET /api/patients/search` - Search patients by identifiers
- `POST /api/patients/identifiers` - Add patient identifier
- `GET /api/patients/{globalId}/sources` - Get data sources

### **Data Requests**
- `POST /api/data-requests` - Create request
- `GET /api/data-requests` - List requests by organization
- `PUT /api/data-requests/{id}/approve` - Approve/reject
- `GET /api/data-requests/{id}` - Get request details

### **Files**
- `POST /api/files/upload` - Upload file
- `GET /api/files/{id}/validate` - Validate file
- `DELETE /api/files/{id}` - Delete file

### **Conversion**
- `GET /api/convert/{fileId}/detect-fields` - Auto-detect fields
- `POST /api/convert/start` - Start conversion
- `GET /api/convert/status/{jobId}` - Check status
- `GET /api/convert/preview/{jobId}` - Preview FHIR bundle
- `GET /api/convert/download/{jobId}` - Download bundle
- `GET /api/convert/history` - Conversion history

### **Analytics (Admin)**
- `GET /api/admin/analytics/overview` - System overview
- `GET /api/admin/analytics/conversions` - Conversion stats
- `GET /api/admin/analytics/users` - User activity

---

## 💡 Key Talking Points for Evaluation

### **1. Why Clean Architecture?**
"I implemented Clean Architecture to ensure separation of concerns. The domain layer contains pure business logic with no external dependencies. The application layer orchestrates use cases, while infrastructure details like database access are isolated in the data layer. This makes the code testable, maintainable, and allows us to swap implementations without affecting business logic."

### **2. Why ADO.NET + EF Core Hybrid?**
"I used ADO.NET with stored procedures for all read operations because it's faster and more efficient for queries. EF Core is used only for write operations (Create, Update, Delete) where its change tracking and transaction management add value. This hybrid approach gives us the best of both worlds - performance for reads and convenience for writes."

### **3. How Does Encryption Work?**
"I implemented a dual storage strategy for patient data. Sensitive fields like names are encrypted using AES-256 for secure storage and retrieval. The same fields are also hashed using SHA-256 to enable searching without decryption. The EncryptionInterceptor automatically encrypts data on save and decrypts on read, so the application layer doesn't need to handle encryption logic."

### **4. FHIR Standards Compliance**
"The conversion engine follows HL7 FHIR R4 standards strictly. All observations use LOINC codes (e.g., Hemoglobin → 718-7), all units use UCUM (e.g., g/dL), and the bundle structure follows FHIR specifications with proper resource references. I created a TerminologyMappingService with 30+ common lab test mappings to ensure industry-standard codes."

### **5. Security & Compliance**
"Security is built into every layer. We use JWT tokens with HTTP-only cookies to prevent XSS attacks. All PII is encrypted at rest using AES-256. The system is stateless - medical data is never stored, only processed in-memory and immediately discarded. Auto-expiration policies ensure temporary data doesn't linger. All operations are logged without PHI for audit trails."

### **6. MagicStrings Pattern**
"I centralized all string literals in MagicStrings.cs to avoid hardcoded values scattered throughout the code. This makes the application maintainable - if we need to change an error message or stored procedure name, we change it in one place. It also prevents typos and makes refactoring safer."

### **7. Result Pattern**
"Instead of throwing exceptions for business logic failures, I use the Result pattern. Methods return `Result<T>` which can be Success or Failure. This makes error handling explicit and forces callers to handle both success and failure cases. It's more performant than exceptions and makes the code more predictable."

### **8. Why Separate Read Repositories?**
"I separated read and write repositories following CQRS principles. UserRepository handles writes with EF Core, while UserReadRepository handles reads with ADO.NET. This separation allows us to optimize each operation independently and makes the code intention clearer."

---

## 📝 Common Evaluation Questions & Answers

**Q: Why not use EF Core for everything?**  
A: EF Core is great for writes but ADO.NET with stored procedures is faster for reads. In healthcare, we have many read-heavy operations (searching patients, viewing requests), so performance matters.

**Q: How do you ensure HIPAA compliance?**  
A: We implement encryption at rest (AES-256), encryption in transit (HTTPS), no PHI storage (stateless processing), auto-expiration policies, comprehensive audit logging, and role-based access control.

**Q: What happens if conversion fails?**  
A: All operations are wrapped in try-catch blocks. If conversion fails, we log the error with details (no PHI), update the ConversionJob status to Failed, store the error message (encrypted), and return a user-friendly error response. The user can retry or reset the job.

**Q: How do you handle large files?**  
A: We use stream processing for large files, validate file size before upload (max 50MB), process data in batches, and use async operations throughout to avoid blocking threads.

**Q: Why Guid instead of int for IDs?**  
A: Guids are globally unique, which is important for distributed healthcare systems. They prevent ID collisions when merging data from multiple sources and don't expose sequential patterns that could be security risks.

**Q: How do you test FHIR conversion accuracy?**  
A: I have unit tests that validate LOINC code mappings, UCUM unit conversions, bundle structure, and resource references. Integration tests verify end-to-end conversion with sample CSV/JSON files against expected FHIR output.

---

## 🎯 Project Achievements

✅ **Clean Architecture** - Proper separation of concerns  
✅ **FHIR R4 Compliance** - Industry-standard healthcare interoperability  
✅ **Security First** - Encryption, hashing, no PHI storage  
✅ **Performance Optimized** - ADO.NET for reads, indexed searches  
✅ **Maintainable Code** - MagicStrings, Result pattern, comprehensive logging  
✅ **Real-world Ready** - Validation, error handling, auto-cleanup  
✅ **Scalable Design** - Stateless processing, async operations  
✅ **Full-stack Implementation** - .NET 10 backend + Angular 17 frontend  

---

**Document Version:** 1.0  
**Created:** January 2025  
**Purpose:** Technical evaluation and code walkthrough guide
