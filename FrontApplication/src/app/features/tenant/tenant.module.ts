import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { TENANT_ROUTES } from './tenant.routes';

@NgModule({
  imports: [RouterModule.forChild(TENANT_ROUTES)],
  exports: [RouterModule],
})
export class TenantModule {}
