import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { LucideDynamicIcon } from '@lucide/angular';

import { TextPipe } from '../../../../../core/i18n/text.pipe';
import { AccountDatePipe } from '../../../../../core/time/account-date.pipe';
import { EmptyStateComponent, PageHeaderComponent } from '../../../../../shared/components';
import { TextInputComponent } from '../../../../../shared/forms';
import {
  trackByEntityId,
  virtualTableBufferPixels,
  virtualTableViewportHeight,
  VirtualTableRowHeight,
} from '../../../../../shared/tables';
import { ApplicationConfigurationsActions } from '../../actions/application-configurations.actions';
import { ApplicationConfigurationsStore } from '../../stores/application-configurations.store';

@Component({
  selector: 'app-configuration-history-page',
  standalone: true,
  imports: [
    AccountDatePipe,
    AsyncPipe,
    EmptyStateComponent,
    LucideDynamicIcon,
    PageHeaderComponent,
    ReactiveFormsModule,
    ScrollingModule,
    TextInputComponent,
    TextPipe,
  ],
  templateUrl: './configuration-history.page.html',
})
export class ConfigurationHistoryPage {
  readonly store = inject(ApplicationConfigurationsStore);
  readonly trackByEntityId = trackByEntityId;
  readonly virtualTableBufferPixels = virtualTableBufferPixels;
  readonly virtualTableViewportHeight = virtualTableViewportHeight;
  readonly virtualTableRowHeight = VirtualTableRowHeight;

  private readonly actions = inject(ApplicationConfigurationsActions);
  private readonly fb = inject(FormBuilder);

  readonly filterForm = this.fb.nonNullable.group({ searchTerm: [''] });

  constructor() {
    this.load();
  }

  load(): void {
    this.actions.load().subscribe();
  }

  search(): void {
    this.actions.load(this.filterForm.controls.searchTerm.value.trim()).subscribe();
  }
}
