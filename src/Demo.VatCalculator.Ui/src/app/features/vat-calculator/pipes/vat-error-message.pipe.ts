import { Pipe, type PipeTransform } from '@angular/core';
import type { ValidationErrors } from '@angular/forms';

@Pipe({
  name: 'vatErrorMessage',
  standalone: true,
})
export class VatErrorMessagePipe implements PipeTransform {
  transform(errors: ValidationErrors | null, touched: boolean): string | null {
    if (!touched || !errors) {
      return null;
    }

    if (errors['required']) {
      return 'Enter an amount.';
    }
    if (errors['invalidFormat']) {
      return 'Use a number with at most 2 decimals (e.g. 12,34 or 12.34).';
    }
    if (errors['notPositive']) {
      return 'Amount must be greater than 0.';
    }
    if (errors['tooLarge']) {
      return 'Amount may not exceed 1 000 000 000.00.';
    }
    return null;
  }
}