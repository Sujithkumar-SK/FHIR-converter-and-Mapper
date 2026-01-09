import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor() { }

  showSuccess(message: string): void {
    // Using browser alert for now - can be replaced with toast library later
    alert('Success: ' + message);
  }

  showError(message: string): void {
    // Using browser alert for now - can be replaced with toast library later
    alert('Error: ' + message);
  }

  showInfo(message: string): void {
    // Using browser alert for now - can be replaced with toast library later
    alert('Info: ' + message);
  }
}