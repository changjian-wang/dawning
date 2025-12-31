/**
 * Data Export Utility
 * Supports exporting to CSV and Excel (xlsx) formats
 */

/**
 * Column definition
 */
export interface ExportColumn {
  /** Data field name */
  field: string;
  /** Column title */
  title: string;
  /** Formatter function */
  formatter?: (value: any, record: any) => string;
}

/**
 * Export configuration
 */
export interface ExportOptions {
  /** Filename (without extension) */
  filename: string;
  /** Column definitions */
  columns: ExportColumn[];
  /** Data */
  data: any[];
  /** Export format */
  format?: 'csv' | 'xlsx';
  /** CSV separator */
  separator?: string;
}

/**
 * Convert data to CSV string
 */
function convertToCSV(
  data: any[],
  columns: ExportColumn[],
  separator = ','
): string {
  // Add BOM to support Chinese characters
  const BOM = '\uFEFF';

  // Headers
  const headers = columns.map((col) => `"${col.title}"`).join(separator);

  // Data rows
  const rows = data.map((record) => {
    return columns
      .map((col) => {
        let value = record[col.field];

        // Use formatter function
        if (col.formatter) {
          value = col.formatter(value, record);
        }

        // Handle null/undefined
        if (value === null || value === undefined) {
          value = '';
        }

        // Convert to string
        value = String(value);

        // Escape double quotes and wrap with double quotes
        value = value.replace(/"/g, '""');
        return `"${value}"`;
      })
      .join(separator);
  });

  return BOM + [headers, ...rows].join('\n');
}

/**
 * Download file
 */
function downloadFile(content: string, filename: string, mimeType: string) {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);

  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();

  // Cleanup
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * Export to CSV file
 */
export function exportToCSV(options: ExportOptions): void {
  const { filename, columns, data, separator = ',' } = options;

  const csvContent = convertToCSV(data, columns, separator);
  downloadFile(csvContent, `${filename}.csv`, 'text/csv;charset=utf-8');
}

/**
 * HTML escape
 */
function escapeHtml(str: string): string {
  const htmlEntities: Record<string, string> = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
  };
  return str.replace(/[&<>"']/g, (char) => htmlEntities[char]);
}

/**
 * Export to Excel file (simple HTML table format, Excel can open directly)
 * Note: This is a simple implementation, use xlsx library for more complex Excel features
 */
export function exportToExcel(options: ExportOptions): void {
  const { filename, columns, data } = options;

  // Use HTML table format, Excel can open directly
  let html = `
    <html xmlns:o="urn:schemas-microsoft-com:office:office" 
          xmlns:x="urn:schemas-microsoft-com:office:excel" 
          xmlns="http://www.w3.org/TR/REC-html40">
    <head>
      <meta charset="UTF-8">
      <!--[if gte mso 9]>
      <xml>
        <x:ExcelWorkbook>
          <x:ExcelWorksheets>
            <x:ExcelWorksheet>
              <x:Name>Sheet1</x:Name>
              <x:WorksheetOptions>
                <x:DisplayGridlines/>
              </x:WorksheetOptions>
            </x:ExcelWorksheet>
          </x:ExcelWorksheets>
        </x:ExcelWorkbook>
      </xml>
      <![endif]-->
      <style>
        table { border-collapse: collapse; }
        th, td { 
          border: 1px solid #ddd; 
          padding: 8px; 
          text-align: left; 
        }
        th { 
          background-color: #f5f5f5; 
          font-weight: bold; 
        }
      </style>
    </head>
    <body>
      <table>
        <thead>
          <tr>
            ${columns
              .map((col) => `<th>${escapeHtml(col.title)}</th>`)
              .join('')}
          </tr>
        </thead>
        <tbody>
  `;

  data.forEach((record) => {
    html += '<tr>';
    columns.forEach((col) => {
      let value = record[col.field];

      if (col.formatter) {
        value = col.formatter(value, record);
      }

      if (value === null || value === undefined) {
        value = '';
      }

      html += `<td>${escapeHtml(String(value))}</td>`;
    });
    html += '</tr>';
  });

  html += `
        </tbody>
      </table>
    </body>
    </html>
  `;

  downloadFile(
    html,
    `${filename}.xls`,
    'application/vnd.ms-excel;charset=utf-8'
  );
}

/**
 * General export function
 */
export function exportData(options: ExportOptions): void {
  const format = options.format || 'csv';

  if (format === 'xlsx' || format === 'csv') {
    if (format === 'csv') {
      exportToCSV(options);
    } else {
      exportToExcel(options);
    }
  }
}

/**
 * Format date time
 * Returns format: YYYY-MM-DD HH:mm:ss
 */
export function formatDateTime(
  value: string | Date | null | undefined
): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}

/**
 * Format boolean value
 */
export function formatBoolean(value: boolean | null | undefined): string {
  if (value === null || value === undefined) return '';
  return value ? 'Yes' : 'No';
}
