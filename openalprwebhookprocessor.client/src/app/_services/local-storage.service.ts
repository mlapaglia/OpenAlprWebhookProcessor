import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LocalStorageService {
  public setData(key: string, data: string) {
    const jsonData = JSON.stringify(data);
    localStorage.setItem(key, jsonData);
  }

  public getData(key: string): string {
    return localStorage.getItem(key) ?? '';
  }

  removeData(key: string) {
    localStorage.removeItem(key);
  }
}
