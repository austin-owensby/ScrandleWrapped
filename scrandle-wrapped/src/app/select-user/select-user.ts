import { Component, computed, inject } from '@angular/core';
import { StatisticsService } from '../statistics.service';

@Component({
  imports: [],
  selector: 'app-select-user',
  styleUrl: './select-user.css',
  templateUrl: './select-user.html',
})
export class SelectUser {
  statisticsService = inject(StatisticsService);

  users = computed(() => {
    return this.statisticsService.statistics()?.PlayerStatistics?.filter(p => !p.DeletedUser)?.map(p => p.User) ?? [];
  });
}
