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
  statisticsService = inject(StatisticsService);

  items = input.required<ImageBarItem[]>();
  images = input.required<HTMLImageElement[]>();
  percent = input<boolean>(false);
  min = input<number>();
  max = input<number>();
  step = input<number>();
  user = this.statisticsService.user;

  data: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [],
  };

  options: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    layout: { padding: { top: 165, right: 12 } },
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
        min: this.min(),
        max: this.max() ?? 1,
        grace: '10%',
        grid: { color: 'rgba(32, 27, 49, .12)', tickLength: 0 },
        ticks: {
          color: '#695b72',
          padding: 8,
          font: { family: 'DM Sans', weight: 700 },
          precision: 0,
          stepSize: this.step() ?? 1,
          format: { style: 'percent' },
        },
      },
    },
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
            borderWidth: 2,
            borderColor: '#201b31',
            borderRadius: 5,
            borderSkipped: false,
            backgroundColor: items.map((i, index) => i.label == this.user() ? '#f04e3e' : ['#d8d1dc', '#c8c0ce', '#b8adbf', '#a79ab0', '#96889f'][index % 5])
          }],
        };
      });
    });

    effect(() => {
      const images = this.images();

      untracked(() => {
        this.plugins = [this.createBarImagesPlugin(images)];
        this.ready.set(true); // render the chart only once images are available
      });
    });

    effect(() => {
      const percent = this.percent();
      const min = this.min();
      const max = this.max();
      const step = this.step();

      untracked(() => {
        this.options = {
          ...this.options,
          scales: {
            x: {
              grid: { display: false },
              ticks: { color: '#695b72', font: { family: 'DM Sans', weight: 700 } }
            },
            y: {
              beginAtZero: true,
              min,
              max: percent ? (max ?? 1) : max,
              grace: '10%',
              grid: { color: 'rgba(32, 27, 49, .12)', tickLength: 0 },
              ticks: {
                color: '#695b72',
                padding: 8,
                font: { family: 'DM Sans', weight: 700 },
                callback: (value: string | number) => percent
                  ? `${Math.round(Number(value) * 100)}%`
                  : `${Math.round(Number(value))}`
              },
              afterBuildTicks: (scale) => {
                const lowerBound = min ?? 0;
                const upperBound = max ?? scale.max;
                const interval = step ?? (percent ? 0.2 : 1);
                const tickValues: number[] = [];

                for (let value = lowerBound; value <= upperBound + interval / 10; value += interval) {
                  tickValues.push(Number(value.toFixed(10)));
                }

                scale.ticks = tickValues.map(value => ({ value }));
              }
            }
          }
        };
      });
    });
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

          // Give each avatar enough room to remain recognizable above its bar.
          const drawW = Math.min(width * 1.65, (chart.chartArea.width / this.data.labels!.length) * 1.2);
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
