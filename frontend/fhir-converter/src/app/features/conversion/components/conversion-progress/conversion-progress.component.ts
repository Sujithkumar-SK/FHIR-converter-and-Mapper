import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionService } from '../../../../core/services/conversion.service';
import { ConversionStatus, ConversionStatusLabels } from '../../../../core/models/conversion.model';
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
  @Input() jobId!: string;
  
  conversionStatus: ConversionStatus | null = null;
  private statusSubscription?: Subscription;
  
  readonly ConversionStatusLabels = ConversionStatusLabels;

  constructor(private conversionService: ConversionService) {}

  ngOnInit(): void {
    this.startStatusPolling();
  }

  ngOnDestroy(): void {
    this.statusSubscription?.unsubscribe();
  }

  startStatusPolling(): void {
    this.statusSubscription = interval(2000)
      .pipe(
        switchMap(() => this.conversionService.getConversionStatus(this.jobId)),
        takeWhile(status => status.status === 'Processing', true)
      )
      .subscribe({
        next: (status) => {
          this.conversionStatus = status;
        },
        error: (error) => {
          console.error('Error polling conversion status:', error);
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
