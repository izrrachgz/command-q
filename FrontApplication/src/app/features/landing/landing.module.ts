import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { LANDING_ROUTES } from './landing.routes';

@NgModule({
  imports: [RouterModule.forChild(LANDING_ROUTES)],
  exports: [RouterModule],
})
export class LandingModule {}
