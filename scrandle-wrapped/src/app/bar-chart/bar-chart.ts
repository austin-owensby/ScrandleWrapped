import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import {
  Chart, CategoryScale, LinearScale, Tooltip,
  ChartConfiguration,
  BarController,
  BarElement
} from 'chart.js';
import { StatisticsService } from '../statistics.service';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip);

export interface BarItem { label: string; value: number }

@Component({
  imports: [BaseChartDirective],
  selector: 'app-bar-chart',
  styleUrl: './bar-chart.css',
  templateUrl: './bar-chart.html',
})
export class BarChart {
  ready = signal(false);
  statisticsService = inject(StatisticsService);

  items = input.required<BarItem[]>();

  data: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [],
  };

  options: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: '#201b31',
        titleFont: { family: 'DM Sans', weight: 700 },
        bodyFont: { family: 'DM Sans' },
        displayColors: false,
        padding: 12,
      },
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: { color: '#695b72', font: { family: 'DM Sans', weight: 700 } },
      },
      y: {
        beginAtZero: true,
        grid: { color: 'rgba(32, 27, 49, .12)', tickLength: 0 },
        ticks: {
          color: '#695b72',
          padding: 8,
          font: { family: 'DM Sans', weight: 700 },
          precision: 0,
          stepSize: 1,
        },
        afterBuildTicks: (scale) => {
          const upperBound = Math.ceil(scale.max / 5) * 5;
          scale.ticks = Array.from(
            { length: upperBound / 5 + 1 },
            (_, index) => ({ value: index * 5 })
          );
        },
      },
    },
  };

  constructor() {
    effect(() => {
      const items = this.items();

      untracked(() => {
        this.data = {
          labels: items.map(i => i.label),
          datasets: [{
            data: items.map(i => i.value),
            backgroundColor: items.map(item => this.scoreColor(item.label)),
            borderColor: '#201b31',
            borderWidth: 2,
            borderRadius: 5,
            borderSkipped: false,
          }],
        };
      });
    });
  }

  private scoreColor(label: string): string {
    const score = Math.max(0, Math.min(10, Number(label)));
    const red = { r: 201, g: 120, b: 120 };
    const green = { r: 127, g: 181, b: 138 };
    const progress = score / 10;
    const channel = (start: number, end: number) => Math.round(start + (end - start) * progress);

    return `rgb(${channel(red.r, green.r)}, ${channel(red.g, green.g)}, ${channel(red.b, green.b)})`;
  }
}
