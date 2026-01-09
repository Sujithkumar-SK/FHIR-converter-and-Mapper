namespace Kanini.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Hospital = 2,
    Clinic = 3
}

public enum OrganizationType
{
    Hospital = 1,
    Clinic = 2
}


public enum DataRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    DataReady = 4,
    Completed = 5,
    Expired = 6
}

public enum InputFormat
{
    CSV = 1,
    JSON = 2,
    CCDA = 3
}

public enum ConversionStatus
{
    Processing = 1,
    Completed = 2,
    Failed = 3
}
