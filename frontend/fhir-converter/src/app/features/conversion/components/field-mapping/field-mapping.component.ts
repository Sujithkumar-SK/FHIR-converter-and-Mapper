import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConversionService } from '../../../../core/services/conversion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { 
  DetectedField, 
  FieldMapping, 
  FhirFieldLabels 
} from '../../../../core/models/conversion.model';

@Component({
  selector: 'app-field-mapping',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './field-mapping.component.html',
  styleUrl: './field-mapping.component.css'
})
export class FieldMappingComponent implements OnInit {
  @Input() fileId!: string;
  @Output() mappingComplete = new EventEmitter<FieldMapping[]>();
  
  detectedFields: DetectedField[] = [];
  availableFhirFields: string[] = [];
  requiredMappings: string[] = [];
  fieldMappings: FieldMapping[] = [];
  isLoading = false;
  
  readonly FhirFieldLabels = FhirFieldLabels;

  constructor(
    private conversionService: ConversionService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadFieldDetection();
  }

  loadFieldDetection(): void {
    this.isLoading = true;
    
    this.conversionService.detectFields(this.fileId).subscribe({
      next: (response) => {
        this.detectedFields = response.detectedFields;
        this.availableFhirFields = response.availableFhirFields;
        this.requiredMappings = response.requiredMappings;
        
        // Initialize mappings with detected suggestions
        this.fieldMappings = this.detectedFields.map(field => ({
          csvColumn: field.columnName,
          fhirField: field.suggestedFhirField,
          isRequired: this.requiredMappings.includes(field.suggestedFhirField)
        }));
        
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to detect fields');
        this.isLoading = false;
      }
    });
  }

  updateMapping(csvColumn: string, fhirField: string): void {
    const mapping = this.fieldMappings.find(m => m.csvColumn === csvColumn);
    if (mapping) {
      mapping.fhirField = fhirField;
      mapping.isRequired = this.requiredMappings.includes(fhirField);
    }
  }

  onSelectChange(event: Event, csvColumn: string): void {
    const target = event.target as HTMLSelectElement;
    this.updateMapping(csvColumn, target.value);
  }

  getRequiredFieldsText(): string {
    return this.requiredMappings.map(f => this.FhirFieldLabels[f] || f).join(', ');
  }

  validateMappings(): boolean {
    const mappedRequiredFields = this.fieldMappings
      .filter(m => m.fhirField && this.requiredMappings.includes(m.fhirField))
      .map(m => m.fhirField);
    
    const missingRequired = this.requiredMappings.filter(req => 
      !mappedRequiredFields.includes(req)
    );
    
    if (missingRequired.length > 0) {
      this.notificationService.showError(
        `Missing required mappings: ${missingRequired.map(f => FhirFieldLabels[f]).join(', ')}`
      );
      return false;
    }
    
    return true;
  }

  confirmMappings(): void {
    if (this.validateMappings()) {
      const validMappings = this.fieldMappings.filter(m => m.fhirField);
      this.mappingComplete.emit(validMappings);
    }
  }

  getConfidenceColor(score: number): string {
    if (score >= 0.8) return 'text-success';
    if (score >= 0.6) return 'text-warning';
    return 'text-danger';
  }

  getConfidenceText(score: number): string {
    if (score >= 0.8) return 'High';
    if (score >= 0.6) return 'Medium';
    return 'Low';
  }
}
