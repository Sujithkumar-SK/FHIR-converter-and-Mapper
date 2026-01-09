import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DataRequestService } from '../../../../core/services/data-request.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { DataRequestResponse, DataRequestStatus, DataRequestStatusLabels, ApproveDataRequestDto } from '../../../../core/models/data-request.model';

@Component({
  selector: 'app-approval-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './approval-dashboard.component.html',
  styleUrl: './approval-dashboard.component.css'
})
export class ApprovalDashboardComponent implements OnInit {
  pendingRequests: DataRequestResponse[] = [];
  isLoading = false;
  selectedRequest: DataRequestResponse | null = null;
  approvalForm: FormGroup;
  isSubmitting = false;
  DataRequestStatus = DataRequestStatus;
  DataRequestStatusLabels = DataRequestStatusLabels;

  constructor(
    private fb: FormBuilder,
    private dataRequestService: DataRequestService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.approvalForm = this.fb.group({
      status: ['', Validators.required],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    this.loadPendingRequests();
  }

  loadPendingRequests(): void {
    this.isLoading = true;
    this.dataRequestService.getPendingRequests().subscribe({
      next: (requests) => {
        this.pendingRequests = requests;
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load pending requests');
        this.isLoading = false;
      }
    });
  }

  selectRequest(request: DataRequestResponse): void {
    this.selectedRequest = request;
    this.approvalForm.reset();
  }

  viewRequestDetails(requestId: string): void {
    this.router.navigate(['/app/data-requests', requestId]);
  }

  closeModal(): void {
    this.selectedRequest = null;
    this.approvalForm.reset();
  }

  onApprovalSubmit(): void {
    if (this.approvalForm.valid && this.selectedRequest) {
      this.isSubmitting = true;
      const approval: ApproveDataRequestDto = {
        status: parseInt(this.approvalForm.value.status),
        notes: this.approvalForm.value.notes
      };

      this.dataRequestService.approveRequest(this.selectedRequest.requestId, approval).subscribe({
        next: (response) => {
          const action = approval.status === DataRequestStatus.Approved ? 'approved' : 'rejected';
          this.notificationService.showSuccess(`Data request ${action} successfully`);
          this.closeModal();
          this.loadPendingRequests();
        },
        error: (error) => {
          this.notificationService.showError(error.error?.message || 'Failed to process request');
          this.isSubmitting = false;
        }
      });
    }
  }

  isExpired(expiresAt: Date): boolean {
    return new Date(expiresAt) <= new Date();
  }
}