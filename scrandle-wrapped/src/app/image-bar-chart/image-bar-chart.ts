import { Component, effect, input, OnInit, signal, untracked } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import {
  Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip,
  ChartConfiguration, Plugin,
} from 'chart.js';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip);

export interface ImageBarItem { label: string; value: number; img: string }

@Component({
  imports: [BaseChartDirective],
  selector: 'app-image-bar-chart',
  styleUrl: './image-bar-chart.css',
  templateUrl: './image-bar-chart.html',
})
export class ImageBarChart implements OnInit {
  ready = signal(false);

  items = input.required<ImageBarItem[]>();

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
            // TODO highlight the current user's bar
          }],
        };
      });
    })
  }

  async ngOnInit() {
    // TODO, load all the images from the parent component and pass them in
    // TODO, does this solve the issue where images doesn't appear on page refresh?
    const images = await Promise.all(this.items().map(i => this.load(i.img)));
    this.plugins = [this.createBarImagesPlugin(images)];
    this.ready.set(true); // render the chart only once images are available
  }

  private createBarImagesPlugin(images: HTMLImageElement[]): Plugin<'bar'> {
    const GAP = 6; // gap between bar end and image

    return {
      id: 'barImages',
      afterDatasetsDraw: (chart) => {
        const { ctx } = chart;
        chart.getDatasetMeta(0).data.forEach((bar, i) => {
          const img = images[i];
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

  private load(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve(img);
      img.onerror = reject;
      img.src = src;
    });
  }
}
