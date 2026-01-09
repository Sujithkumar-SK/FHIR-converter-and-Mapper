import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { DataRequestsRoutingModule } from './data-requests-routing.module';
import { DataRequestListComponent } from './components/data-request-list/data-request-list.component';
import { DataRequestFormComponent } from './components/data-request-form/data-request-form.component';
import { ApprovalDashboardComponent } from './components/approval-dashboard/approval-dashboard.component';
import { DataRequestDetailsComponent } from './components/data-request-details/data-request-details.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DataRequestsRoutingModule,
    DataRequestListComponent,
    DataRequestFormComponent,
    ApprovalDashboardComponent,
    DataRequestDetailsComponent
  ]
})
export class DataRequestsModule { }