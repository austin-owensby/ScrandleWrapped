import { Routes } from '@angular/router';
import { SelectUser } from './select-user/select-user';
import { StatSlideshow } from './stat-slideshow/stat-slideshow';
import { StatDetails } from './stat-details/stat-details';

export const routes: Routes = [
    {
        path: '',
        component: SelectUser
    },
    {
        path: 'slideshow',
        component: StatSlideshow
    },
    {
        path: 'statistics',
        component: StatDetails
    }
];
