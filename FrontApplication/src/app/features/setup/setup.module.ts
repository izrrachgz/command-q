import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SETUP_ROUTES } from './setup.routes';

@NgModule({
  imports: [RouterModule.forChild(SETUP_ROUTES)],
  exports: [RouterModule],
})
export class SetupModule {}
