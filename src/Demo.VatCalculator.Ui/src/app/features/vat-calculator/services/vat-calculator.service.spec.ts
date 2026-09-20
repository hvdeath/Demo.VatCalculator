import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { VatCalculatorService } from './vat-calculator.service';
import type { VatCalculationRequest } from '../models/vat-calculator.model';

describe('VatCalculatorService', () => {
  let service: VatCalculatorService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VatCalculatorService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts the request body and returns a success outcome', async () => {
    const request: VatCalculationRequest = { rate: 20, net: 100 };
    const promise = firstValueFrom(service.calculate(request));

    const expected = http.expectOne('/api/v1/vat/calculate');
    expect(expected.request.method).toBe('POST');
    expect(expected.request.body).toEqual({ rate: 20, net: 100 });

    expected.flush({ rate: 20, net: 100, vat: 20, gross: 120 });

    await expect(promise).resolves.toEqual({
      ok: true,
      data: { rate: 20, net: 100, vat: 20, gross: 120 },
    });
  });

  it('maps the first field error message for a validation failure', async () => {
    const promise = firstValueFrom(service.calculate({ rate: 21, net: 100 }));

    const expected = http.expectOne('/api/v1/vat/calculate');
    expected.flush(
      {
        type: 'urn:demo-vat:validation-failed',
        title: 'Validation failed.',
        status: 400,
        errors: {
          rate: [{ code: 'vat.rateNotSupported', message: 'Rate 21 is not supported.' }],
          net: [{ code: 'vat.multipleInputs', message: 'Only one amount.' }],
        },
      },
      { status: 400, statusText: 'Bad Request' },
    );

    await expect(promise).resolves.toEqual({
      ok: false,
      message: 'Rate 21 is not supported.',
    });
  });

  it('falls back to the problem detail when there are no field errors', async () => {
    const promise = firstValueFrom(service.calculate({ rate: 20, net: 100 }));

    const expected = http.expectOne('/api/v1/vat/calculate');
    expected.flush(
      { type: 'urn:demo-vat:unknown-field', title: 'Unknown field', detail: 'Property "tax" is not allowed.' },
      { status: 400, statusText: 'Bad Request' },
    );

    await expect(promise).resolves.toEqual({
      ok: false,
      message: 'Property "tax" is not allowed.',
    });
  });

  it('falls back to a generic message for non-HTTP errors', async () => {
    const promise = firstValueFrom(service.calculate({ rate: 20, net: 100 }));

    const expected = http.expectOne('/api/v1/vat/calculate');
    expected.error(new ProgressEvent('network'));

    await expect(promise).resolves.toEqual({
      ok: false,
      message: 'Unexpected error. Please try again.',
    });
  });
});