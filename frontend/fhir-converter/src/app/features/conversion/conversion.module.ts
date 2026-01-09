import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionPageComponent } from './conversion-page.component';
import { ConversionRoutingModule } from './conversion-routing.module';
import { FileUploadComponent } from '../files/components/file-upload/file-upload.component';
import { ConversionHistoryComponent } from './components/conversion-history/conversion-history.component';

@NgModule({
  declarations: [
    ConversionPageComponent
  ],
  imports: [
    CommonModule,
    ConversionRoutingModule,
    FileUploadComponent,
    ConversionHistoryComponent
  ]
})
export class ConversionModule { }
