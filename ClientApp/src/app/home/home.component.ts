import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../services/http.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  form: FormGroup;
  
  constructor(
    private formBuilder: FormBuilder,
    private httpService: HttpService
  ){
    this.form = this.formBuilder.group({
      url: ['']
  });

  }
  ngOnInit(): void {
    this.form = this.formBuilder.group({
      url: ['']
    })
  }

  onSubmit(){

    if (this.form.invalid) {
      return;
    }

    this.httpService.ParseSite(this.form.controls.url.value)
    .pipe()
    .subscribe(
      data => {
        alert(data);
      }
    );  
  }
}
