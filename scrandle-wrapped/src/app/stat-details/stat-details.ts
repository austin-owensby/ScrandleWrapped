import { Component, inject } from '@angular/core';
import { StatisticsService } from '../statistics.service';

@Component({
  imports: [],
  selector: 'app-stat-details',
  styleUrl: './stat-details.css',
  templateUrl: './stat-details.html',
})
export class StatDetails {
  statisticsService = inject(StatisticsService);
}
