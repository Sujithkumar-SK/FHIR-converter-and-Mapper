# FHIR Data Converter & Mapper - Complete Project Documentation

## 📋 Project Overview

**Project Name:** FHIR Data Converter & Mapper  
**Purpose:** Convert medical data from various formats (CSV, JSON, CCDA) to FHIR R4 standard for healthcare interoperability  
**Architecture:** Clean Architecture with .NET 10, Entity Framework Core, SQL Server  

## 🎯 Business Requirements

### Core Functionality
- **Multi-format Support:** Convert CSV, JSON, CCDA to FHIR R4
- **User Management:** 3 role-based logins (Admin, Hospital, Clinic)
- **Data Request Workflow:** Hospital-to-hospital data sharing requests
- **Security:** Encrypted PII storage with searchable hashes
- **Stateless Processing:** No permanent PHI storage, temporary conversion only

### User Flow
1. **Patient Treatment:** Patient treated at Hospital A, tests at Clinic
2. **Transfer Request:** Patient moves to Hospital B
3. **Data Request:** Hospital B requests patient data from Hospital A via app
4. **Data Sharing:** Hospital A uploads patient data (CSV/JSON/CCDA)
5. **Conversion:** App converts data to FHIR format in real-time
6. **Delivery:** Hospital B receives FHIR bundle
7. **Cleanup:** All temporary data auto-deleted

## 🏗️ System Architecture

### Layer Structure
```
┌─────────────────────────────────────────────────────────────┐
│                    Kanini.Api (Presentation)               │
│  Controllers, Program.cs, Swagger, Authentication          │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                Kanini.Application (Business)               │
│     Services, DTOs, AutoMapper, Validation, Logic         │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                  Kanini.Data (Data Access)                 │
│   Repositories, DbContext, Configurations, Migrations     │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                 Kanini.Domain (Core Entities)              │
│           Entities, Enums, Business Models                 │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                Kanini.Common (Shared Utilities)            │
│      Attributes, Services, Validators, Constants           │
└─────────────────────────────────────────────────────────────┘
```

## 📊 Database Design

### Core Entities

#### 1. User Entity
```csharp
public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; } // Admin, Hospital, Clinic
    [Encrypted] public string OrganizationName { get; set; }
    public string OrganizationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
}
```

#### 2. Organization Entity
```csharp
public class Organization : BaseEntity
{
    public string OrganizationId { get; set; } // Primary Key
    [Encrypted] public string Name { get; set; }
    public OrganizationType Type { get; set; } // Hospital, Clinic
    [Encrypted] public string ContactEmail { get; set; }
    [Encrypted] public string ContactPhone { get; set; }
    public bool IsActive { get; set; }
}
```

#### 3. PatientIdentifier Entity (Dual Storage Strategy)
```csharp
public class PatientIdentifier : BaseEntity
{
    public int Id { get; set; }
    public Guid GlobalPatientId { get; set; } // Cross-hospital identifier
    public string SourceOrganization { get; set; }
    
    // Encrypted fields (for data retrieval)
    [Encrypted] public string LocalPatientId { get; set; }
    [Encrypted] public string? NationalId { get; set; }
    [Encrypted] public string? LastName { get; set; }
    [Encrypted] public string? FirstName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Hashed fields (for searching)
    public string? NationalIdHash { get; set; }
    public string? LastNameHash { get; set; }
    public string? FirstNameHash { get; set; }
    public string? DateOfBirthHash { get; set; }
    
    public DateTime ExpiresAt { get; set; } // Auto-cleanup after 30 days
}
```

#### 4. DataRequest Entity
```csharp
public class DataRequest : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid GlobalPatientId { get; set; }
    public int RequestingUserId { get; set; }
    public string RequestingOrganization { get; set; }
    public string SourceOrganization { get; set; }
    public DataRequestStatus Status { get; set; }
    [Encrypted] public string? Notes { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime ExpiresAt { get; set; } // Auto-cleanup after 7 days
}
```

#### 5. ConversionJob Entity
```csharp
public class ConversionJob : BaseEntity
{
    public Guid JobId { get; set; }
    public int UserId { get; set; }
    public Guid? RequestId { get; set; }
    public InputFormat InputFormat { get; set; } // CSV, JSON, CCDA
    public ConversionStatus Status { get; set; }
    [Encrypted] public string? ErrorMessage { get; set; }
    public int PatientsCount { get; set; }
    public int ObservationsCount { get; set; }
    public DateTime? CompletedAt { get; set; }
    [Encrypted] public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public long? ProcessingTimeMs { get; set; }
}
```

