import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemOverview {
  totalUsers: number;
  activeUsers: number;
  totalOrganizations: number;
  totalConversions: number;
  totalDataRequests: number;
  pendingDataRequests: number;
  totalPatients: number;
  lastUpdated: Date;
}

export interface ConversionStatistics {
  totalConversions: number;
  successfulConversions: number;
  failedConversions: number;
  processingConversions: number;
  successRate: number;
  averageProcessingTimeMs: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getSystemOverview(): Observable<SystemOverview> {
    return this.http.get<SystemOverview>(`${this.apiUrl}/overview`, { withCredentials: true });
  }

  getConversionStatistics(startDate?: Date, endDate?: Date): Observable<ConversionStatistics> {
    let params = '';
    if (startDate && endDate) {
      params = `?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
    }
    return this.http.get<ConversionStatistics>(`${this.apiUrl}/conversions/statistics${params}`, { withCredentials: true });
  }

  getUserActivityStats(startDate?: Date, endDate?: Date): Observable<any> {
    let params = '';
    if (startDate && endDate) {
      params = `?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
    }
    return this.http.get(`${this.apiUrl}/users/activity${params}`, { withCredentials: true });
  }

  getDataRequestStats(startDate?: Date, endDate?: Date): Observable<any> {
    let params = '';
    if (startDate && endDate) {
      params = `?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`;
    }
    return this.http.get(`${this.apiUrl}/data-requests/statistics${params}`, { withCredentials: true });
  }

  getOrganizationStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/organizations/statistics`, { withCredentials: true });
  }
}