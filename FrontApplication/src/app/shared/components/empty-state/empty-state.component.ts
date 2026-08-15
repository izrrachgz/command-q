import { Component, Input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './empty-state.component.html',
})
export class EmptyStateComponent {
  @Input({ required: true }) title = '';
  @Input() message = '';
  @Input() icon?: string;
}
