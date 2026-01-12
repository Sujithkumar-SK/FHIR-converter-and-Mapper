export interface FieldMapping {
  csvColumn: string;
  fhirField: string;
  isRequired: boolean;
}

export interface StartConversionRequest {
  fileId: string;
  requestId?: string;
  fieldMappings: FieldMapping[];
}

export interface ConversionStatus {
  jobId: string;
  status: string;
  progress: number;
  errorMessage?: string;
  patientsProcessed: number;
  observationsProcessed: number;
  completedAt?: Date;
  processingTimeMs?: number;
}

export interface DetectedField {
  columnName: string;
  suggestedFhirField: string;
  confidenceScore: number;
  sampleValues: string[];
}

export interface FieldDetectionResponse {
  fileId: string;
  detectedFields: DetectedField[];
  requiredMappings: string[];
  availableFhirFields: string[];
}

export interface FhirBundlePreview {
  jobId: string;
  bundleId: string;
  patientCount: number;
  observationCount: number;
  patientSample: string[];
  observationSample: string[];
}

export const FhirFieldLabels: Record<string, string> = {
  'patient.identifier': 'Patient ID',
  'patient.name.given': 'First Name',
  'patient.name.family': 'Last Name',
  'patient.birthDate': 'Date of Birth',
  'patient.gender': 'Gender',
  'observation.code': 'Test/Lab Name (mapped to LOINC)',
  'observation.valueQuantity.value': 'Test Result Value',
  'observation.valueQuantity.unit': 'Unit of Measure (mapped to UCUM)',
  'observation.effectiveDateTime': 'Test Date'
};

export const ConversionStatusLabels: Record<string, string> = {
  'Processing': 'Processing',
  'Completed': 'Completed',
  'Failed': 'Failed'
};