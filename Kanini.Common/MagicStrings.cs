namespace Kanini.Common.Constants;

public static class MagicStrings
{
    public static class StoredProcedures
    {
        public const string CheckUserExistsByEmail = "sp_CheckUserExistsByEmail";
        public const string GetUserByEmail = "sp_GetUserByEmail";
        public const string GetUserById = "sp_GetUserById";
        public const string GetAllUsers = "sp_GetAllUsers";
        
        // Patient stored procedures
        public const string GetAllPatients = "sp_GetAllPatients";
        public const string GetPatientByGlobalId = "sp_GetPatientByGlobalId";
        public const string GetPatientsByOrganization = "sp_GetPatientsByOrganization";
        
        // Data Request stored procedures
        public const string GetDataRequestsByOrganization = "sp_GetDataRequestsByOrganization";
        public const string GetDataRequestById = "sp_GetDataRequestById";
        public const string GetPendingDataRequests = "sp_GetPendingDataRequests";
        public const string CheckDataRequestExists = "sp_CheckDataRequestExists";
        
        // Analytics stored procedures
        public const string GetSystemOverview = "sp_GetSystemOverview";
        public const string GetConversionStatistics = "sp_GetConversionStatistics";
        public const string GetUserActivityStats = "sp_GetUserActivityStats";
        public const string GetDataRequestStats = "sp_GetDataRequestStats";
        public const string GetOrganizationStats = "sp_GetOrganizationStats";
        public const string GetRecentActivity = "sp_GetRecentActivity";
        public const string GetConversionTrends = "sp_GetConversionTrends";
    }

    public static class FileValidation
    {
        public const int MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB
        public const int FileExpirationHours = 1;
        public static readonly string[] AllowedExtensions = { ".csv", ".json", ".xml" };
        public static readonly string[] AllowedMimeTypes = { "text/csv", "application/json", "text/xml", "application/xml" };
        public const string TempFolderName = "TempUploads";
    }

    public static class ErrorMessages
    {
        public const string EmailAlreadyExists = "Email address is already registered";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string RegistrationFailed = "Registration failed. Please try again";
        public const string InvalidOrExpiredOtp = "Invalid or expired OTP";
        public const string OtpVerificationFailed = "OTP verification failed";
        public const string InternalServerError = "An internal server error occurred";
        public const string AccountInactive = "Account is inactive";
        public const string LoginFailed = "Login failed. Please try again";
        public const string EmailRequired = "Email is required";
        public const string InvalidEmail = "Invalid email format";
        public const string OrganizationRequired = "Organization is required";
        public const string OrganizationNotFound = "Organization not found";
        
        // Patient error messages
        public const string PatientNotFound = "Patient not found";
        public const string PatientAlreadyExists = "Patient already exists in this organization";
        public const string InvalidPatientData = "Invalid patient data provided";
        public const string PatientCreationFailed = "Failed to create patient identifier";
        
        // Data Request error messages
        public const string DataRequestNotFound = "Data request not found";
        public const string DataRequestAlreadyExists = "Data request already exists for this patient";
        public const string DataRequestExpired = "Data request has expired";
        public const string UnauthorizedDataRequest = "Not authorized to access this data request";
        public const string InvalidDataRequestStatus = "Invalid data request status";
        public const string DataRequestCreationFailed = "Failed to create data request";
        public const string DataRequestApprovalFailed = "Failed to approve data request";
        
        // File Upload error messages
        public const string FileNotFound = "File not found";
        public const string FileTooLarge = "File size exceeds maximum limit";
        public const string InvalidFileFormat = "Invalid file format";
        public const string FileUploadFailed = "File upload failed";
        public const string FileValidationFailed = "File validation failed";
        public const string UnsupportedFileType = "Unsupported file type";
        public const string FileExpired = "File has expired";
        public const string FileRequired = "File is required";
        
        // Conversion error messages
        public const string ConversionJobNotFound = "Conversion job not found";
        public const string ConversionFailed = "File conversion failed";
        public const string ConversionInProgress = "Conversion is already in progress";
        public const string InvalidFieldMapping = "Invalid field mapping provided";
        public const string RequiredFieldMissing = "Required field mapping is missing";
        public const string FhirBundleGenerationFailed = "FHIR bundle generation failed";
        public const string ConversionJobExpired = "Conversion job has expired";
        public const string UnsupportedConversionFormat = "Unsupported conversion format";
        
        // Terminology error messages
        public const string LoincMappingNotFound = "LOINC mapping not found for test name";
        public const string UcumMappingNotFound = "UCUM mapping not found for unit";
        public const string InvalidTerminologyCode = "Invalid terminology code provided";
        public const string TerminologyServiceError = "Terminology service error occurred";
        
        // Analytics error messages
        public const string AnalyticsDataNotFound = "Analytics data not found";
        public const string InvalidDateRange = "Invalid date range provided";
        public const string UnauthorizedAnalyticsAccess = "Not authorized to access analytics data";
    }

    public static class SuccessMessages
    {
        public const string OtpSent = "OTP sent successfully to your email";
        public const string RegistrationCompleted = "Registration completed successfully";
        public const string LoginSuccessful = "Login successful";
        
        // Patient success messages
        public const string PatientCreated = "Patient identifier created successfully";
        public const string PatientUpdated = "Patient identifier updated successfully";
        
        // Data Request success messages
        public const string DataRequestCreated = "Data request created successfully";
        public const string DataRequestApproved = "Data request approved successfully";
        public const string DataRequestRejected = "Data request rejected successfully";
        
        // File Upload success messages
        public const string FileUploaded = "File uploaded successfully";
        public const string FileValidated = "File validated successfully";
        public const string FileDeleted = "File deleted successfully";
        
