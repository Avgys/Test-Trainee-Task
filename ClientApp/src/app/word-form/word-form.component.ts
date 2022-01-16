import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../services/http.service';

@Component({
  selector: 'app-word-form',
  templateUrl: './word-form.component.html',
  styleUrls: ['./word-form.component.css']
})
export class WordFormComponent implements OnInit {

  findWordForm: FormGroup;

  constructor(
    private httpService: HttpService
  ) { 
    
    this.findWordForm = new FormGroup({
      word: new FormControl('')
    });
  }

  ngOnInit() {
    
    this.findWordForm = new FormGroup({
      word: new FormControl('')
    });
  }

  onWordSubmit(){
    alert(this.findWordForm.controls.word.value);
    this.httpService.FindKeyword(this.findWordForm.controls.word.value)
    .pipe()
    .subscribe(
      data => {
        alert(data);
      }
    );  
  }

}