### Enums
```csharp
public enum UserRole { Admin = 1, Hospital = 2, Clinic = 3 }
public enum OrganizationType { Hospital = 1, Clinic = 2 }
public enum DataRequestStatus { Pending = 1, Approved = 2, Rejected = 3, Completed = 4, Expired = 5 }
public enum InputFormat { CSV = 1, JSON = 2, CCDA = 3 }
public enum ConversionStatus { Processing = 1, Completed = 2, Failed = 3 }
```

## 🔐 Security Implementation

### Encryption Strategy
- **AES-256 Encryption:** For PII fields (names, IDs, contact info)
- **SHA-256 Hashing:** For searchable fields (patient matching)
- **Dual Storage:** Both encrypted (retrieval) and hashed (searching) versions

### Security Features
- **No PHI Storage:** Only identifiers for matching, no medical records
- **Auto-Expiration:** Patient maps (30 days), requests (7 days)
- **Audit Trails:** All conversions logged without PHI
- **Role-Based Access:** Admin, Hospital, Clinic permissions
- **Encrypted Attributes:** Custom `[Encrypted]` attribute for sensitive fields

## 🔄 Data Flow Architecture

### 1. Patient Identification Flow
```
Hospital B Search → Hash Search Terms → Query PatientIdentifier → 
Return Global GUID → Show Available Sources
```

### 2. Data Request Flow
```
Hospital B Request → Create DataRequest → Notify Hospital A → 
Hospital A Approval → Status Update → Ready for Upload
```

### 3. Conversion Flow
```
File Upload → Validate Format → Convert to FHIR (Memory) → 
Return Bundle → Log Conversion → Auto-Delete Temp Data
```

## 🛠️ Development Standards

## 🛠️ Development Standards & Rules

### Mandatory Development Rules

#### 1. Exception Handling
- **Rule:** All operations must be wrapped in try-catch blocks
- **Implementation:** Every service method, controller action, and repository operation
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

#### 2. Data Access Pattern
- **Rule:** ADO.NET for ALL read operations, EF Core for write operations only
- **Read Operations:** Use stored procedures with ADO.NET
- **Write Operations:** Use EF Core (Insert, Update, Delete)
```csharp
// ✅ Read with ADO.NET
public async Task<IEnumerable<User>> GetUsersAsync()
{
    return await _dbReader.ExecuteStoredProcedureAsync<User>("sp_GetAllUsers");
}

// ✅ Write with EF Core
public async Task<User> CreateUserAsync(User user)
{
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return user;
}
```

#### 3. Magic String Concept
- **Rule:** All string literals must be centralized as constants
- **Implementation:** Create MagicStrings class for all hardcoded values
```csharp
public static class MagicStrings
{
    public static class StoredProcedures
    {
        public const string GetAllUsers = "sp_GetAllUsers";
        public const string GetUserById = "sp_GetUserById";
        public const string GetPatientsByOrganization = "sp_GetPatientsByOrganization";
    }
    
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found";
        public const string InvalidCredentials = "Invalid email or password";
        public const string ConversionFailed = "File conversion failed";
    }
    
    public static class LogMessages
    {
        public const string UserLoginSuccess = "User {Email} logged in successfully";
        public const string ConversionStarted = "Conversion started for file {FileName}";
        public const string DataRequestCreated = "Data request {RequestId} created";
    }
}
```

#### 4. Stored Procedures Requirement
- **Rule:** All ADO.NET operations must use stored procedures
- **No inline SQL allowed for read operations**
```csharp
// ✅ Correct - Using stored procedure
var users = await _dbReader.ExecuteStoredProcedureAsync<User>(
    MagicStrings.StoredProcedures.GetUsersByRole,
    new { Role = UserRole.Hospital }
);

// ❌ Wrong - Inline SQL not allowed
var users = await _dbReader.QueryAsync<User>(
    "SELECT * FROM Users WHERE Role = @Role",
    new { Role = UserRole.Hospital }
);
```

#### 5. AutoMapper Separation
- **Rule:** AutoMapper profiles must be in separate files
- **Structure:** One profile per entity with clear naming
```csharp
// UserMappingProfile.cs
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<CreateUserRequestDto, User>();
        CreateMap<UpdateUserRequestDto, User>();
    }
}

// ConversionJobMappingProfile.cs
public class ConversionJobMappingProfile : Profile
{
    public ConversionJobMappingProfile()
    {
        CreateMap<ConversionJob, ConversionJobResponseDto>();
        CreateMap<CreateConversionJobRequestDto, ConversionJob>();
    }
}
```

