export interface OperationResponse {
  readonly message: string;
}

export interface PageResponse<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly total: number;
  readonly totalPages: number;
}

export interface SearchCriteria {
  readonly page: number;
  readonly size: number;
  readonly searchTerm?: string;
  readonly sortBy?: string;
  readonly ascendingSort?: boolean;
  readonly startDate?: string;
  readonly endDate?: string;
}
