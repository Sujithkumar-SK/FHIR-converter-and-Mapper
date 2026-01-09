import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConversionProgressComponent } from './conversion-progress.component';

describe('ConversionProgressComponent', () => {
  let component: ConversionProgressComponent;
  let fixture: ComponentFixture<ConversionProgressComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConversionProgressComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ConversionProgressComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
