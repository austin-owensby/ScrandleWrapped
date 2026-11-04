import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
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
  images = signal<HTMLImageElement[]>([]);

  daysPlayed = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.DaysPlayed,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  constructor() {
    effect(() => {
      const users = this.statisticsService.statistics()?.PlayerStatistics?.map(p => p.User) ?? [];

      untracked(async () => {
        const images = await Promise.all(users.map(user => this.load(user + ".png")));
        this.images.set(images);
      });
    });
  }

  private load(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = src;
    });
  }
}
