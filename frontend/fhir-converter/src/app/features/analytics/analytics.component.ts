import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, SystemOverview, ConversionStatistics } from '../../core/services/analytics.service';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html',
  styleUrl: './analytics.component.css'
})
export class AnalyticsComponent implements OnInit {
  systemOverview?: SystemOverview;
  conversionStats?: ConversionStatistics;
  loading = true;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getSystemOverview().subscribe({
      next: (data) => {
        this.systemOverview = data;
        this.loadConversionStats();
      },
      error: () => this.loading = false
    });
  }

  private loadConversionStats(): void {
    this.analyticsService.getConversionStatistics().subscribe({
      next: (data) => {
        this.conversionStats = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
