import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { StatisticsService } from '../statistics.service';
import { ImageBarChart, ImageBarItem } from '../image-bar-chart/image-bar-chart';
import { FormsModule } from '@angular/forms';
import { BarChart, BarItem } from '../bar-chart/bar-chart';
import { DecimalPipe } from '@angular/common';

@Component({
  imports: [ImageBarChart, FormsModule, BarChart, DecimalPipe],
  selector: 'app-stat-details',
  styleUrl: './stat-details.css',
  templateUrl: './stat-details.html',
})
export class StatDetails {
  statisticsService = inject(StatisticsService);
  statistics = this.statisticsService.statistics;
  user = this.statisticsService.user;
  images = signal<HTMLImageElement[]>([]);

  percentages = signal<boolean>(true);

  daysPlayed = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.DaysPlayed,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  playerAverage = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.Average,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  bestStreak = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.BestStreak,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  bestPerfectStreak = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.BestPerfectStreak,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  beatCapsherAverage = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: this.percentages() ? p.BeatCapsherAverage / p.DaysPlayed : p.BeatCapsherAverage,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  beatCommunityAverage = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: this.percentages() ? p.BeatCommunityCount / p.DaysPlayed : p.BeatCommunityCount,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  beatScrandlebot = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: this.percentages() ? p.BeatScrandlebotCount / p.DaysPlayed : p.BeatScrandlebotCount,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  blewItCount = computed(() => {
    return (this.statistics()?.PlayerStatistics.map(p => ({
      label: p.User,
      value: p.BlewItCount,
      img: p.User + ".png"
    } as ImageBarItem)) ?? []).sort((a, b) => a.value - b.value);
  });

  scoreDistribution = computed(() => {
    const scoreFrequency = this.statistics()?.PlayerStatistics.find(p => p.User == this.statisticsService.user())?.ScoreFrequency ?? {};

    return Object.keys(scoreFrequency).map(key => ({
      label: key,
      value: scoreFrequency[key]
    }) as BarItem);
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

  private load = (src: string): Promise<HTMLImageElement> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = src;
    });
  }

  downloadStatistics = () => {
    this.downloadFile('statistics.json', 'statistics.json');
  }

  downloadRawData = () => {
    this.downloadFile('slack_messages.json', 'raw_data.json');
  }

  private downloadFile = (url: string, filename: string) => {
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }
}
