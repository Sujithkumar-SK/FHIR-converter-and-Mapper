import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DataRequestService } from '../../../../core/services/data-request.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { ConversionService } from '../../../../core/services/conversion.service';
import { DataRequestResponse, DataRequestStatus, DataRequestStatusLabels } from '../../../../core/models/data-request.model';
import { FileUploadComponent } from '../../../files/components/file-upload/file-upload.component';
import { FileListComponent } from '../../../files/components/file-list/file-list.component';
import { FieldMappingComponent } from '../../../conversion/components/field-mapping/field-mapping.component';
import { ConversionProgressComponent } from '../../../conversion/components/conversion-progress/conversion-progress.component';
import { FhirPreviewComponent } from '../../../conversion/components/fhir-preview/fhir-preview.component';
import { FileUploadResponse } from '../../../../core/models/file.model';
import { FieldMapping, StartConversionRequest } from '../../../../core/models/conversion.model';

@Component({
  selector: 'app-data-request-details',
  standalone: true,
  imports: [
    CommonModule, 
    FileUploadComponent, 
    FileListComponent,
    FieldMappingComponent,
    ConversionProgressComponent,
    FhirPreviewComponent
  ],
  templateUrl: './data-request-details.component.html',
  styleUrl: './data-request-details.component.css'
})
export class DataRequestDetailsComponent implements OnInit {
  request: DataRequestResponse | null = null;
  isLoading = false;
  uploadedFiles: FileUploadResponse[] = [];
  showUploadSection = false;
  showApprovalSection = false;
  isSubmitting = false;
  
  // Conversion flow states
  currentStep: 'upload' | 'mapping' | 'converting' | 'preview' = 'upload';
  selectedFileForConversion: FileUploadResponse | null = null;
  conversionJobId: string | null = null;
  fieldMappings: FieldMapping[] = [];
  
  readonly DataRequestStatus = DataRequestStatus;
  readonly DataRequestStatusLabels = DataRequestStatusLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dataRequestService: DataRequestService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private conversionService: ConversionService
  ) {}

  ngOnInit(): void {
    const requestId = this.route.snapshot.paramMap.get('id');
    if (requestId) {
      this.loadRequestDetails(requestId);
    }
  }

  loadRequestDetails(requestId: string): void {
    this.isLoading = true;
    this.dataRequestService.getRequestById(requestId).subscribe({
      next: (request) => {
        this.request = request;
        this.isLoading = false;
        
        // Get current user's organization
        const currentUser = this.authService.getCurrentUser();
        const isSourceOrganization = currentUser?.organizationId === request.sourceOrganizationId;
        const isRequestingOrganization = currentUser?.organizationId === request.requestingOrganizationId;
        
        // Show upload section only if:
        // 1. Request is approved AND
        // 2. User is from source organization AND
        // 3. Request is not expired
        this.showUploadSection = request.status === DataRequestStatus.Approved && 
                                isSourceOrganization && 
                                !this.isExpired();
        
        // Show approval section only if:
        // 1. Request is pending AND
        // 2. User is from source organization
        this.showApprovalSection = request.status === DataRequestStatus.Pending && isSourceOrganization;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load request details');
        this.isLoading = false;
        this.router.navigate(['/app/data-requests']);
      }
    });
  }

  onFileUploaded(file: FileUploadResponse): void {
    this.uploadedFiles.unshift(file);
    this.notificationService.showSuccess('File uploaded successfully for this request');
  }

  startConversion(file: FileUploadResponse): void {
    this.selectedFileForConversion = file;
    this.currentStep = 'mapping';
  }

  onMappingComplete(fieldMappings: FieldMapping[]): void {
    if (!this.selectedFileForConversion) return;
    
    // Store field mappings for conversion step
    this.fieldMappings = fieldMappings;
    this.currentStep = 'converting';
  }

  onConversionComplete(): void {
    this.currentStep = 'preview';
  }

  onConversionCompleteWithJobId(jobId: string): void {
    this.conversionJobId = jobId;
    this.currentStep = 'preview';
  }

  goBack(): void {
    this.router.navigate(['/app/data-requests']);
  }

  getStatusBadgeClass(status: DataRequestStatus): string {
    switch (status) {
      case DataRequestStatus.Pending: return 'status-pending';
      case DataRequestStatus.Approved: return 'status-approved';
      case DataRequestStatus.Rejected: return 'status-rejected';
      case DataRequestStatus.Completed: return 'status-completed';
      case DataRequestStatus.Expired: return 'status-expired';
      default: return 'status-unknown';
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }

  isExpired(): boolean {
    return this.request ? new Date(this.request.expiresAt) <= new Date() : false;
  }

  isRequestingOrganization(): boolean {
    if (!this.request) return false;
    const currentUser = this.authService.getCurrentUser();
    return currentUser?.organizationId === this.request.requestingOrganizationId;
  }

  isSourceOrganization(): boolean {
    if (!this.request) return false;
    const currentUser = this.authService.getCurrentUser();
    return currentUser?.organizationId === this.request.sourceOrganizationId;
  }

  approveRequest(): void {
    if (!this.request) return;
    
    this.isSubmitting = true;
    const approval = {
      status: DataRequestStatus.Approved,
      notes: 'Request approved for data sharing'
    };

    this.dataRequestService.approveRequest(this.request.requestId, approval).subscribe({
      next: (updatedRequest) => {
        this.request = updatedRequest;
        this.showApprovalSection = false;
        // Update upload section visibility after approval
        this.showUploadSection = updatedRequest.status === DataRequestStatus.Approved && 
                                this.isSourceOrganization() && 
                                !this.isExpired();
        this.isSubmitting = false;
        this.notificationService.showSuccess('Request approved successfully! You can now upload patient files.');
      },
      error: (error) => {
        this.isSubmitting = false;
        this.notificationService.showError('Failed to approve request: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  rejectRequest(): void {
    if (!this.request) return;
    
    this.isSubmitting = true;
    const approval = {
      status: DataRequestStatus.Rejected,
      notes: 'Request rejected'
    };

    this.dataRequestService.approveRequest(this.request.requestId, approval).subscribe({
      next: (updatedRequest) => {
        this.request = updatedRequest;
        this.showApprovalSection = false;
        this.showUploadSection = false;
        this.isSubmitting = false;
        this.notificationService.showSuccess('Request rejected successfully.');
      },
      error: (error) => {
        this.isSubmitting = false;
        this.notificationService.showError('Failed to reject request: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }
}
