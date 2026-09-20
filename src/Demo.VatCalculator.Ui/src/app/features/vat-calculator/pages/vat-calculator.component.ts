import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators, type AbstractControl } from '@angular/forms';
import { merge } from 'rxjs';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent } from '@angular/material/card';
import {
  MatError,
  MatFormField,
  MatHint,
  MatInput,
  MatLabel,
} from '@angular/material/input';
import { MatRadioButton, MatRadioGroup } from '@angular/material/radio';
import { CurrencyPipe } from '@angular/common';
import type { VatCalculationRequest, VatCalculationResponse } from '../models/vat-calculator.model';
import { VatCalculatorService } from '../services/vat-calculator.service';
import { vatAmountValidator } from '../validators/vat-amount.validator';

export type AmountKind = 'net' | 'gross' | 'vat';

@Component({
  selector: 'app-vat-calculator',
  imports: [
    ReactiveFormsModule,
    MatCard,
    MatCardContent,
    MatFormField,
    MatLabel,
    MatInput,
    MatHint,
    MatError,
    MatRadioGroup,
    MatRadioButton,
    MatButton,
    CurrencyPipe,
  ],
  templateUrl: './vat-calculator.component.html',
  styleUrl: './vat-calculator.component.scss',
})
export class VatCalculatorComponent {
  protected readonly rates = [10, 13, 20];
  protected readonly kinds: AmountKind[] = ['net', 'gross', 'vat'];
  protected readonly kindLabels: Record<AmountKind, string> = {
    net: 'Net amount',
    gross: 'Gross amount',
    vat: 'VAT amount',
  };

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(VatCalculatorService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = this.fb.nonNullable.group({
    rate: [20],
    kind: ['net' as AmountKind],
    amount: ['', [Validators.required, vatAmountValidator()]],
  });

  protected readonly result = signal<VatCalculationResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly amountError = signal<string | null>(null);

  constructor() {
    const amount = this.form.controls.amount;
    merge(amount.valueChanges, amount.statusChanges)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.amountError.set(this.messageFor(amount)));
  }

  protected submit(): void {
    if (this.pending()) {
      return;
    }

    const amountControl = this.form.controls.amount;

    if (this.form.invalid) {
      amountControl.markAsTouched();
      return;
    }

    const { rate, kind } = this.form.getRawValue();
    const amount = Number(amountControl.value.replace(',', '.'));

    const request: Partial<VatCalculationRequest> = { rate };
    request[kind] = amount;

    this.result.set(null);
    this.error.set(null);
    this.pending.set(true);

    this.service.calculate(request as VatCalculationRequest).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (outcome) => {

        if (outcome.ok) {
          this.result.set(outcome.data);
        } else {
          this.error.set(outcome.message);
        }

        this.pending.set(false);
      },
    });
  }

  private messageFor(control: AbstractControl): string | null {
    if (!control.touched || control.pristine || !control.errors) {
      return null;
    }

    const errors = control.errors;

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