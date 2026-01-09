import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RegisterWithOtpRequest {
  email: string;
  password: string;
  role: number;
  organizationName: string;
  mobileNumber: string;
}

export interface OtpResponse {
  email: string;
  otpToken: string;
  expiresAt: string;
  message: string;
}

export interface VerifyRegistrationOtpRequest {
  email: string;
  otp: string;
  otpToken: string;
  role: number;
}

export interface RegistrationResponse {
  userId: number;
  email: string;
  role: string;
  message: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  email: string;
  role: string;
  organizationName: string;
  organizationId?: string; // Add organizationId field
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl || 'https://localhost:7222/api';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  registerWithOtp(request: RegisterWithOtpRequest): Observable<OtpResponse> {
    return this.http.post<OtpResponse>(`${this.apiUrl}/auth/register-with-otp`, request);
  }

  verifyRegistrationOtp(request: VerifyRegistrationOtpRequest): Observable<RegistrationResponse> {
    return this.http.post<RegistrationResponse>(`${this.apiUrl}/auth/verify-registration-otp`, request);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request, { withCredentials: true });
  }

  setCurrentUser(user: any): void {
    this.currentUserSubject.next(user);
    localStorage.setItem('user', JSON.stringify(user));
  }

  getCurrentUser(): any {
    if (!this.currentUserSubject.value) {
      const storedUser = localStorage.getItem('user');
      if (storedUser) {
        this.currentUserSubject.next(JSON.parse(storedUser));
      }
    }
    return this.currentUserSubject.value;
  }

  logout(): void {
    this.currentUserSubject.next(null);
    // Clear any local storage if needed
    localStorage.removeItem('user');
  }

  isAuthenticated(): boolean {
    const user = this.getCurrentUser();
    return user !== null;
  }

  getUserRole(): string | null {
    const user = this.getCurrentUser();
    return user ? user.role : null;
  }
}