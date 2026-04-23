import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageGroupsPage } from './manage-groups-page';

describe('ManageGroupsPage', () => {
  let component: ManageGroupsPage;
  let fixture: ComponentFixture<ManageGroupsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageGroupsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageGroupsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
