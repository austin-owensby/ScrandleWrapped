import { Component, computed, inject } from '@angular/core';
import { StatisticsService } from '../statistics.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  imports: [FormsModule],
  selector: 'app-select-user',
  styleUrl: './select-user.css',
  templateUrl: './select-user.html',
})
export class SelectUser {
  statisticsService = inject(StatisticsService);
  router = inject(Router);

  selectedUser = this.statisticsService.user();

  users = computed(() => {
    return this.statisticsService.statistics()?.PlayerStatistics?.filter(p => !p.DeletedUser)?.map(p => p.User) ?? [];
  });

  selectUser = () => {
    if (this.selectedUser) {
      this.statisticsService.selectUser(this.selectedUser);
      this.router.navigate(['slideshow']);
    }
  }
}