#### 6. Real-time Validation
- **Rule:** All API endpoints must have comprehensive validation
- **Implementation:** FluentValidation with real-world scenarios
```csharp
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MagicStrings.ErrorMessages.EmailRequired)
            .EmailAddress().WithMessage(MagicStrings.ErrorMessages.InvalidEmail)
            .MustAsync(BeUniqueEmail).WithMessage(MagicStrings.ErrorMessages.EmailExists);
            
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage(MagicStrings.ErrorMessages.OrganizationRequired)
            .MustAsync(OrganizationExists).WithMessage(MagicStrings.ErrorMessages.OrganizationNotFound);
    }
}
```

#### 7. Recent Records First
- **Rule:** All fetch operations must return newest records first
- **Implementation:** ORDER BY CreatedOn DESC in all stored procedures
```sql
-- sp_GetAllUsers.sql
CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SELECT UserId, Email, Role, OrganizationName, CreatedOn
    FROM Users
    WHERE IsActive = 1
    ORDER BY CreatedOn DESC -- ✅ Recent first
END
```

#### 8. File-based Logging
- **Rule:** All logs must be saved to separate files
- **Implementation:** Serilog with file sinks and structured logging
```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.File("Logs/fhir-converter-{Date}.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("Logs/errors-{Date}.txt",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day);
});
```

#### 9. No DTOs in Repository
- **Rule:** Repository layer must work only with domain entities
- **Implementation:** DTOs only in Application/API layers
```csharp
// ✅ Correct - Repository uses entities
public interface IUserRepository
{
    Task<User> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
}

// ❌ Wrong - Repository using DTOs
public interface IUserRepository
{
    Task<UserDto> GetByIdAsync(int userId); // Don't use DTOs here
}
```

#### 10. Fixed Entity Structure
- **Rule:** Entity structure must remain stable post-migration
- **Implementation:** No breaking changes to existing entities
```csharp
// ✅ Allowed - Adding new optional properties
public class User : BaseEntity
{
    // Existing properties (DO NOT MODIFY)
    public int UserId { get; set; }
    public string Email { get; set; }
    
    // ✅ New optional properties allowed
    public DateTime? LastPasswordChange { get; set; }
    public string? ProfilePicture { get; set; }
}

// ❌ Not allowed - Changing existing properties
public class User : BaseEntity
{
    public long UserId { get; set; } // ❌ Changed int to long
    public string EmailAddress { get; set; } // ❌ Renamed Email
}
```

### Implementation Architecture

#### Repository Pattern with Rules
```csharp
// IUserRepository.cs - Only entities
public interface IUserRepository
{
    Task<User> CreateAsync(User user); // EF Core
    Task<User> UpdateAsync(User user); // EF Core
    Task DeleteAsync(int userId); // EF Core
}

// IUserReadRepository.cs - ADO.NET reads
public interface IUserReadRepository
{
    Task<IEnumerable<User>> GetAllAsync(); // ADO.NET + SP
    Task<User> GetByIdAsync(int userId); // ADO.NET + SP
    Task<User> GetByEmailAsync(string email); // ADO.NET + SP
}
```

#### Service Layer with Exception Handling
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public async Task<Result<UserResponseDto>> GetUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.GetUserStarted, userId);
            
            var user = await _userReadRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(MagicStrings.ErrorMessages.UserNotFound);
            }
            
            var userDto = _mapper.Map<UserResponseDto>(user);
            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.GetUserFailed, userId);
            return Result.Failure(MagicStrings.ErrorMessages.InternalServerError);
        }
    }
}
```

### Folder Structure Following Rules
```
Kanini.Application/
├── Services/           # Business logic with try-catch
├── DTOs/              # Data transfer objects
├── AutoMapper/        # Separate mapping profiles
├── Validators/        # Real-time validation rules
└── Interfaces/        # Service contracts

Kanini.Data/
├── Repositories/      # EF Core write operations
├── ReadRepositories/  # ADO.NET read operations
├── StoredProcedures/  # SQL stored procedures
└── Infrastructure/    # ADO.NET infrastructure

Kanini.Common/
├── Constants/         # MagicStrings class
├── Logging/          # File logging configuration
└── Results/          # Result pattern for error handling
```

These rules ensure consistent, maintainable, and robust code across the entire FHIR Converter project.

### Project Structure
```
Kanini.FhirConverter/
├── Kanini.Api/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── Kanini.Application/
│   ├── Services/
│   ├── DTOs/
│   ├── AutoMapper/
│   └── Validators/
├── Kanini.Data/
│   ├── DatabaseContext/
│   ├── Configurations/
│   ├── Repositories/
│   ├── StoredProcedures/
│   └── Migrations/
├── Kanini.Domain/
│   ├── Entities/
│   └── Enums/
└── Kanini.Common/
    ├── Attributes/
    ├── Services/
    └── Constants/
