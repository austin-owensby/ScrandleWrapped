import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import {
  Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip,
  ChartConfiguration, Plugin,
} from 'chart.js';
import { StatisticsService } from '../statistics.service';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip);

export interface ImageBarItem { label: string; value: number; img: string }

@Component({
  imports: [BaseChartDirective],
  selector: 'app-image-bar-chart',
  styleUrl: './image-bar-chart.css',
  templateUrl: './image-bar-chart.html',
})
export class ImageBarChart {
  ready = signal(false);
  statisticsService = inject(StatisticsService)

  items = input.required<ImageBarItem[]>();
  images = input.required<HTMLImageElement[]>();
  user = this.statisticsService.user;

  data: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [],
  };

  options: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    layout: { padding: { top: 120 } },
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, grace: '10%' } },
  };

  plugins: Plugin<'bar'>[] = [];

  constructor() {
    effect(() => {
      const items = this.items();

      untracked(() => {
        this.data = {
          labels: items.map(i => i.label),
          datasets: [{
            data: items.map(i => i.value),
            borderWidth: 1,
            backgroundColor: items.map(i => i.label == this.user() ? 'red' : undefined)
          }],
        };
      });
    })

    effect(() => {
      const images = this.images();

      untracked(() => {
        this.plugins = [this.createBarImagesPlugin(images)];
        this.ready.set(true); // render the chart only once images are available
      });
    })
  }

  private createBarImagesPlugin(images: HTMLImageElement[]): Plugin<'bar'> {
    const GAP = 6; // gap between bar end and image

    return {
      id: 'barImages',
      afterDatasetsDraw: (chart) => {
        const { ctx } = chart;
        chart.getDatasetMeta(0).data.forEach((bar, i) => {
          const img = images.find(img => img.src.replaceAll("%20", " ").endsWith(this.data.labels![i] + ".png"));
          if (!img) return;

          const { x, y, base, width } = bar.getProps(['x', 'y', 'base', 'width'], true) as
            { x: number; y: number; base: number; width: number };
          const isNegative = y > base;

          // Full image scaled to the bar's width, aspect ratio preserved
          const drawW = width;
          const drawH = drawW * (img.naturalHeight / img.naturalWidth);
          const drawX = x - drawW / 2;

          // Above the bar's end (below it for negative values)
          const drawY = isNegative ? Math.max(y, base) + GAP : Math.min(y, base) - GAP - drawH;

          ctx.drawImage(img, drawX, drawY, drawW, drawH);
        });
      },
    };
  }
}
