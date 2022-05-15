import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
  styleUrls: ['./features.component.css']
})
export class FeaturesComponent {
  isLoaded = false;
  public featuresForm = this.formBuilder.group({
    backgroundColor: '',
    fontColor: '',
    fontSize: 0,
    message: ''
  });

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private formBuilder: FormBuilder) {
    http.get<Features>(baseUrl + 'api/features').subscribe(result => {
      this.featuresForm = this.formBuilder.group(result);
      this.isLoaded = true;
    }, error => console.error(error));
  }
}

interface Features {
  backgroundColor: string;
  fontColor: string;
  fontSize: number;
  message: string;
}
