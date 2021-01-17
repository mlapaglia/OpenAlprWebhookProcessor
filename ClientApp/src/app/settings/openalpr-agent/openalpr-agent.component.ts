import { Component, Input, OnInit } from '@angular/core';
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

  constructor(private settingsService: SettingsService) { }

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

  private getAgent() {
    this.settingsService.getAgent().subscribe(result => {
      this.agent = result;
    });
  }
}
