import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Site } from '../models/Site';
import { HttpService } from '../services/http.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  crawlSiteForm: FormGroup;
  findWordForm: FormGroup;
  findEntityForm: FormGroup;
  findAttrForm: FormGroup;
  sites: Site[];

  constructor(
    private httpService: HttpService
  ){    
    this.crawlSiteForm = new FormGroup({
      url: new FormControl('')
    });

    this.findWordForm = new FormGroup({
      word: new FormControl('')
    });

    this.findEntityForm = new FormGroup({
      word: new FormControl('')
    });

    this.findAttrForm = new FormGroup({
      word: new FormControl('')
    });
  }

  ngOnInit(): void {
    this.crawlSiteForm = new FormGroup({
      url: new FormControl('')
    });

    this.findWordForm = new FormGroup({
      word: new FormControl('')
    });

    this.findEntityForm = new FormGroup({
      word: new FormControl('')
    });

    this.findAttrForm = new FormGroup({
      word: new FormControl('')
    });
  }

  onCrawlSubmit(){
    if (this.crawlSiteForm.invalid) {
     return;
    }

    this.httpService.ParseSite(this.crawlSiteForm.controls.url.value)
    .pipe()
    .subscribe(
      data => {
        alert('Crawl success');
      }
    );  
  }   
  
  onWordSubmit(){
    this.httpService.FindKeyword(this.findWordForm.controls.word.value, 'word')
    .pipe()
    .subscribe(
      data => {
        this.sites = data;
        alert('success');
      }
    );  
  }

  onEntitySubmit(){
    this.httpService.FindKeyword(this.findEntityForm.controls.word.value, 'entityName')
    .pipe()
    .subscribe(
      data => {
        this.sites = data;
        alert(this.sites.length)
      }
    );  
  }

  onAttrSubmit(){
    this.httpService.FindKeyword(this.findAttrForm.controls.word.value, 'entityAttribute')
    .pipe()
    .subscribe(
      data => {
        this.sites = data;
        alert(this.sites.length);
      }
    );  
  }
}
