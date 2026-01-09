import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  FieldDetectionResponse, 
  StartConversionRequest, 
  ConversionStatus, 
  FhirBundlePreview 
} from '../models/conversion.model';

@Injectable({
  providedIn: 'root'
})
export class ConversionService {
  constructor(private apiService: ApiService) { }

  detectFields(fileId: string): Observable<FieldDetectionResponse> {
    return this.apiService.get<FieldDetectionResponse>(`convert/${fileId}/detect-fields`);
  }

  getAvailableFhirFields(): Observable<string[]> {
    return this.apiService.get<string[]>('convert/fhir-fields');
  }

  startConversion(request: StartConversionRequest): Observable<ConversionStatus> {
    return this.apiService.post<ConversionStatus>('convert/start', request);
  }

  getConversionStatus(jobId: string): Observable<ConversionStatus> {
    return this.apiService.get<ConversionStatus>(`convert/status/${jobId}`);
  }

  getFhirPreview(jobId: string): Observable<FhirBundlePreview> {
    return this.apiService.get<FhirBundlePreview>(`convert/preview/${jobId}`);
  }

  downloadFhirBundle(jobId: string): Observable<Blob> {
    return this.apiService.getBlob(`convert/download/${jobId}`);
  }

  getConversionHistory(): Observable<ConversionStatus[]> {
    return this.apiService.get<ConversionStatus[]>('convert/history');
  }

  getConversionByRequestId(requestId: string): Observable<ConversionStatus> {
    return this.apiService.get<ConversionStatus>(`convert/by-request/${requestId}`);
  }
}