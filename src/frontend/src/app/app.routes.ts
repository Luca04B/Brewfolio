import { Routes } from '@angular/router';
const protectDirtyForm = (component: { canDeactivate(): boolean | Promise<boolean> }) =>
  component.canDeactivate();

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'coffee-beans',
  },
  {
    path: 'coffee-beans',
    loadComponent: () =>
      import('./coffee-beans/coffee-beans-page').then((module) => module.CoffeeBeansPage),
  },
  {
    path: 'coffee-beans/new',
    loadComponent: () =>
      import('./coffee-beans/coffee-bean-create-page').then(
        (module) => module.CoffeeBeanCreatePage,
      ),
    canDeactivate: [protectDirtyForm],
  },
  {
    path: 'coffee-beans/:id',
    loadComponent: () =>
      import('./coffee-beans/coffee-bean-detail-page').then(
        (module) => module.CoffeeBeanDetailPage,
      ),
    canDeactivate: [protectDirtyForm],
  },
];
