import { Component, computed, inject } from '@angular/core';
import { StatisticsService } from '../statistics.service';
import { ImageBarChart, ImageBarItem } from '../image-bar-chart/image-bar-chart';

@Component({
  imports: [ImageBarChart],
  selector: 'app-stat-details',
  styleUrl: './stat-details.css',
  templateUrl: './stat-details.html',
})
export class StatDetails {
  statisticsService = inject(StatisticsService);
  statistics = this.statisticsService.statistics;

  daysPlayed = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.DaysPlayed,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });
}
