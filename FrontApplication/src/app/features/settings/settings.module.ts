import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UpdateSettingsPage } from './pages/update-settings-page/update-settings-page';

const routes: Routes = [
  { path: 'update-settings', component: UpdateSettingsPage },
  { path: '', redirectTo: 'landing', pathMatch: 'full' },
];

@NgModule({
  exports: [RouterModule],
  imports: [CommonModule, RouterModule.forChild(routes)],
})
export class SettingsModule {}
