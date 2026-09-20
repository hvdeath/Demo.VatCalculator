import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import type {
  VatCalculationOutcome,
  VatCalculationProblemDetail,
  VatCalculationRequest,
  VatCalculationResponse,
} from '../models/vat-calculator.model';

@Injectable({ providedIn: 'root' })
export class VatCalculatorService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/v1/vat/calculate';

  calculate(request: VatCalculationRequest): Observable<VatCalculationOutcome> {
    return this.http.post<VatCalculationResponse>(this.endpoint, request).pipe(
      map((data): VatCalculationOutcome => ({ ok: true, data })),
      catchError((error: unknown) =>
        of<VatCalculationOutcome>({ ok: false, message: this.toMessage(error) }),
      ),
    );
  }

  private toMessage(error: unknown): string {
    const problem = (error as { error?: VatCalculationProblemDetail } | null)?.error;

    for (const entries of Object.values(problem?.errors ?? {})) {
      const entry = entries[0];
      if (entry) {
        return entry.message;
      }
    }

    return problem?.detail ?? problem?.title ?? 'Unexpected error. Please try again.';
  }
}