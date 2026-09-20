import type { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function vatAmountValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string | null;
    if (!value) {
      return null;
    }

    const normalized = value.trim().replace(',', '.');

    if (!/^\d+(\.\d{1,2})?$/.test(normalized)) {
      return { invalidFormat: true };
    }

    const number = Number(normalized);

    if (!(number > 0)) {
      return { notPositive: true };
    }

    if (number > 1_000_000_000) {
      return { tooLarge: true };
    }

    return null;
  };
}