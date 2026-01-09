import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { PatientService, PatientSearchRequest, PatientResponse } from '../../../../core/services/patient.service';

@Component({
  selector: 'app-patient-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-search.component.html',
  styleUrl: './patient-search.component.css'
})
export class PatientSearchComponent {
  searchForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  allPatients: PatientResponse[] = [];
  filteredPatients: PatientResponse[] = [];
  hasLoaded = false;

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService
  ) {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      dateOfBirth: ['']
    });
    
    // Load all patients on component init
    this.loadAllPatients();
    
    // Watch for search term changes
    this.searchForm.get('searchTerm')?.valueChanges.subscribe(() => {
      this.filterPatients();
    });
    
    this.searchForm.get('dateOfBirth')?.valueChanges.subscribe(() => {
      this.filterPatients();
    });
  }

  loadAllPatients(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.patientService.getAllPatients().subscribe({
      next: (patients) => {
        this.allPatients = patients;
        this.filteredPatients = patients;
        this.hasLoaded = true;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load patients';
        this.isLoading = false;
      }
    });
  }

  filterPatients(): void {
    const searchTerm = this.searchForm.get('searchTerm')?.value?.toLowerCase() || '';
    const dateOfBirth = this.searchForm.get('dateOfBirth')?.value;
    
    this.filteredPatients = this.allPatients.filter(patient => {
      const matchesSearch = !searchTerm || 
        patient.firstName?.toLowerCase().includes(searchTerm) ||
        patient.lastName?.toLowerCase().includes(searchTerm) ||
        patient.localPatientId?.toLowerCase().includes(searchTerm) ||
        `${patient.firstName} ${patient.lastName}`.toLowerCase().includes(searchTerm);
      
      const matchesDate = !dateOfBirth || 
        (patient.dateOfBirth && new Date(patient.dateOfBirth).toISOString().split('T')[0] === dateOfBirth);
      
      return matchesSearch && matchesDate;
    });
  }

  clearSearch(): void {
    this.searchForm.reset();
    this.filteredPatients = this.allPatients;
    this.errorMessage = '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.searchForm.controls).forEach(key => {
      const control = this.searchForm.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.searchForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.searchForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['maxlength']) return `${fieldName} is too long`;
    }
    return '';
  }
}