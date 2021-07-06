import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Tag } from './tag';

@Injectable({
  providedIn: 'root'
})
export class TagsService {

  constructor(private http: HttpClient) { }

  getTags(): Observable<Tag[]> {
    return this.http.get<Tag[]>(`/settings/tags`);
  }

  upsertTags(tags: Tag[]): Observable<any> {
    return this.http.post(`/settings/tags`, tags);
  }
}
