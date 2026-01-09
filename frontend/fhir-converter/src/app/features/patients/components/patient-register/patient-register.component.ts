import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { PatientService, CreatePatientRequest } from '../../../../core/services/patient.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-patient-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-register.component.html',
  styleUrl: './patient-register.component.css'
})
export class PatientRegisterComponent implements OnInit {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  currentUser: any;

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private authService: AuthService
  ) {
    this.registerForm = this.fb.group({
      localPatientId: ['', [Validators.required, Validators.maxLength(200)]],
      firstName: ['', [Validators.maxLength(200)]],
      lastName: ['', [Validators.required, Validators.maxLength(200)]],
      dateOfBirth: ['']
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
  }

  onSubmit(): void {
    if (this.registerForm.valid && this.currentUser?.organizationId) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';
      
      const request: CreatePatientRequest = {
        localPatientId: this.registerForm.value.localPatientId,
        firstName: this.registerForm.value.firstName || undefined,
        lastName: this.registerForm.value.lastName,
        dateOfBirth: this.registerForm.value.dateOfBirth ? new Date(this.registerForm.value.dateOfBirth) : undefined,
        sourceOrganizationId: this.currentUser.organizationId
      };

      this.patientService.createPatient(request).subscribe({
        next: (response) => {
          this.successMessage = response.message || 'Patient registered successfully';
          this.registerForm.reset();
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Registration failed';
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  clearForm(): void {
    this.registerForm.reset();
    this.errorMessage = '';
    this.successMessage = '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['maxlength']) return `${fieldName} is too long`;
    }
    return '';
  }
}