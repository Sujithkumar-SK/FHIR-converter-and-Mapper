import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-otp-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './otp-verification.component.html',
  styleUrls: ['./otp-verification.component.css']
})
export class OtpVerificationComponent implements OnInit, OnDestroy {
  otpForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  otpData: any = null;
  timeLeft = 300;
  timerInterval: any;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  ngOnInit(): void {
    // Get OTP data from session storage
    const storedData = sessionStorage.getItem('otpData');
    if (storedData) {
      this.otpData = JSON.parse(storedData);
      this.startTimer();
    } else {
      // Redirect to registration if no OTP data
      this.router.navigate(['/auth/register']);
    }
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  startTimer(): void {
    this.timerInterval = setInterval(() => {
      this.timeLeft--;
      if (this.timeLeft <= 0) {
        clearInterval(this.timerInterval);
        this.errorMessage = 'OTP has expired. Please register again.';
      }
    }, 1000);
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  onSubmit(): void {
    if (this.otpForm.valid && this.otpData) {
      this.isLoading = true;
      this.errorMessage = '';

      const request = {
        email: this.otpData.email,
        otp: this.otpForm.value.otp,
        otpToken: this.otpData.otpToken,
        role: this.otpData.role
      };

      this.authService.verifyRegistrationOtp(request).subscribe({
        next: (response) => {
          this.successMessage = response.message;
          
          // Clear session storage
          sessionStorage.removeItem('otpData');
          
          // Clear timer
          if (this.timerInterval) {
            clearInterval(this.timerInterval);
          }

          // Redirect to login after 2 seconds
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 2000);
          
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'OTP verification failed. Please try again.';
          this.isLoading = false;
        }
      });
    } else {
      this.otpForm.get('otp')?.markAsTouched();
    }
  }

  resendOtp(): void {
    if (this.timeLeft > 0) {
      return; // Don't allow resend if timer is still running
    }

    if (!this.otpData) {
      this.router.navigate(['/auth/register']);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Extract original registration data from session storage
    const originalData = JSON.parse(sessionStorage.getItem('originalRegistrationData') || '{}');
    
    if (!originalData.email) {
      this.router.navigate(['/auth/register']);
      return;
    }

    this.authService.registerWithOtp(originalData).subscribe({
      next: (response) => {
        // Update OTP data with new token
        const newOtpData = {
          email: response.email,
          otpToken: response.otpToken,
          role: originalData.role,
          expiresAt: response.expiresAt
        };
        
        sessionStorage.setItem('otpData', JSON.stringify(newOtpData));
        this.otpData = newOtpData;
        
        // Reset timer
        this.timeLeft = 300;
        this.startTimer();
        
        // Clear form
        this.otpForm.get('otp')?.setValue('');
        
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to resend OTP. Please try again.';
        this.isLoading = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.otpForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.otpForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return 'OTP is required';
      if (field.errors['pattern']) return 'OTP must be 6 digits';
    }
    return '';
  }
}