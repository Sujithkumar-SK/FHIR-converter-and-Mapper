import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateDataRequestDto, ApproveDataRequestDto, DataRequestResponse } from '../models/data-request.model';

@Injectable({
  providedIn: 'root'
})
export class DataRequestService {
  private apiUrl = environment.apiUrl || 'https://localhost:7222/api';

  constructor(private http: HttpClient) {}

  createRequest(request: CreateDataRequestDto): Observable<DataRequestResponse> {
    return this.http.post<DataRequestResponse>(`${this.apiUrl}/data-requests`, request, { withCredentials: true });
  }

  getRequestsByOrganization(isRequesting: boolean = true): Observable<DataRequestResponse[]> {
    return this.http.get<DataRequestResponse[]>(`${this.apiUrl}/data-requests?isRequesting=${isRequesting}`, { withCredentials: true });
  }

  getRequestById(requestId: string): Observable<DataRequestResponse> {
    return this.http.get<DataRequestResponse>(`${this.apiUrl}/data-requests/${requestId}`, { withCredentials: true });
  }

  approveRequest(requestId: string, approval: ApproveDataRequestDto): Observable<DataRequestResponse> {
    return this.http.put<DataRequestResponse>(`${this.apiUrl}/data-requests/${requestId}/approve`, approval, { withCredentials: true });
  }

  getPendingRequests(): Observable<DataRequestResponse[]> {
    return this.http.get<DataRequestResponse[]>(`${this.apiUrl}/data-requests/pending`, { withCredentials: true });
  }
}