```

## 📋 API Endpoints Design

### Authentication Endpoints
- `POST /api/auth/login` - User login (Admin/Hospital/Clinic)
- `POST /api/auth/register` - Organization registration
- `POST /api/auth/logout` - User logout

### Patient Management
- `GET /api/patients/search` - Search patients by identifiers
- `POST /api/patients/identifiers` - Add patient identifier mapping
- `GET /api/patients/{globalId}/sources` - Get available data sources

### Data Request Management
- `POST /api/data-requests` - Create data sharing request
- `GET /api/data-requests` - List requests (by organization)
- `PUT /api/data-requests/{id}/approve` - Approve/reject request
- `GET /api/data-requests/{id}/status` - Check request status

### Conversion Endpoints
- `POST /api/convert/csv-to-fhir` - Convert CSV to FHIR
- `POST /api/convert/json-to-fhir` - Convert JSON to FHIR
- `POST /api/convert/ccda-to-fhir` - Convert CCDA to FHIR
- `GET /api/conversions/history` - Conversion history (no PHI)

### Admin Endpoints
- `GET /api/admin/organizations` - Manage organizations
- `GET /api/admin/users` - User management
- `GET /api/admin/analytics` - System analytics

## 🔧 Technology Stack

### Backend
- **.NET 10:** Latest framework version
- **Entity Framework Core 10:** ORM for data access
- **SQL Server:** Primary database
- **ADO.NET:** Read operations with stored procedures
- **AutoMapper:** Object-to-object mapping
- **Swagger/OpenAPI:** API documentation

### Security
- **AES-256:** Data encryption
- **SHA-256:** Hashing for searches
- **JWT:** Authentication tokens
- **HTTPS:** Secure communication

### Development Tools
- **Visual Studio 2024:** IDE
- **Entity Framework Tools:** Migrations
- **Swagger UI:** API testing
- **SQL Server Management Studio:** Database management

## 📈 Performance Considerations

### Database Optimization
- **Indexed Hashes:** Fast patient searches
- **Composite Indexes:** Multi-field queries
- **Partitioning:** Large table management
- **Connection Pooling:** Efficient database connections

### Memory Management
- **Stateless Processing:** No memory leaks from stored data
- **Stream Processing:** Large file handling
- **Garbage Collection:** Automatic cleanup of temporary objects
- **Async Operations:** Non-blocking I/O operations

## 🚀 Deployment Strategy

### Environment Configuration
- **Development:** LocalDB, file logging
- **Staging:** Azure SQL, Application Insights
- **Production:** Azure SQL with encryption, comprehensive monitoring

### CI/CD Pipeline
1. **Build:** Compile and test
2. **Database Migration:** Automated schema updates
3. **Security Scan:** Vulnerability assessment
4. **Deploy:** Blue-green deployment strategy
5. **Monitoring:** Health checks and alerts

## 📊 Monitoring & Analytics

### Key Metrics
- **Conversion Success Rate:** Track failed conversions
- **Processing Time:** Monitor performance
- **User Activity:** Login patterns and usage
- **Error Rates:** System reliability metrics
- **Data Volume:** File sizes and record counts

### Logging Strategy
- **Application Logs:** Business logic events
- **Security Logs:** Authentication and authorization
- **Performance Logs:** Response times and resource usage
- **Error Logs:** Exception details and stack traces

## 🔮 Future Enhancements

### Phase 2 Features
- **Real-time Notifications:** WebSocket integration
- **Bulk Processing:** Multiple file conversion
- **Advanced Analytics:** Usage dashboards
- **Mobile App:** iOS/Android applications

### Integration Opportunities
- **EHR Systems:** Direct Epic/Cerner integration
- **Cloud Storage:** Azure Blob/AWS S3 support
- **AI/ML:** Automated data quality validation
- **Blockchain:** Immutable audit trails

## 📝 Compliance & Governance

### HIPAA Compliance
- **Data Encryption:** At rest and in transit
- **Access Controls:** Role-based permissions
- **Audit Trails:** Complete activity logging
- **Data Retention:** Automated cleanup policies
- **Breach Notification:** Incident response procedures

### Quality Assurance
- **Unit Testing:** 80%+ code coverage
- **Integration Testing:** End-to-end scenarios
- **Security Testing:** Penetration testing
- **Performance Testing:** Load and stress testing
- **User Acceptance Testing:** Stakeholder validation

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**Next Review:** March 2025  

This documentation serves as the complete reference for the FHIR Data Converter & Mapper project, covering all aspects from business requirements to technical implementation details.