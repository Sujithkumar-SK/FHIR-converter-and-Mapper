import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionService } from '../../../../core/services/conversion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FhirBundlePreview } from '../../../../core/models/conversion.model';

@Component({
  selector: 'app-fhir-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fhir-preview.component.html',
  styleUrl: './fhir-preview.component.css'
})
export class FhirPreviewComponent implements OnInit {
  @Input() jobId!: string;
  
  fhirPreview: FhirBundlePreview | null = null;
  isLoading = false;
  isDownloading = false;

  constructor(
    private conversionService: ConversionService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadFhirPreview();
  }

  loadFhirPreview(): void {
    this.isLoading = true;
    
    this.conversionService.getFhirPreview(this.jobId).subscribe({
      next: (preview) => {
        this.fhirPreview = preview;
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load FHIR preview');
        this.isLoading = false;
      }
    });
  }

  downloadFhirBundle(): void {
    this.isDownloading = true;
    
    this.conversionService.downloadFhirBundle(this.jobId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `fhir-bundle-${this.jobId.substring(0, 8)}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
        
        this.notificationService.showSuccess('FHIR bundle downloaded successfully');
        this.isDownloading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to download FHIR bundle');
        this.isDownloading = false;
      }
    });
  }
}
