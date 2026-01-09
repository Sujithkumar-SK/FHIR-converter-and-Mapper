import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilesRoutingModule } from './files-routing.module';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { FileListComponent } from './components/file-list/file-list.component';
import { FilePreviewComponent } from './components/file-preview/file-preview.component';
import { FilesPageComponent } from './files-page.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FilesRoutingModule,
    FileUploadComponent,
    FileListComponent,
    FilePreviewComponent,
    FilesPageComponent
  ],
  exports: [
    FileUploadComponent,
    FileListComponent,
    FilePreviewComponent,
    FilesPageComponent
  ]
})
export class FilesModule { }
