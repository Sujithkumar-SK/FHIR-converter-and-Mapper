import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AnalyticsService, SystemOverview, ConversionStatistics } from '../../core/services/analytics.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container-fluid py-5">
      <div class="row">
        <div class="col-12">
          <div class="text-center mb-5">
            <h1 class="display-4 fw-bold text-primary mb-3">
              <i class="fas fa-tachometer-alt me-3"></i>
              Welcome to FHIR Converter
            </h1>
            <p class="lead text-muted">Your healthcare data conversion platform is ready</p>
          </div>
        </div>
      </div>
      
      <div class="row g-4">
        <!-- Patient Management Card - Hospital/Clinic Only -->
        <div class="col-md-6 col-lg-3" *ngIf="!isAdmin">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-warning text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-users fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Patient Management</h4>
              <p class="card-text text-muted">Register and search patient identifiers</p>
              <button class="btn btn-warning btn-lg" routerLink="/app/patients">
                <i class="fas fa-user-plus me-2"></i>Manage Patients
              </button>
            </div>
          </div>
        </div>
        
        <!-- Convert Data Card - Hospital/Clinic Only -->
        <div class="col-md-6 col-lg-3" *ngIf="!isAdmin">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-primary text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-exchange-alt fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Convert Data</h4>
              <p class="card-text text-muted">Convert CSV, JSON, and CCDA files to FHIR R4 format</p>
              <button class="btn btn-primary btn-lg" routerLink="/app/conversion">
                <i class="fas fa-upload me-2"></i>Start Converting
              </button>
            </div>
          </div>
        </div>
        
        <!-- Data Requests Card - Hospital/Clinic Only -->
        <div class="col-md-6 col-lg-3" *ngIf="!isAdmin">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-success text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-share-alt fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Data Requests</h4>
              <p class="card-text text-muted">Manage inter-hospital data sharing requests</p>
              <button class="btn btn-success btn-lg" routerLink="/app/data-requests">
                <i class="fas fa-plus me-2"></i>New Request
              </button>
            </div>
          </div>
        </div>
        
        <!-- Analytics Card - Admin Only -->
        <div class="col-md-6 col-lg-3" *ngIf="isAdmin">
          <div class="card h-100 border-0 shadow-lg dashboard-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-info text-white rounded-circle mx-auto mb-3">
                <i class="fas fa-chart-bar fa-2x"></i>
              </div>
              <h4 class="card-title fw-bold">Analytics</h4>
              <p class="card-text text-muted">View conversion history and system analytics</p>
              <button class="btn btn-info btn-lg" routerLink="/app/analytics">
                <i class="fas fa-chart-line me-2"></i>View Reports
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Recent Activity Section -->
      <div class="row mt-5">
        <div class="col-12">
          <div class="card border-0 shadow-lg">
            <div class="card-header bg-white border-0 py-4">
              <h3 class="card-title mb-0 fw-bold">
                <i class="fas fa-clock me-2 text-primary"></i>
                Recent Activity
              </h3>
            </div>
            <div class="card-body">
              <div class="text-center py-5">
                <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
                <h5 class="text-muted">No recent activity</h5>
                <p class="text-muted">Start converting data to see your activity here</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .analytics-card {
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      border-radius: 15px;
    }
    
    .analytics-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 20px 40px -12px rgba(0, 0, 0, 0.2) !important;
    }
    
    .analytics-icon {
      width: 70px;
      height: 70px;
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
    
    .card-header {
      border-radius: 15px 15px 0 0 !important;
    }
  `]
})
export class DashboardComponent implements OnInit {
  systemOverview?: SystemOverview;
  conversionStats?: ConversionStatistics;
  loading = true;
  isAdmin = false;

  constructor(
    private analyticsService: AnalyticsService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.checkUserRole();
    this.loadDashboardData();
  }

  private checkUserRole(): void {
    const userRole = this.authService.getUserRole();
    this.isAdmin = userRole === 'Admin';
  }

  private loadDashboardData(): void {
    if (this.isAdmin) {
      this.loadAnalytics();
    } else {
      this.loading = false;
    }
  }

  private loadAnalytics(): void {
    this.analyticsService.getSystemOverview().subscribe({
      next: (data) => {
        this.systemOverview = data;
        this.loadConversionStats();
      },
      error: (error) => {
        console.error('Error loading system overview:', error);
        this.loading = false;
      }
    });
  }

  private loadConversionStats(): void {
    this.analyticsService.getConversionStatistics().subscribe({
      next: (data) => {
        this.conversionStats = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading conversion stats:', error);
        this.loading = false;
      }
    });
  }
}