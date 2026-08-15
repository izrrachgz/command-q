export const ApiHttpHeaders = {
  acceptLanguage: 'Accept-Language',
  contentType: 'Content-Type',
  correlationId: 'X-Correlation-Id',
} as const;

export const ApiHttpMethods = {
  post: 'POST',
} as const;

export const ApiMimeTypes = {
  json: 'application/json',
  xlsx: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
} as const;

export const SearchQueryParamNames = {
  ascendingSort: 'AscendingSort',
  endDate: 'EndDate',
  page: 'Page',
  searchTerm: 'SearchTerm',
  size: 'Size',
  sortBy: 'SortBy',
  startDate: 'StartDate',
} as const;

export const AdminAccountQueryParamNames = {
  active: 'Active',
  locked: 'Locked',
} as const;

export const AdminEmailQueryParamNames = {
  recipient: 'Recipient',
  status: 'Status',
} as const;

export const ApiSortFields = {
  createdUtc: 'CreatedUtc',
  username: 'Username',
} as const;
