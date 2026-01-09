import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Cntent-Type': 'application/json'
    });
  }
  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getHeaders(),
      withCredentials: true
    });
  }
  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getHeaders(),
      withCredentials: true
    });
  }
  
  postFormData<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, formData, {
      withCredentials: true
    });
  }
  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getHeaders(),
      withCredentials: true
    });
  }
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getHeaders(),
      withCredentials: true
    });
  }
  
  getBlob(endpoint: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${endpoint}`, {
      responseType: 'blob',
      withCredentials: true
    });
  }
}
