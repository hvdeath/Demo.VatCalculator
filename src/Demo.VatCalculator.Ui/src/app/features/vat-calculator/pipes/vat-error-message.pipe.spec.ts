import { VatErrorMessagePipe } from './vat-error-message.pipe';

describe('VatErrorMessagePipe', () => {
  const pipe = new VatErrorMessagePipe();

  it('returns null while the control is untouched', () => {
    expect(pipe.transform({ required: true }, false)).toBeNull();
  });

  it('maps a required error', () => {
    expect(pipe.transform({ required: true }, true)).toBe('Enter an amount.');
  });

  it('maps a malformed amount', () => {
    expect(pipe.transform({ invalidFormat: true }, true)).toContain('at most 2 decimals');
  });

  it('maps a non-positive amount', () => {
    expect(pipe.transform({ notPositive: true }, true)).toContain('greater than 0');
  });

  it('maps an amount above the maximum', () => {
    expect(pipe.transform({ tooLarge: true }, true)).toContain('1 000 000 000.00');
  });

  it('returns null when there are no errors', () => {
    expect(pipe.transform(null, true)).toBeNull();
  });
});