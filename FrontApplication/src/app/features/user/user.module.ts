import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InviteUserPage } from './pages/invite-user-page/invite-user-page';

const routes: Routes = [
  { path: 'invite-user', component: InviteUserPage },
  { path: '', redirectTo: 'landing', pathMatch: 'full' },
];

@NgModule({
  exports: [RouterModule],
  imports: [CommonModule, RouterModule.forChild(routes)],
})
export class UserModule {}
