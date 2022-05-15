import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl } from '@angular/forms';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
  styleUrls: ['./features.component.css']
})
export class FeaturesComponent {
  isLoaded = false;
  featuresForm = this.formBuilder.group({
    features: this.formBuilder.array([])
  });

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private formBuilder: FormBuilder) {
    http.get<Feature[]>(baseUrl + 'api/features').subscribe(result => {
      
      this.featuresForm = this.formBuilder.group({
        features: this.formBuilder.array(result.map(x => this.formBuilder.control(x)))
      });
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
