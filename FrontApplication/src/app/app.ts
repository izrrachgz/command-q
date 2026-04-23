import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '@core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App {
  protected readonly title = signal('SpotMe');

  constructor(
    private titleService: Title,
    private auth: AuthService,
  ) {
    this.titleService.setTitle(this.title());

    this.auth.refresh().subscribe({
      next: (res) => this.auth.setAccessToken(res.accessToken),
      error: () => this.auth.logout(),
    });
  }
}
