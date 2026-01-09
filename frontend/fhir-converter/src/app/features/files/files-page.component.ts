import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { FileListComponent } from './components/file-list/file-list.component';
import { FilePreviewComponent } from './components/file-preview/file-preview.component';
import { FileUploadResponse } from '../../core/models/file.model';

@Component({
  selector: 'app-files-page',
  standalone: true,
  imports: [CommonModule, FileUploadComponent, FileListComponent, FilePreviewComponent],
  template: `
    <div class="files-page">
      <div class="page-header">
        <h2>File Upload & Management</h2>
        <p>Upload and manage your medical data files for FHIR conversion</p>
      </div>

      <div class="upload-section">
        <app-file-upload 
          [requestId]="selectedRequestId"
          (fileUploaded)="onFileUploaded($event)">
        </app-file-upload>
      </div>

      <div class="files-section" *ngIf="uploadedFiles.length > 0">
        <app-file-list [files]="uploadedFiles"></app-file-list>
      </div>

      <div class="preview-section" *ngIf="selectedFileId">
        <h3>File Preview</h3>
        <app-file-preview [fileId]="selectedFileId"></app-file-preview>
      </div>
    </div>
  `,
  styles: [`
    .files-page {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .page-header {
      text-align: center;
      margin-bottom: 40px;
    }

    .page-header h2 {
      color: #333;
      margin-bottom: 10px;
    }

    .page-header p {
      color: #666;
      font-size: 16px;
    }

    .upload-section {
      margin-bottom: 40px;
    }

    .files-section {
      margin-bottom: 40px;
    }

    .preview-section h3 {
      color: #333;
      margin-bottom: 20px;
    }
  `]
})
export class FilesPageComponent {
  uploadedFiles: FileUploadResponse[] = [];
  selectedRequestId?: string;
  selectedFileId?: string;

  onFileUploaded(file: FileUploadResponse): void {
    this.uploadedFiles.unshift(file); // Add to beginning of array
    this.selectedFileId = file.fileId; // Auto-select for preview
  }
}