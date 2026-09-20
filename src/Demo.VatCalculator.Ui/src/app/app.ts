import { Component } from '@angular/core';
import { VatCalculatorComponent } from './features/vat-calculator/pages/vat-calculator.component';

@Component({
  selector: 'app-root',
  imports: [VatCalculatorComponent],
  template: `<app-vat-calculator />`,
})
export class App {}