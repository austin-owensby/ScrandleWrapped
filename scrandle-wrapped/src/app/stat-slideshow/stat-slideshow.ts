import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { StatisticsService } from '../statistics.service';
import { Subscription, tap, timer } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';

@Component({
  imports: [DecimalPipe, DatePipe],
  selector: 'app-stat-slideshow',
  styleUrl: './stat-slideshow.css',
  templateUrl: './stat-slideshow.html',
})
export class StatSlideshow {
  destroyRef = inject(DestroyRef);
  router = inject(Router);
  statisticsService = inject(StatisticsService);
  subscription: Subscription | null = null;
  progressSubscription: Subscription | null = null;

  user = this.statisticsService.user;
  statistics = this.statisticsService.statistics;

  userStatistics = computed(() => {
    return this.statistics()?.PlayerStatistics?.find(p => p.User == this.user());
  });

  isLastPage = computed(() => {
    return this.pageIndex() + 1 == this.pageCount;
  });

  pageIndex = signal(0);

  readonly pageCount = 13;
  delay = 5000;
  progress = signal(0);

  constructor() {
    this.autoAdvancePages();
  }

  nextPage = () => {
    if (this.pageIndex() < this.pageCount - 1) {
      this.pageIndex.update(current => current + 1);
      this.delay = this.pageIndex() == 10 || this.pageIndex() == 11 ? 10000 : 5000;
      this.autoAdvancePages();
    }
    else {
      this.subscription?.unsubscribe();
      this.progressSubscription?.unsubscribe();
      this.progress.set(1);
    }
  };

  backPage = () => {
    if (this.pageIndex() > 0) {
      this.pageIndex.update(current => current - 1);
      this.delay = this.pageIndex() == 10 || this.pageIndex() == 11 ? 10000 : 5000;
      this.autoAdvancePages();
    }
  };

  autoAdvancePages = () => {
    // When we change pages manually, restart our timer
    this.subscription?.unsubscribe();
    this.progressSubscription?.unsubscribe();
    this.progress.set(0);

    this.subscription = timer(this.delay, this.delay).pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.nextPage();
      })
    ).subscribe();

    this.progressSubscription = timer(0, 100).pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.progress.set(Math.min(1, this.progress() + 100 / this.delay));
      })
    ).subscribe();
  };

  goToStatistics = () => {
    this.router.navigate(['statistics']);
  }
}
