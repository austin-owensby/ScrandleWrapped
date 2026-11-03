import { Component, DestroyRef, inject, signal } from '@angular/core';
import { StatisticsService } from '../statistics.service';
import { Subscription, tap, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';

@Component({
  imports: [],
  selector: 'app-stat-slideshow',
  styleUrl: './stat-slideshow.css',
  templateUrl: './stat-slideshow.html',
})
export class StatSlideshow {
  destroyRef = inject(DestroyRef);
  router = inject(Router);
  statisticsService = inject(StatisticsService);
  subscription: Subscription | null = null;

  user = this.statisticsService.user;

  pageIndex = signal(0);

  readonly pageCount = 13;
  readonly delay = 5000;

  constructor() {
    this.autoAdvancePages();
  }

  nextPage = () => {
    if (this.pageIndex() < this.pageCount - 1) {
      this.pageIndex.update(current => current + 1);
      this.autoAdvancePages();
    }
    else {
      this.subscription?.unsubscribe();
    }
  };

  backPage = () => {
    if (this.pageIndex() > 0) {
      this.pageIndex.update(current => current - 1);
      this.autoAdvancePages();
    }
  };

  autoAdvancePages = () => {
    // When we change pages manually, restart our timer
    this.subscription?.unsubscribe();

    this.subscription = timer(this.delay, this.delay).pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.nextPage();
      })
    ).subscribe();
  };

  goToStatistics = () => {
    this.router.navigate(['statistics']);
  }
}
