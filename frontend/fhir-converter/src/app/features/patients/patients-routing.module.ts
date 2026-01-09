import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PatientSearchComponent } from './components/patient-search/patient-search.component';
import { PatientRegisterComponent } from './components/patient-register/patient-register.component';
import { PatientDashboardComponent } from './components/patient-dashboard/patient-dashboard.component';

const routes: Routes = [
  { path: '', component: PatientDashboardComponent },
  { path: 'search', component: PatientSearchComponent },
  { path: 'register', component: PatientRegisterComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PatientsRoutingModule { }