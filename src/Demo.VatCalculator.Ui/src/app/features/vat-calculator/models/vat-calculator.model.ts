export interface VatCalculationRequest {
  rate: number;
  net?: number;
  gross?: number;
  vat?: number;
}

export interface VatCalculationResponse {
  rate: number;
  net: number;
  vat: number;
  gross: number;
}

export interface VatCalculationEntry {
  code: string;
  message: string;
}

export interface VatCalculationProblemDetail {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, VatCalculationEntry[]>;
}

export interface VatCalculationSuccess {
  ok: true;
  data: VatCalculationResponse;
}

export interface VatCalculationFailure {
  ok: false;
  message: string;
}

export type VatCalculationOutcome = VatCalculationSuccess | VatCalculationFailure;