import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../../../../core/services/file.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FileUploadResponse, InputFormat } from '../../../../core/models/file.model';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-upload.component.html',
  styleUrl: './file-upload.component.css'
})
export class FileUploadComponent {
  @Input() requestId?: string;
  @Output() fileUploaded = new EventEmitter<FileUploadResponse>();
  
  selectedFile: File | null = null;
  isDragOver = false;
  isUploading = false;
  uploadProgress = 0;
  
  readonly maxFileSize = 50 * 1024 * 1024; // 50MB
  readonly allowedTypes = ['text/csv', 'application/json', 'text/xml', 'application/xml'];
  readonly allowedExtensions = ['.csv', '.json', '.xml'];

  constructor(
    private fileService: FileService,
    private notificationService: NotificationService
  ) {}

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleFileSelection(file);
    }
  }

  private handleFileSelection(file: File): void {
    if (!this.validateFile(file)) {
      return;
    }
    
    this.selectedFile = file;
  }

  private validateFile(file: File): boolean {
    if (file.size > this.maxFileSize) {
      this.notificationService.showError('File size exceeds 50MB limit');
      return false;
    }

    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!this.allowedExtensions.includes(extension)) {
      this.notificationService.showError('Only CSV, JSON, and XML files are allowed');
      return false;
    }

    if (!this.allowedTypes.includes(file.type)) {
      this.notificationService.showError('Invalid file type');
      return false;
    }

    return true;
  }

  uploadFile(): void {
    if (!this.selectedFile) {
      this.notificationService.showError('Please select a file');
      return;
    }

    this.isUploading = true;
    this.uploadProgress = 0;

    this.fileService.uploadFile(this.selectedFile, this.requestId).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.uploadProgress = 100;
        this.notificationService.showSuccess('File uploaded successfully');
        this.fileUploaded.emit(response);
        this.resetForm();
      },
      error: (error) => {
        this.isUploading = false;
        this.uploadProgress = 0;
        this.notificationService.showError(error.error?.message || 'Upload failed');
      }
    });
  }

  removeFile(): void {
    this.selectedFile = null;
  }

  private resetForm(): void {
    this.selectedFile = null;
    this.uploadProgress = 0;
  }

  getFileIcon(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'csv': return '📊';
      case 'json': return '📄';
      case 'xml': return '📋';
      default: return '📁';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
