import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  isLoaded = false;
  public settingsForm = this.formBuilder.group({
    backgroundColor: { value: '', disabled: true },
    fontColor: { value: '', disabled: true },
    fontSize: { value: 0, disabled: true },
    message: { value: '', disabled: true }
  });

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private formBuilder: FormBuilder) {
    http.get<Settings>(baseUrl + 'api/settings').subscribe(result => {
      this.settingsForm = this.formBuilder.group(result);
      this.isLoaded = true;
    }, error => console.error(error));
  }
}

interface Settings {
  backgroundColor: string;
  fontColor: string;
  fontSize: number;
  message: string;
}
