import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionService } from '../../../../core/services/conversion.service';
import { ConversionStatus, ConversionStatusLabels } from '../../../../core/models/conversion.model';

@Component({
  selector: 'app-conversion-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './conversion-history.component.html',
  styleUrl: './conversion-history.component.css'
})
export class ConversionHistoryComponent implements OnInit {
  conversions: ConversionStatus[] = [];
  isLoading = false;
  
  readonly ConversionStatusLabels = ConversionStatusLabels;

  constructor(private conversionService: ConversionService) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.conversionService.getConversionHistory().subscribe({
      next: (conversions) => {
        this.conversions = conversions;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Completed': return 'badge bg-success';
      case 'Processing': return 'badge bg-primary';
      case 'Failed': return 'badge bg-danger';
      default: return 'badge bg-secondary';
    }
  }

  downloadBundle(jobId: string): void {
    this.conversionService.downloadFhirBundle(jobId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `fhir-bundle-${jobId.substring(0, 8)}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Download failed:', error);
      }
    });
  }
}
