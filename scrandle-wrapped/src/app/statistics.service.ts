import { HttpClient } from '@angular/common/http';
import { inject, Service, signal } from '@angular/core';

@Service()
export class StatisticsService {
    private _statistics = signal<Statistics | null>(null);
    readonly statistics = this._statistics.asReadonly();

    private http = inject(HttpClient);

    loadData = () => {
        this.http.get<Statistics>('/assets/statistics.json').subscribe({
            next: (res) => {
                this._statistics.set(res);
            },
            error: (err) => console.error('Error loading statistics', err)
        });
    }
}

export interface Statistics {
    PlayerStatistics: PlayerStatistics[];
    DaysPlayed: number;
    CapsherAverage: number;
    CommunityAverage: number;
    ScrandlebotAverage: number;
    CapsherAveragePerDay: { [date: string]: number }[];
    CommunityAveragePerDay: { [date: string]: number }[];
    DaysCapsherBeatCommunity: number;
    DaysCapsherBeatScrandlebot: number;
    CapsherHardestDay: string;
    CapsherEasiestDay: string;
    CommunityHardestDay: string;
    CommunityEasiestDay: string;
}

export interface PlayerStatistics {
    DeletedUser: boolean;
    User: string;
    DaysPlayed: number;
    Average: number;
    BestStreak: number;
    BestPerfectStreak: number;
    BeatCapsherAverage: number;
    BeatCommunityCount: number;
    BeatScrandlebotCount: number;
    BlewItCount: number;
    BestDay: number;
    WorstDay: number;
    BestMatchup: MatchupStatistic;
    WorstMatchup: MatchupStatistic;
    ScoreFrequency: { [score: number]: number }[];
}

export interface MatchupStatistic {
    Day: string;
    ItemIndex: number;
    Difficulty: number;
}