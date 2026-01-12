import { Injectable } from '@angular/core';
import Toastify from 'toastify-js';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor() { }

  showSuccess(message: string): void {
    Toastify({
      text: message,
      duration: 3000,
      gravity: 'top',
      position: 'right',
      backgroundColor: '#28a745',
      className: 'toast-success'
    }).showToast();
  }

  showError(message: string): void {
    Toastify({
      text: message,
      duration: 5000,
      gravity: 'top',
      position: 'right',
      backgroundColor: '#dc3545',
      className: 'toast-error'
    }).showToast();
  }

  showInfo(message: string): void {
    Toastify({
      text: message,
      duration: 4000,
      gravity: 'top',
      position: 'right',
      backgroundColor: '#17a2b8',
      className: 'toast-info'
    }).showToast();
  }

  showWarning(message: string): void {
    Toastify({
      text: message,
      duration: 4000,
      gravity: 'top',
      position: 'right',
      backgroundColor: '#ffc107',
      className: 'toast-warning'
    }).showToast();
  }
}