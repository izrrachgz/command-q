import { Injectable } from '@angular/core';
import type { Cell, Column } from 'write-excel-file/browser';

import { ApiMimeTypes } from '../api/api-constants';

export interface ReportColumn<T> {
  readonly header: string;
  readonly key: keyof T & string;
  readonly width?: number;
}

@Injectable({ providedIn: 'root' })
export class ReportExportService {
  async exportXlsx<T extends Record<string, unknown>>(
    filename: string,
    worksheetName: string,
    rows: readonly T[],
    columns: readonly ReportColumn<T>[],
  ): Promise<void> {
    const { default: writeXlsxFile } = await import('write-excel-file/browser');
    const excelColumns: Column<T>[] = columns.map((column) => ({
      header: {
        value: column.header,
        fontWeight: 'bold',
      },
      cell: (row) => normalizeCell(row[column.key]),
      width: column.width ?? Math.max(column.header.length + 4, 16),
    }));

    const blob = await writeXlsxFile([...rows], {
      sheet: worksheetName,
      columns: excelColumns,
      stickyRowsCount: 1,
      showGridLines: true,
    }).toBlob();

    downloadBlob(new Blob([blob], {
      type: ApiMimeTypes.xlsx,
    }), filename.endsWith('.xlsx') ? filename : `${filename}.xlsx`);
  }
}

const normalizeCell = (value: unknown): Cell => {
  if (value === null || value === undefined) {
    return null;
  }

  if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean' || value instanceof Date) {
    return value;
  }

  if (Array.isArray(value)) {
    return value.join(', ');
  }

  try {
    return JSON.stringify(value);
  } catch {
    return String(value);
  }
};

const downloadBlob = (blob: Blob, filename: string): void => {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  URL.revokeObjectURL(url);
};
