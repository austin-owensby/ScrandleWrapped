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
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true } },
  };

  constructor() {
    effect(() => {
      const items = this.items();

      untracked(() => {
        this.data = {
          labels: items.map(i => i.label),
          datasets: [{
            data: items.map(i => i.value)
          }],
        };
      });
    });
  }
}
