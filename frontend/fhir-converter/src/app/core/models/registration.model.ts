import { UserRole } from "./user.model";

export interface RegisterWithOtpRequest {
  email: string;
  password: string;
  role: UserRole;
  organizationName: string;
  organizationId: string;
}

export interface OtpResponse {
  email: string;
  otpToken: string;
  expiresAt: Date;
  message: string;
}

export interface VerifyRegistrationOtpRequest {
  email: string;
  otp: string;
  otpToken: string;
  role: UserRole;
}

export interface RegistrationResponse {
  userId: number;
  email: string;
  role: string;
  message: string;
}
