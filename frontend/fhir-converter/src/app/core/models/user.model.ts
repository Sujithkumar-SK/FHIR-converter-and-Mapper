export interface User {
  userId: number;
  email: string;
  role: UserRole;
  organizationId: string;
  isActive: boolean;
  lastLogin?: Date;
}

export enum UserRole {
  Admin = 1,
  Hospital = 2,
  Clinic = 3
}