import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  
  userRoles = [
    { value: 2, label: 'Hospital Staff', icon: 'fas fa-hospital', orgLabel: 'Hospital Name' },
    { value: 3, label: 'Clinic Staff', icon: 'fas fa-clinic-medical', orgLabel: 'Clinic Name' }
  ];

  selectedRole: number | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      role: ['', [Validators.required]],
      organizationName: ['', [Validators.required, Validators.maxLength(200)]],
      mobileNumber: ['', [Validators.required, Validators.pattern(/^[6-9]\d{9}$/)]]
    }, { validators: this.passwordMatchValidator });

    // Watch for role changes to update labels
    this.registerForm.get('role')?.valueChanges.subscribe(value => {
      this.selectedRole = value ? parseInt(value) : null;
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const formValue = this.registerForm.value;
      const request = {
        email: formValue.email,
        password: formValue.password,
        role: parseInt(formValue.role),
        organizationName: formValue.organizationName,
        mobileNumber: formValue.mobileNumber
      };

      this.authService.registerWithOtp(request).subscribe({
        next: (response) => {
          // Store both OTP response data and original registration data
          sessionStorage.setItem('otpData', JSON.stringify({
            email: response.email,
            otpToken: response.otpToken,
            role: request.role,
            expiresAt: response.expiresAt
          }));
          
          // Store original registration data for resend functionality
          sessionStorage.setItem('originalRegistrationData', JSON.stringify(request));
          
          // Navigate to OTP verification
          this.router.navigate(['/auth/verify-otp']);
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
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
      if (field.errors['email']) return 'Invalid email format';
      if (field.errors['minlength']) return `Minimum ${field.errors['minlength'].requiredLength} characters required`;
      if (field.errors['maxlength']) return `Maximum ${field.errors['maxlength'].requiredLength} characters allowed`;
      if (field.errors['passwordMismatch']) return 'Passwords do not match';
    }
    return '';
  }

  getOrganizationLabel(): string {
    const role = this.userRoles.find(r => r.value === this.selectedRole);
    return role?.orgLabel || 'Organization Name';
  }

  getOrganizationPlaceholder(): string {
    const role = this.userRoles.find(r => r.value === this.selectedRole);
    return role?.value === 2 ? 'Enter hospital name' : 'Enter clinic name';
  }
}