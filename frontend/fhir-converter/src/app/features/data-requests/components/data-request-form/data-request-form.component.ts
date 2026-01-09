import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DataRequestService } from '../../../../core/services/data-request.service';
import { PatientService } from '../../../../core/services/patient.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CreateDataRequestDto } from '../../../../core/models/data-request.model';
import { PatientResponse } from '../../../../core/services/patient.service';

@Component({
  selector: 'app-data-request-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './data-request-form.component.html',
  styleUrl: './data-request-form.component.css'
})
export class DataRequestFormComponent implements OnInit {
  requestForm: FormGroup;
  patients: PatientResponse[] = [];
  filteredPatients: PatientResponse[] = [];
  selectedPatient: PatientResponse | null = null;
  searchTerm = '';
  isLoading = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private dataRequestService: DataRequestService,
    private patientService: PatientService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.requestForm = this.fb.group({
      globalPatientId: ['', Validators.required],
      sourceOrganizationId: ['', Validators.required],
      notes: ['', [Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.isLoading = true;
    this.patientService.getAllPatients().subscribe({
      next: (patients) => {
        // Get current user's organization ID from localStorage
        const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
        const currentOrgId = currentUser.organizationId;
        
        // Filter out patients from the same organization
        const availablePatients = patients.filter(patient => 
          patient.sourceOrganizationId !== currentOrgId
        );
        
        this.patients = availablePatients;
        this.filteredPatients = availablePatients;
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to load patients');
        this.isLoading = false;
      }
    });
  }

  onSearchChange(): void {
    if (!this.searchTerm.trim()) {
      this.filteredPatients = this.patients;
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredPatients = this.patients.filter(patient => 
      patient.firstName?.toLowerCase().includes(term) ||
      patient.lastName?.toLowerCase().includes(term) ||
      patient.localPatientId?.toLowerCase().includes(term) ||
      patient.globalPatientId?.toLowerCase().includes(term)
    );
  }

  onPatientSelect(patient: PatientResponse): void {
    this.selectedPatient = patient;
    this.requestForm.patchValue({
      globalPatientId: patient.globalPatientId,
      sourceOrganizationId: patient.sourceOrganizationId
    });
  }

  onSubmit(): void {
    if (this.requestForm.valid) {
      this.isSubmitting = true;
      const request: CreateDataRequestDto = this.requestForm.value;
      
      this.dataRequestService.createRequest(request).subscribe({
        next: (response) => {
          this.notificationService.showSuccess('Data request created successfully');
          this.router.navigate(['/app/data-requests']);
        },
        error: (error) => {
          this.notificationService.showError(error.error?.message || 'Failed to create data request');
          this.isSubmitting = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/app/data-requests']);
  }
}