import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ManageGroupsPage } from './pages/manage-groups-page/manage-groups-page';

const routes: Routes = [
  { path: 'manage-groups', component: ManageGroupsPage },
  { path: '', redirectTo: 'landing', pathMatch: 'full' },
];

@NgModule({
  exports: [RouterModule],
  imports: [CommonModule, RouterModule.forChild(routes)],
})
export class GroupsModule {}
