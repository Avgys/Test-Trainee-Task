import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../services/http.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  crawlSiteForm: FormGroup;

  constructor(
    private httpService: HttpService
  ){    
    this.crawlSiteForm = new FormGroup({
      url: new FormControl('')
    });
  }

  ngOnInit(): void {
    this.crawlSiteForm = new FormGroup({
      url: new FormControl('')
    });
  }

  onCrawlSubmit(){

    // alert("2");
    if (this.crawlSiteForm.invalid) {
      return;
    }

    this.httpService.ParseSite(this.crawlSiteForm.controls.url.value)
    .pipe()
    .subscribe(
      data => {
        alert(data);
      }
    );  
  }    
}
