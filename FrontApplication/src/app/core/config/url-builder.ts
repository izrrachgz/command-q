export const combineUrl = (baseUrl: string, relativeUrl: string): string => {
  const normalizedBase = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
  const normalizedRelative = relativeUrl.startsWith('/') ? relativeUrl.slice(1) : relativeUrl;

  return new URL(normalizedRelative, normalizedBase).toString();
};
