import { Component, Input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './stat-card.component.html',
})
export class StatCardComponent {
  @Input({ required: true }) label = '';
  @Input({ required: true }) value = '';
  @Input() hint = '';
  @Input() icon?: string;
}
