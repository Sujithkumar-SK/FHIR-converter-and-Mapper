import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FhirPreviewComponent } from './fhir-preview.component';

describe('FhirPreviewComponent', () => {
  let component: FhirPreviewComponent;
  let fixture: ComponentFixture<FhirPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FhirPreviewComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(FhirPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
