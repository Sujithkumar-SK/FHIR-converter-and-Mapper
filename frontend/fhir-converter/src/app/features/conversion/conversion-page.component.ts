import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileUploadComponent } from '../files/components/file-upload/file-upload.component';
import { ConversionHistoryComponent } from './components/conversion-history/conversion-history.component';
import { FieldMappingComponent } from './components/field-mapping/field-mapping.component';
import { ConversionProgressComponent } from './components/conversion-progress/conversion-progress.component';
import { FhirPreviewComponent } from './components/fhir-preview/fhir-preview.component';
import { FileUploadResponse } from '../../core/models/file.model';
import { FieldMapping } from '../../core/models/conversion.model';

@Component({
  selector: 'app-conversion-page',
  templateUrl: './conversion-page.component.html',
  styleUrls: ['./conversion-page.component.css']
})
export class ConversionPageComponent {
  currentStep = 1;
  uploadedFile: FileUploadResponse | null = null;
  conversionJobId: string | null = null;
  fieldMappings: FieldMapping[] = [];

  onFileUploaded(file: FileUploadResponse): void {
    this.uploadedFile = file;
    this.currentStep = 2; // Move to field mapping
  }

  onMappingComplete(mappings: FieldMapping[]): void {
    // Store mappings for conversion step
    this.fieldMappings = mappings;
    this.currentStep = 3; // Move to conversion progress
  }

  onConversionComplete(jobId: string): void {
    this.conversionJobId = jobId;
    this.currentStep = 4; // Move to FHIR preview
  }

  startNewConversion(): void {
    this.currentStep = 1;
    this.uploadedFile = null;
    this.conversionJobId = null;
    this.fieldMappings = [];
  }
}