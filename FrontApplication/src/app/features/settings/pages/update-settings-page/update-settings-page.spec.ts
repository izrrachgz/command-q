import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateSettingsPage } from './update-settings-page';

describe('UpdateSettingsPage', () => {
  let component: UpdateSettingsPage;
  let fixture: ComponentFixture<UpdateSettingsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateSettingsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateSettingsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
