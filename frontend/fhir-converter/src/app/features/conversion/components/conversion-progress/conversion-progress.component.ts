import { Component, Input, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionService } from '../../../../core/services/conversion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConversionStatus, ConversionStatusLabels, FieldMapping, StartConversionRequest } from '../../../../core/models/conversion.model';
import { interval, Subscription } from 'rxjs';
import { switchMap, takeWhile } from 'rxjs/operators';

@Component({
  selector: 'app-conversion-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './conversion-progress.component.html',
  styleUrl: './conversion-progress.component.css'
})
export class ConversionProgressComponent implements OnInit, OnDestroy {
  @Input() fileId!: string;
  @Input() fieldMappings: FieldMapping[] = [];
  @Output() conversionComplete = new EventEmitter<string>();
  
  conversionStatus: ConversionStatus | null = null;
  isStarting = false;
  private statusSubscription?: Subscription;
  
  readonly ConversionStatusLabels = ConversionStatusLabels;

  constructor(
    private conversionService: ConversionService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.startConversion();
  }

  ngOnDestroy(): void {
    this.statusSubscription?.unsubscribe();
  }

  private startConversion(): void {
    this.isStarting = true;
    
    const request: StartConversionRequest = {
      fileId: this.fileId,
      fieldMappings: this.fieldMappings
    };

    this.conversionService.startConversion(request).subscribe({
      next: (status) => {
        this.conversionStatus = status;
        this.isStarting = false;
        this.startStatusPolling(status.jobId);
      },
      error: (error) => {
        this.isStarting = false;
        this.notificationService.showError('Failed to start conversion');
      }
    });
  }

  private startStatusPolling(jobId: string): void {
    this.statusSubscription = interval(2000)
      .pipe(
        switchMap(() => this.conversionService.getConversionStatus(jobId)),
        takeWhile(status => status.status === 'Processing', true)
      )
      .subscribe({
        next: (status) => {
          this.conversionStatus = status;
          if (status.status === 'Completed') {
            this.conversionComplete.emit(jobId);
          } else if (status.status === 'Failed') {
            this.notificationService.showError(status.errorMessage || 'Conversion failed');
          }
        },
        error: (error) => {
          this.notificationService.showError('Failed to get conversion status');
        }
      });
  }

  getProgressBarClass(): string {
    if (!this.conversionStatus) return 'bg-secondary';
    
    switch (this.conversionStatus.status) {
      case 'Processing': return 'bg-primary progress-bar-animated progress-bar-striped';
      case 'Completed': return 'bg-success';
      case 'Failed': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getStatusIcon(): string {
    if (!this.conversionStatus) return 'fas fa-clock';
    
    switch (this.conversionStatus.status) {
      case 'Processing': return 'fas fa-spinner fa-spin';
      case 'Completed': return 'fas fa-check-circle';
      case 'Failed': return 'fas fa-exclamation-circle';
      default: return 'fas fa-clock';
    }
  }

  getStatusColor(): string {
    if (!this.conversionStatus) return 'text-secondary';
    
    switch (this.conversionStatus.status) {
      case 'Processing': return 'text-primary';
      case 'Completed': return 'text-success';
      case 'Failed': return 'text-danger';
      default: return 'text-secondary';
    }
  }

  formatProcessingTime(timeMs?: number): string {
    if (!timeMs) return 'N/A';
    
    const seconds = Math.floor(timeMs / 1000);
    const minutes = Math.floor(seconds / 60);
    
    if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    }
    return `${seconds}s`;
  }
}
