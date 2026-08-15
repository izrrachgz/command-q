import { Component, Input } from '@angular/core';

import { BrandAssets } from '../../../core/branding/brand-assets';

export type BrandMarkSize = 'sm' | 'md' | 'lg' | 'xl';

@Component({
  selector: 'app-brand-mark',
  standalone: true,
  templateUrl: './brand-mark.component.html',
})
export class BrandMarkComponent {
  @Input() size: BrandMarkSize = 'md';

  readonly logo = BrandAssets.robotLogo;
}
