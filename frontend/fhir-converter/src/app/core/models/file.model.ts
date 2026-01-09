export interface FileUploadResponse {
  fileId: string;
  originalFileName: string;
  fileSizeBytes: number;
  detectedFormat: InputFormat;
  status: ConversionStatus;
  uploadedAt: Date;
  expiresAt: Date;
  requestId?: string;
}

export interface FileValidationResult {
  fileId: string;
  isValid: boolean;
  detectedFormat: InputFormat;
  validationErrors: string[];
  recordCount: number;
  previewRecords: string[];
}

export interface FilePreviewResponse {
  fileId: string;
  originalFileName: string;
  format: InputFormat;
  totalRecords: number;
  headers: string[];
  previewData: Record<string, any>[];
}

export enum InputFormat {
  CSV = 1,
  JSON = 2,
  CCDA = 3
}

export enum ConversionStatus {
  Processing = 1,
  Completed = 2,
  Failed = 3
}