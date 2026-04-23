import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LandingPage } from './pages/landing-page/landing-page';

const routes: Routes = [
  { path: 'landing', component: LandingPage },
  { path: '', redirectTo: 'landing', pathMatch: 'full' },
];

@NgModule({
  exports: [RouterModule],
  imports: [CommonModule, RouterModule.forChild(routes)],
})
export class DashboardModule {}
