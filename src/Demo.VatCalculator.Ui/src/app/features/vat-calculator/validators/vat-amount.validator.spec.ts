import { FormControl } from '@angular/forms';
import { vatAmountValidator } from './vat-amount.validator';

describe('vatAmountValidator', () => {
  function errorsFor(value: string): Record<string, unknown> | null {
    const control = new FormControl(value, { nonNullable: true, validators: [vatAmountValidator()] });
    return control.errors;
  }

  it.each(['100', '100.50', '100,50', '0.01', '1000', '1000000000'])(
    'accepts %s as a valid amount',
    (value) => {
      expect(errorsFor(value)).toBeNull();
    },
  );

  it.each(['abc', '1.234', '1,234', '1.', '.5', '1 000'])(
    'rejects %s as malformed',
    (value) => {
      expect(errorsFor(value)).toEqual({ invalidFormat: true });
    },
  );

  it.each(['0', '-5'])('rejects %s because it is not positive', (value) => {
    expect(errorsFor(value)).toEqual({ notPositive: true });
  });

  it('rejects an amount above the maximum', () => {
    expect(errorsFor('1000000000.01')).toEqual({ tooLarge: true });
  });

  it('returns null for an empty value (required handled separately)', () => {
    expect(errorsFor('')).toBeNull();
  });
});