import { SearchCriteria } from '../../../../core/models/api-response';

export interface ApplicationConfigurationModel {
  readonly id: number;
  readonly serverTimeZone: string;
  readonly applicationVersion: string;
  readonly language: string;
  readonly environment: string;
  readonly created: string;
}

export interface ApplicationConfigurationFilters extends SearchCriteria {}
