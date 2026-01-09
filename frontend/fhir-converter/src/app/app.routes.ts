import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: 'register',
        loadComponent: () => import('./features/auth/components/register/register.component').then(m => m.RegisterComponent)
      },
      {
        path: 'verify-otp',
        loadComponent: () => import('./features/auth/components/otp-verification/otp-verification.component').then(m => m.OtpVerificationComponent)
      },
      {
        path: 'login',
        loadComponent: () => import('./features/auth/components/login/login.component').then(m => m.LoginComponent)
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    path: 'app',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'patients',
        loadChildren: () => import('./features/patients/patients.module').then(m => m.PatientsModule)
      },
      {
        path: 'data-requests',
        loadChildren: () => import('./features/data-requests/data-requests.module').then(m => m.DataRequestsModule)
      },
      {
        path: 'analytics',
        loadComponent: () => import('./features/analytics/analytics.component').then(m => m.AnalyticsComponent)
      },
      {
        path: 'conversion',
        loadChildren: () => import('./features/conversion/conversion.module').then(m => m.ConversionModule)
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/auth/login' }
];
