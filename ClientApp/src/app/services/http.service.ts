import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Site } from '../models/Site';

@Injectable({
  providedIn: 'root'
})
export class HttpService {

  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  curApi : string = 'https://localhost:5001/api/Site';

  constructor(
    private http: HttpClient,
  ) { }

  ParseSite(siteUrl: string, currentApi: string = this.curApi) {   
    return this.http.post<Site>(currentApi, { url : siteUrl }, this.httpOptions);
  }

  FindKeyword(keyword: string, options: string, currentApi: string = this.curApi) {   
    let url = `${currentApi}/` + `?${options}=${keyword}`; 
    //alert(url);
    return this.http.get<Site[]>(url, this.httpOptions);
  }
}
