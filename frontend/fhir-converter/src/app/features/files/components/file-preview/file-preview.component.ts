import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../../../../core/services/file.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FilePreviewResponse, InputFormat } from '../../../../core/models/file.model';

@Component({
  selector: 'app-file-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-preview.component.html',
  styleUrl: './file-preview.component.css'
})
export class FilePreviewComponent implements OnInit {
  @Input() fileId!: string;
  
  previewData: FilePreviewResponse | null = null;
  isLoading = false;
  error: string | null = null;
  
  readonly InputFormat = InputFormat;

  constructor(
    private fileService: FileService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    if (this.fileId) {
      this.loadPreview();
    }
  }

  loadPreview(): void {
    this.isLoading = true;
    this.error = null;
    
    this.fileService.getFilePreview(this.fileId).subscribe({
      next: (data) => {
        this.previewData = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to load preview';
        this.isLoading = false;
        this.notificationService.showError(this.error);
      }
    });
  }

  getFormatText(format: InputFormat): string {
    switch (format) {
      case InputFormat.CSV: return 'CSV';
      case InputFormat.JSON: return 'JSON';
      case InputFormat.CCDA: return 'CCDA/XML';
      default: return 'Unknown';
    }
  }

  getFormatIcon(format: InputFormat): string {
    switch (format) {
      case InputFormat.CSV: return '📊';
      case InputFormat.JSON: return '📄';
      case InputFormat.CCDA: return '📋';
      default: return '📁';
    }
  }

  isTableFormat(): boolean {
    return this.previewData?.format === InputFormat.CSV;
  }

  getTableHeaders(): string[] {
    return this.previewData?.headers || [];
  }

  getTableRows(): Record<string, any>[] {
    return this.previewData?.previewData || [];
  }

  getJsonPreview(): string {
    if (this.previewData?.previewData && this.previewData.previewData.length > 0) {
      return JSON.stringify(this.previewData.previewData[0], null, 2);
    }
    return '';
  }

  getXmlPreview(): string {
    if (this.previewData?.previewData && this.previewData.previewData.length > 0) {
      return JSON.stringify(this.previewData.previewData[0], null, 2);
    }
    return '';
  }
}