        // Conversion success messages
        public const string ConversionStarted = "File conversion started successfully";
        public const string ConversionCompleted = "File conversion completed successfully";
        public const string FhirBundleGenerated = "FHIR bundle generated successfully";
        public const string ConversionJobCreated = "Conversion job created successfully";
    }

    public static class LogMessages
    {
        public const string RegistrationOtpStarted = "Registration OTP process started for email: {Email}";
        public const string RegistrationOtpCompleted = "Registration OTP sent successfully for email: {Email}";
        public const string RegistrationOtpFailed = "Registration OTP failed for email: {Email}, Error: {Error}";
        public const string RegistrationVerificationStarted = "Registration verification started for email: {Email}";
        public const string RegistrationVerificationCompleted = "Registration verification completed for email: {Email}, UserId: {UserId}";
        public const string RegistrationVerificationFailed = "Registration verification failed for email: {Email}, Error: {Error}";
        public const string LoginAttemptStarted = "Login attempt started for email: {Email}";
        public const string LoginAttemptCompleted = "Login completed successfully for email: {Email}, UserId: {UserId}";
        public const string LoginAttemptFailed = "Login failed for email: {Email}, Error: {Error}";
        public const string EncryptionFailed = "Encryption failed for data length: {Length}";
        public const string DecryptionFailed = "Decryption failed for data length: {Length}";
        public const string HashingFailed = "Hashing failed for data length: {Length}";
        
        // Patient log messages
        public const string PatientSearchStarted = "Patient search started with criteria: FirstName={FirstName}, LastName={LastName}";
        public const string PatientSearchCompleted = "Patient search completed, found {Count} results";
        public const string PatientCreationStarted = "Patient creation started for GlobalId: {GlobalPatientId}";
        public const string PatientCreationCompleted = "Patient created successfully with ID: {PatientId}";
        public const string PatientCreationFailed = "Patient creation failed for GlobalId: {GlobalPatientId}, Error: {Error}";
        
        // Data Request log messages
        public const string DataRequestCreationStarted = "Data request creation started for GlobalPatientId: {GlobalPatientId}";
        public const string DataRequestCreationCompleted = "Data request created successfully with ID: {RequestId}";
        public const string DataRequestCreationFailed = "Data request creation failed for GlobalPatientId: {GlobalPatientId}, Error: {Error}";
        public const string DataRequestApprovalStarted = "Data request approval started for RequestId: {RequestId}";
        public const string DataRequestApprovalCompleted = "Data request approved successfully for RequestId: {RequestId}";
        public const string DataRequestApprovalFailed = "Data request approval failed for RequestId: {RequestId}, Error: {Error}";
        
        // File Upload log messages
        public const string FileUploadStarted = "File upload started: {FileName}, Size: {FileSize}";
        public const string FileUploadCompleted = "File upload completed: {FileId}, Path: {FilePath}";
        public const string FileUploadFailed = "File upload failed: {FileName}, Error: {Error}";
        public const string FileValidationStarted = "File validation started for FileId: {FileId}";
        public const string FileValidationCompleted = "File validation completed for FileId: {FileId}, Status: {Status}";
        public const string FileValidationFailed = "File validation failed for FileId: {FileId}, Error: {Error}";
        public const string FileCleanupStarted = "File cleanup started for expired files";
        public const string FileCleanupCompleted = "File cleanup completed, removed {Count} files";
        
        // Conversion log messages
        public const string ConversionStarted = "Conversion started for FileId: {FileId}, JobId: {JobId}";
        public const string ConversionCompleted = "Conversion completed for JobId: {JobId}, Patients: {PatientCount}, Observations: {ObservationCount}";
        public const string ConversionFailed = "Conversion failed for JobId: {JobId}, Error: {Error}";
        public const string FieldMappingDetected = "Field mapping detected for FileId: {FileId}, Detected: {DetectedCount} fields";
        public const string FhirBundleGenerated = "FHIR bundle generated for JobId: {JobId}, Size: {BundleSize} bytes";
        public const string ConversionProgressUpdate = "Conversion progress for JobId: {JobId}: {Progress}%";
        public const string ConversionJobCleanup = "Conversion job cleanup started for expired jobs";
        public const string ConversionJobCleanupCompleted = "Conversion job cleanup completed, removed {Count} jobs";
        
        // Analytics log messages
        public const string AnalyticsRequestStarted = "Analytics request started for type: {AnalyticsType}";
        public const string AnalyticsRequestCompleted = "Analytics request completed for type: {AnalyticsType}, Records: {RecordCount}";
        public const string AnalyticsRequestFailed = "Analytics request failed for type: {AnalyticsType}, Error: {Error}";
        
        // Terminology log messages
        public const string LoincMappingFound = "LOINC mapping found for test '{TestName}': {LoincCode}";
        public const string LoincMappingNotFound = "No LOINC mapping found for test '{TestName}', using fallback";
        public const string UcumMappingFound = "UCUM mapping found for unit '{Unit}': {UcumCode}";
        public const string UcumMappingNotFound = "No UCUM mapping found for unit '{Unit}', using as-is";
        public const string TerminologyMappingStarted = "Terminology mapping started for test: {TestName}, unit: {Unit}";
        public const string TerminologyMappingCompleted = "Terminology mapping completed - LOINC: {LoincCode}, UCUM: {UcumCode}";
    }

    public static class EmailTemplates
    {
        public static string GetVerificationCodeEmail(string userName, string otp)
        {
            return $@"
                <html>
                <body>
                    <h2>FHIR Converter - Email Verification</h2>
                    <p>Hello {userName},</p>
                    <p>Your verification code is: <strong style='font-size: 24px; color: #007bff;'>{otp}</strong></p>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>FHIR Converter Team</p>
                </body>
                </html>";
        }
    }
}