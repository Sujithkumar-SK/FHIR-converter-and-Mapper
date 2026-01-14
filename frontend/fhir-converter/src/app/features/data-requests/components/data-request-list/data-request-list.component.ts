import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DataRequestService } from '../../../../core/services/data-request.service';
import { ConversionService } from '../../../../core/services/conversion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DataRequestResponse, DataRequestStatus, DataRequestStatusLabels } from '../../../../core/models/data-request.model';
import { IstDatePipe } from '../../../../shared/pipes/ist-date.pipe';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-data-request-list',
  standalone: true,
  imports: [CommonModule, IstDatePipe],
  templateUrl: './data-request-list.component.html',
  styleUrl: './data-request-list.component.css'
})
export class DataRequestListComponent implements OnInit, OnDestroy {
  requests: DataRequestResponse[] = [];
  isLoading = false;
  activeTab: 'sent' | 'received' = 'sent';
  DataRequestStatus = DataRequestStatus;
  DataRequestStatusLabels = DataRequestStatusLabels;
  private refreshSubscription?: Subscription;

  constructor(
    private dataRequestService: DataRequestService,
    private conversionService: ConversionService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadRequests();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  private startAutoRefresh(): void {
    // Refresh every 30 seconds
    this.refreshSubscription = interval(30000)
      .pipe(switchMap(() => {
        const isRequesting = this.activeTab === 'sent';
        return this.dataRequestService.getRequestsByOrganization(isRequesting);
      }))
      .subscribe({
        next: (requests) => {
          this.requests = requests;
        },
        error: () => {
          // Silent refresh - don't show error notifications
        }
      });
  }

  setActiveTab(tab: 'sent' | 'received'): void {
    this.activeTab = tab;
    this.loadRequests();
    // Restart auto-refresh for new tab
    this.refreshSubscription?.unsubscribe();
    this.startAutoRefresh();
  }

  loadRequests(): void {
    this.isLoading = true;
    const isRequesting = this.activeTab === 'sent';
    
    this.dataRequestService.getRequestsByOrganization(isRequesting).subscribe({
      next: (requests) => {
        this.requests = requests;
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load data requests');
        this.isLoading = false;
      }
    });
  }

  createNewRequest(): void {
    this.router.navigate(['/app/data-requests/create']);
  }

  viewRequest(requestId: string): void {
    this.router.navigate(['/app/data-requests', requestId]);
  }

  viewRequestDetails(requestId: string): void {
    this.router.navigate(['/app/data-requests', requestId]);
  }

  getStatusBadgeClass(status: DataRequestStatus): string {
    switch (status) {
      case DataRequestStatus.Pending:
        return 'badge bg-warning';
      case DataRequestStatus.Approved:
        return 'badge bg-success';
      case DataRequestStatus.Rejected:
        return 'badge bg-danger';
      case DataRequestStatus.DataReady:
        return 'badge bg-primary';
      case DataRequestStatus.Completed:
        return 'badge bg-info';
      case DataRequestStatus.Expired:
        return 'badge bg-secondary';
      default:
        return 'badge bg-secondary';
    }
  }

  getStatusIcon(status: DataRequestStatus): string {
    switch (status) {
      case DataRequestStatus.Pending:
        return '⏳';
      case DataRequestStatus.Approved:
        return '✅';
      case DataRequestStatus.Rejected:
        return '❌';
      case DataRequestStatus.DataReady:
        return '📦';
      case DataRequestStatus.Completed:
        return '🎯';
      case DataRequestStatus.Expired:
        return '⏰';
      default:
        return '❓';
    }
  }

  isExpired(expiresAt: Date): boolean {
    return new Date(expiresAt) <= new Date();
  }

  canDownload(request: DataRequestResponse): boolean {
    return request.status === DataRequestStatus.DataReady && this.activeTab === 'sent';
  }

  downloadData(request: DataRequestResponse): void {
    // Get conversion job by request ID
    this.conversionService.getConversionByRequestId(request.requestId).subscribe({
      next: (job) => {
        this.conversionService.downloadFhirBundle(job.jobId).subscribe({
          next: (blob) => {
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `patient-data-${request.globalPatientId.substring(0, 8)}.json`;
            link.click();
            window.URL.revokeObjectURL(url);
            
            this.notificationService.showSuccess('Patient data downloaded successfully');
            this.loadRequests(); // Refresh to show updated status
          },
          error: () => {
            this.notificationService.showError('Failed to download patient data');
          }
        });
      },
      error: () => {
        this.notificationService.showError('Conversion job not found for this request');
      }
    });
  }
}