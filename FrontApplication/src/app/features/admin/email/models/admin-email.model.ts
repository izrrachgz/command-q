import { SearchCriteria } from '../../../../core/models/api-response';

export interface AdminEmailRecord {
  readonly id: string;
  readonly subject: string;
  readonly recipient: string;
  readonly status: 'queued' | 'processing' | 'sent' | 'failed' | 'deadLetter';
  readonly attempts: number;
  readonly createdUtc?: string;
  readonly updatedUtc?: string;
}

export interface AdminEmailFilters extends SearchCriteria {
  readonly status?: string;
  readonly recipient?: string;
}
