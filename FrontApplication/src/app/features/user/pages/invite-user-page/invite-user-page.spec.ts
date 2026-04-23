import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InviteUserPage } from './invite-user-page';

describe('InviteUserPage', () => {
  let component: InviteUserPage;
  let fixture: ComponentFixture<InviteUserPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InviteUserPage],
    }).compileComponents();

    fixture = TestBed.createComponent(InviteUserPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
