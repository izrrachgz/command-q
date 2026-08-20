import { TZDate } from '@date-fns/tz';

const LocalDateTimePattern = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?$/;

export const toUtcIso = (value: Date | string): string => {
  const date = value instanceof Date ? value : new Date(ensureUtcSuffix(value));

  if (Number.isNaN(date.getTime())) {
    throw new RangeError(`The value '${String(value)}' is not a valid date/time.`);
  }

  return date.toISOString();
};

export const accountLocalDateTimeToUtcIso = (value: string, timeZone: string): string => {
  const match = LocalDateTimePattern.exec(value);
  if (!match) {
    throw new RangeError(`The value '${value}' must use the yyyy-MM-ddTHH:mm[:ss] format.`);
  }

  const [, year, month, day, hour, minute, second = '0'] = match;
  const zoned = TZDate.tz(
    timeZone,
    Number(year),
    Number(month) - 1,
    Number(day),
    Number(hour),
    Number(minute),
    Number(second),
  );

  if (Number.isNaN(zoned.getTime())) {
    throw new RangeError(`The value '${value}' is invalid in timezone '${timeZone}'.`);
  }

  return new Date(zoned.getTime()).toISOString();
};

export const accountLocalDateToUtcIso = (
  value: string,
  timeZone: string,
  endOfDay = false,
): string => {
  return accountLocalDateTimeToUtcIso(`${value}T${endOfDay ? '23:59:59' : '00:00:00'}`, timeZone);
};

export const ensureUtcSuffix = (value: string): string => {
  const trimmed = value.trim();
  return /(?:Z|[+-]\d{2}:?\d{2})$/i.test(trimmed) ? trimmed : `${trimmed}Z`;
};
