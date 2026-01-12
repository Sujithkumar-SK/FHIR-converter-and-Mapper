import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConversionPageComponent } from './conversion-page.component';
import { ConversionRoutingModule } from './conversion-routing.module';
import { FileUploadComponent } from '../files/components/file-upload/file-upload.component';
import { ConversionHistoryComponent } from './components/conversion-history/conversion-history.component';
import { FieldMappingComponent } from './components/field-mapping/field-mapping.component';
import { ConversionProgressComponent } from './components/conversion-progress/conversion-progress.component';
import { FhirPreviewComponent } from './components/fhir-preview/fhir-preview.component';

@NgModule({
  declarations: [
    ConversionPageComponent
  ],
  imports: [
    CommonModule,
    ConversionRoutingModule,
    FileUploadComponent,
    ConversionHistoryComponent,
    FieldMappingComponent,
    ConversionProgressComponent,
    FhirPreviewComponent
  ]
})
export class ConversionModule { }
