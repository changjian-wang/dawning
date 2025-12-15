/**
 * 数据导出工具
 * 支持导出为 CSV 和 Excel (xlsx) 格式
 */

/**
 * 列定义
 */
export interface ExportColumn {
  /** 数据字段名 */
  field: string;
  /** 列标题 */
  title: string;
  /** 格式化函数 */
  formatter?: (value: any, record: any) => string;
}

/**
 * 导出配置
 */
export interface ExportOptions {
  /** 文件名（不含扩展名） */
  filename: string;
  /** 列定义 */
  columns: ExportColumn[];
  /** 数据 */
  data: any[];
  /** 导出格式 */
  format?: 'csv' | 'xlsx';
  /** CSV 分隔符 */
  separator?: string;
}

/**
 * 将数据转换为 CSV 字符串
 */
function convertToCSV(
  data: any[],
  columns: ExportColumn[],
  separator = ','
): string {
  // 添加 BOM 以支持中文
  const BOM = '\uFEFF';

  // 表头
  const headers = columns.map((col) => `"${col.title}"`).join(separator);

  // 数据行
  const rows = data.map((record) => {
    return columns
      .map((col) => {
        let value = record[col.field];

        // 使用格式化函数
        if (col.formatter) {
          value = col.formatter(value, record);
        }

        // 处理 null/undefined
        if (value === null || value === undefined) {
          value = '';
        }

        // 转换为字符串
        value = String(value);

        // 转义双引号，并用双引号包裹
        value = value.replace(/"/g, '""');
        return `"${value}"`;
      })
      .join(separator);
  });

  return BOM + [headers, ...rows].join('\n');
}

/**
 * 下载文件
 */
function downloadFile(content: string, filename: string, mimeType: string) {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);

  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();

  // 清理
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * 导出为 CSV 文件
 */
export function exportToCSV(options: ExportOptions): void {
  const { filename, columns, data, separator = ',' } = options;

  const csvContent = convertToCSV(data, columns, separator);
  downloadFile(csvContent, `${filename}.csv`, 'text/csv;charset=utf-8');
}

/**
 * 导出为 Excel 文件（简单 HTML 表格格式，Excel 可直接打开）
 * 注意：这是一个简易实现，如需更复杂的 Excel 功能请使用 xlsx 库
 */
export function exportToExcel(options: ExportOptions): void {
  const { filename, columns, data } = options;

  // 使用 HTML 表格格式，Excel 可以直接打开
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
            ${columns.map((col) => `<th>${escapeHtml(col.title)}</th>`).join('')}
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
 * HTML 转义
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
 * 通用导出函数
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
 * 格式化日期时间
 */
export function formatDateTime(value: string | Date | null | undefined): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
}

/**
 * 格式化布尔值
 */
export function formatBoolean(value: boolean | null | undefined): string {
  if (value === null || value === undefined) return '';
  return value ? '是' : '否';
}
