import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'istDate',
  standalone: true
})
export class IstDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, format: string = 'short'): string {
    if (!value) return '';
    
    const date = new Date(value);
    if (isNaN(date.getTime())) return '';
    
    // Convert UTC to IST (UTC + 5:30)
    const istDate = new Date(date.getTime() + (5.5 * 60 * 60 * 1000));
    
    // Format based on the format parameter
    switch (format) {
      case 'short':
        return istDate.toLocaleString('en-IN', {
          year: 'numeric',
          month: '2-digit',
          day: '2-digit',
          hour: '2-digit',
          minute: '2-digit',
          hour12: true
        });
      case 'medium':
        return istDate.toLocaleString('en-IN', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
          hour12: true
        });
      case 'date':
        return istDate.toLocaleDateString('en-IN');
      case 'time':
        return istDate.toLocaleTimeString('en-IN', { hour12: true });
      default:
        return istDate.toLocaleString('en-IN');
    }
  }
}