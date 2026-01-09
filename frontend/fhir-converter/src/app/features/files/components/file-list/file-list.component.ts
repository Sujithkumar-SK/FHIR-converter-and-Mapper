import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../../../../core/services/file.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FileUploadResponse, FileValidationResult, InputFormat, ConversionStatus } from '../../../../core/models/file.model';

@Component({
  selector: 'app-file-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-list.component.html',
  styleUrl: './file-list.component.css'
})
export class FileListComponent implements OnInit {
  @Input() files: FileUploadResponse[] = [];
  
  validationResults: Map<string, FileValidationResult> = new Map();
  loadingValidation: Set<string> = new Set();
  
  readonly InputFormat = InputFormat;
  readonly ConversionStatus = ConversionStatus;

  constructor(
    private fileService: FileService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.validateAllFiles();
  }

  private validateAllFiles(): void {
    this.files.forEach(file => {
      if (file.status === ConversionStatus.Processing) {
        this.validateFile(file.fileId);
      }
    });
  }

  validateFile(fileId: string): void {
    if (this.loadingValidation.has(fileId)) return;
    
    this.loadingValidation.add(fileId);
    
    this.fileService.validateFile(fileId).subscribe({
      next: (result) => {
        this.validationResults.set(fileId, result);
        this.loadingValidation.delete(fileId);
      },
      error: (error) => {
        this.loadingValidation.delete(fileId);
        this.notificationService.showError('Validation failed: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  deleteFile(fileId: string): void {
    if (confirm('Are you sure you want to delete this file?')) {
      this.fileService.deleteFile(fileId).subscribe({
        next: () => {
          this.files = this.files.filter(f => f.fileId !== fileId);
          this.validationResults.delete(fileId);
          this.notificationService.showSuccess('File deleted successfully');
        },
        error: (error) => {
          this.notificationService.showError('Delete failed: ' + (error.error?.message || 'Unknown error'));
        }
      });
    }
  }

  getFileIcon(format: InputFormat): string {
    switch (format) {
      case InputFormat.CSV: return '📊';
      case InputFormat.JSON: return '📄';
      case InputFormat.CCDA: return '📋';
      default: return '📁';
    }
  }

  getStatusIcon(status: ConversionStatus): string {
    switch (status) {
      case ConversionStatus.Processing: return '⏳';
      case ConversionStatus.Completed: return '✅';
      case ConversionStatus.Failed: return '❌';
      default: return '❓';
    }
  }

  getStatusText(status: ConversionStatus): string {
    switch (status) {
      case ConversionStatus.Processing: return 'Processing';
      case ConversionStatus.Completed: return 'Completed';
      case ConversionStatus.Failed: return 'Failed';
      default: return 'Unknown';
    }
  }

  getFormatText(format: InputFormat): string {
    switch (format) {
      case InputFormat.CSV: return 'CSV';
      case InputFormat.JSON: return 'JSON';
      case InputFormat.CCDA: return 'CCDA/XML';
      default: return 'Unknown';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }

  isExpired(expiresAt: Date): boolean {
    return new Date(expiresAt) < new Date();
  }

  getValidationResult(fileId: string): FileValidationResult | undefined {
    return this.validationResults.get(fileId);
  }

  isValidating(fileId: string): boolean {
    return this.loadingValidation.has(fileId);
  }
}
