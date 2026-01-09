import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { FileUploadResponse, FileValidationResult, FilePreviewResponse } from '../models/file.model';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  constructor(private apiService: ApiService) { }

  uploadFile(file: File, requestId?: string): Observable<FileUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    
    const endpoint = requestId ? `files/upload?requestId=${requestId}` : 'files/upload';
    
    return this.apiService.postFormData<FileUploadResponse>(endpoint, formData);
  }

  validateFile(fileId: string): Observable<FileValidationResult> {
    return this.apiService.get<FileValidationResult>(`files/${fileId}/validate`);
  }

  getFilePreview(fileId: string): Observable<FilePreviewResponse> {
    return this.apiService.get<FilePreviewResponse>(`files/${fileId}/preview`);
  }

  deleteFile(fileId: string): Observable<any> {
    return this.apiService.delete(`files/${fileId}`);
  }
}
