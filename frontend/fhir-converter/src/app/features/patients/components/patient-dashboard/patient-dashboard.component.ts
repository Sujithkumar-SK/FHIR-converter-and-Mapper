import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container-fluid py-5">
      <div class="row">
        <div class="col-12">
          <div class="text-center mb-5">
            <h1 class="display-4 fw-bold text-primary mb-3">
              <i class="fas fa-users me-3"></i>
              Patient Management
            </h1>
            <p class="lead text-muted">Manage patient identifiers and cross-hospital data sharing</p>
          </div>
        </div>
      </div>
      
      <div class="row g-4 mb-5">
        <!-- Register Patient Card -->
        <div class="col-md-6">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-success text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-user-plus fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Register Patient</h4>
              <p class="card-text text-muted">Add new patient identifiers to your organization</p>
              <button class="btn btn-success btn-lg" routerLink="/app/patients/register">
                <i class="fas fa-plus me-2"></i>Register New Patient
              </button>
            </div>
          </div>
        </div>
        
        <!-- Search Patients Card -->
        <div class="col-md-6">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-primary text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-search fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Search Patients</h4>
              <p class="card-text text-muted">Find patients across hospitals for data requests</p>
              <button class="btn btn-primary btn-lg" routerLink="/app/patients/search">
                <i class="fas fa-search me-2"></i>Search Patients
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-card {
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      border-radius: 15px;
    }
    
    .dashboard-card:hover {
      transform: translateY(-8px);
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25) !important;
    }
    
    .feature-icon {
      width: 80px;
      height: 80px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .btn-lg {
      padding: 0.75rem 2rem;
      border-radius: 10px;
      font-weight: 600;
      transition: all 0.3s ease;
    }
    
    .btn:hover {
      transform: translateY(-2px);
    }
  `]
})
export class PatientDashboardComponent {

}