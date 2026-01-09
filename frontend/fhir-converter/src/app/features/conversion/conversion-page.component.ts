import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileUploadComponent } from '../files/components/file-upload/file-upload.component';
import { ConversionHistoryComponent } from './components/conversion-history/conversion-history.component';
import { FileUploadResponse } from '../../core/models/file.model';

@Component({
  selector: 'app-conversion-page',
  templateUrl: './conversion-page.component.html',
  styleUrls: ['./conversion-page.component.css']
})
export class ConversionPageComponent {
  uploadedFiles: FileUploadResponse[] = [];

  onFileUploaded(file: FileUploadResponse): void {
    this.uploadedFiles.unshift(file);
  }
}