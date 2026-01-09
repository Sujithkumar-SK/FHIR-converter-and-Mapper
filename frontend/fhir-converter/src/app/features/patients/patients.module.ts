import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { PatientsRoutingModule } from './patients-routing.module';
import { PatientSearchComponent } from './components/patient-search/patient-search.component';
import { PatientRegisterComponent } from './components/patient-register/patient-register.component';
import { PatientDashboardComponent } from './components/patient-dashboard/patient-dashboard.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PatientsRoutingModule,
    PatientSearchComponent,
    PatientRegisterComponent,
    PatientDashboardComponent
  ]
})
export class PatientsModule { }