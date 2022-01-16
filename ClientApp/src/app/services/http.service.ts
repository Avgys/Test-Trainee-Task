import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ParseRequest } from '../models/ParseRequest';

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
    //let url = `${environment.apiUrl}/` + currentApi + 'login'; 
    return this.http.post<ParseRequest>(currentApi, { url : siteUrl }, this.httpOptions).pipe(            
        // catchError(err => {
        //     this.handleError<Account>(`login`);
        //     throw 'Wrong login or password';
        // })
    );
}
}
