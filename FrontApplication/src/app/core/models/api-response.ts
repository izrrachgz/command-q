export interface BasicResponse {
  readonly success: boolean;
  readonly message: string;
}

export interface DetailResponse<T> extends BasicResponse {
  readonly detail?: T;
  readonly data?: T;
}

export const detailPayload = <T>(response: DetailResponse<T>): T | undefined => {
  return response.detail ?? response.data;
};

export interface PagedResponse<T> {
  readonly collection: readonly T[];
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
