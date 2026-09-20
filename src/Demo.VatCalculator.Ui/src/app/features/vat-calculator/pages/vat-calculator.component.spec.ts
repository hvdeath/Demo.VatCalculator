import { LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeDeAt from '@angular/common/locales/de-AT';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { VatCalculatorComponent } from './vat-calculator.component';

describe('VatCalculatorComponent', () => {
  registerLocaleData(localeDeAt, 'de-AT');

  function setup() {
    TestBed.configureTestingModule({
      imports: [VatCalculatorComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: LOCALE_ID, useValue: 'de-AT' },
      ],
    });

    const fixture = TestBed.createComponent(VatCalculatorComponent);
    fixture.detectChanges();
    return fixture;
  }

  function amountInput(fixture: ComponentFixture<VatCalculatorComponent>): HTMLInputElement {
    return fixture.nativeElement.querySelector('input[formcontrolname="amount"]') as HTMLInputElement;
  }

  function submitButton(fixture: ComponentFixture<VatCalculatorComponent>): HTMLButtonElement {
    return fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
  }

  function typeAmount(fixture: ComponentFixture<VatCalculatorComponent>, value: string): void {
    const input = amountInput(fixture);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function blurAmount(fixture: ComponentFixture<VatCalculatorComponent>): void {
    amountInput(fixture).dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function submit(fixture: ComponentFixture<VatCalculatorComponent>): void {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('disables the submit button while the amount is missing and enables it once valid', () => {
    const fixture = setup();

    expect(submitButton(fixture).disabled).toBe(true);

    typeAmount(fixture, '100');

    expect(submitButton(fixture).disabled).toBe(false);
  });

  it('shows a field error for a malformed amount once blurred', () => {
    const fixture = setup();
    typeAmount(fixture, '1.234');
    blurAmount(fixture);

    const error = fixture.nativeElement.querySelector('mat-error');
    expect(error?.textContent).toContain('Use a number with at most 2 decimals');
  });

  it('shows a required error for an emptied amount', () => {
    const fixture = setup();
    typeAmount(fixture, '100');
    typeAmount(fixture, '');
    blurAmount(fixture);

    const error = fixture.nativeElement.querySelector('mat-error');
    expect(error?.textContent).toContain('Enter an amount.');
  });

  it('submits a normalized comma amount and renders the result', () => {
    const fixture = setup();
    typeAmount(fixture, '1,23');
    submit(fixture);

    const http = TestBed.inject(HttpTestingController);
    const request = http.expectOne('/api/v1/vat/calculate');
    expect(request.request.body).toEqual({ rate: 20, net: 1.23 });

    request.flush({ rate: 20, net: 1.23, vat: 0.25, gross: 1.48 });
    fixture.detectChanges();

    const values = fixture.nativeElement.querySelectorAll('.result dd');
    expect(values[0]?.textContent).toContain('1,23');
    expect(values[1]?.textContent).toContain('0,25');
    expect(values[2]?.textContent).toContain('1,48');
  });

  it('renders the server error message', () => {
    const fixture = setup();
    typeAmount(fixture, '100');
    submit(fixture);

    const http = TestBed.inject(HttpTestingController);
    const request = http.expectOne('/api/v1/vat/calculate');
    request.flush(
      {
        type: 'urn:demo-vat:validation-failed',
        title: 'Validation failed.',
        status: 400,
        errors: {
          net: [{ code: 'vat.amountNotPositive', message: 'Amount must be greater than 0.' }],
        },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();

    const errorBox = fixture.nativeElement.querySelector('.error-box');
    expect(errorBox?.textContent).toContain('Amount must be greater than 0.');
  });

  it('ignores re-submission while a request is pending', () => {
    const fixture = setup();
    typeAmount(fixture, '100');

    submit(fixture);
    submit(fixture);

    const http = TestBed.inject(HttpTestingController);
    const requests = http.match('/api/v1/vat/calculate');
    expect(requests.length).toBe(1);

    requests[0].flush({ rate: 20, net: 100, vat: 20, gross: 120 });
  });

  afterEach(() => {
    const http = TestBed.inject(HttpTestingController);
    http.verify();
  });
});