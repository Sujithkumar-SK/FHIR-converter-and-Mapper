import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DataRequestListComponent } from './components/data-request-list/data-request-list.component';
import { DataRequestFormComponent } from './components/data-request-form/data-request-form.component';
import { ApprovalDashboardComponent } from './components/approval-dashboard/approval-dashboard.component';
import { DataRequestDetailsComponent } from './components/data-request-details/data-request-details.component';

const routes: Routes = [
  { path: '', component: DataRequestListComponent },
  { path: 'create', component: DataRequestFormComponent },
  { path: 'approvals', component: ApprovalDashboardComponent },
  { path: ':id', component: DataRequestDetailsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DataRequestsRoutingModule { }