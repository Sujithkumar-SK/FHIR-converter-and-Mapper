import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataRequestDetailsComponent } from './data-request-details.component';

describe('DataRequestDetailsComponent', () => {
  let component: DataRequestDetailsComponent;
  let fixture: ComponentFixture<DataRequestDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataRequestDetailsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DataRequestDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
