import { Component, OnInit } from '@angular/core';
import { SnackbarService } from '@app/snackbar/snackbar.service';
import { SnackBarType } from '@app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import { Agent } from './agent';

@Component({
  selector: 'app-openalpr-agent',
  templateUrl: './openalpr-agent.component.html',
  styleUrls: ['./openalpr-agent.component.less']
})
export class OpenalprAgentComponent implements OnInit {
  public agent: Agent;
  public isSaving: boolean = false;
  public isHydrating: boolean = false;

  constructor(
    private settingsService: SettingsService,
    private snackBarService: SnackbarService) { }

  ngOnInit(): void {
    this.getAgent();
  }

  public saveAgent() {
    this.isSaving = true;

    this.settingsService.upsertAgent(this.agent).subscribe(result => {
      this.isSaving = false;
      this.getAgent();
    });
  }

  public scrapeAgent() {
    this.isHydrating = true;

    this.settingsService.startAgentScrape().subscribe(_ => {
      this.isHydrating = false;
      this.snackBarService.create("Agent Scraping has begun, check system logs for progress", SnackBarType.Info);
    });
  }

  private getAgent() {
    this.settingsService.getAgent().subscribe(result => {
      this.agent = result;
    });
  }
}
