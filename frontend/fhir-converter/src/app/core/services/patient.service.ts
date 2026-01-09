import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreatePatientRequest {
  localPatientId: string;
  firstName?: string;
  lastName: string;
  dateOfBirth?: Date;
  sourceOrganizationId: string;
}

export interface PatientSearchRequest {
  searchTerm: string;
  dateOfBirth?: Date;
}

export interface PatientResponse {
  id: string;
  globalPatientId: string;
  sourceOrganizationId: string;
  localPatientId: string;
  firstName?: string;
  lastName?: string;
  dateOfBirth?: Date;
  createdOn: Date;
  organizationName?: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class PatientService {
  private apiUrl = environment.apiUrl || 'https://localhost:7222/api';

  constructor(private http: HttpClient) {}

  createPatient(request: CreatePatientRequest): Observable<PatientResponse> {
    return this.http.post<PatientResponse>(`${this.apiUrl}/patients/identifiers`, request, { withCredentials: true });
  }

  getAllPatients(): Observable<PatientResponse[]> {
    return this.http.get<PatientResponse[]>(`${this.apiUrl}/patients`, { withCredentials: true });
  }

  getPatientByGlobalId(globalId: string): Observable<PatientResponse> {
    return this.http.get<PatientResponse>(`${this.apiUrl}/patients/${globalId}`, { withCredentials: true });
  }

  getPatientsByOrganization(organizationId: string): Observable<PatientResponse[]> {
    return this.http.get<PatientResponse[]>(`${this.apiUrl}/patients/organization/${organizationId}`, { withCredentials: true });
  }
}