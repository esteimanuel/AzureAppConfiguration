import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl } from '@angular/forms';
import { AppConfigService } from '../app-config.service';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
  styleUrls: ['./features.component.css']
})
export class FeaturesComponent implements OnInit, OnDestroy {
  isLoaded = false;
  featuresForm = this.formBuilder.group({
    features: this.formBuilder.array([])
  });

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private formBuilder: FormBuilder, private appConfig: AppConfigService) {
    this.getFeatures();
  }

  async ngOnInit() {
    this.appConfig.startConnection();
    this.appConfig
      .onConfigChanged()
      .subscribe(() => this.getFeatures());
  }
  
  ngOnDestroy(): void {
    this.appConfig.closeConnection();
  }

  updateValues(features: Feature[]) {
    this.featuresForm = this.formBuilder.group({
      features: this.formBuilder.array(features)
    });
  }

  getFeatures() {
    this.isLoaded = false;
    return this.http.get<Feature[]>(this.baseUrl + 'api/features').subscribe(result => {
      console.log('getFeatures', result)
      this.updateValues(result);
      this.isLoaded = true;
    }, error => console.error(error));
  }

  get features() {
    return this.featuresForm.controls['features'] as FormArray
  }
}

interface Feature {
  name: string;
  isEnabled: boolean;
}
