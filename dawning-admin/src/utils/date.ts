import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

/**
 * 格式化日期时间
 */
export function formatDateTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('YYYY-MM-DD HH:mm:ss');
}

/**
 * 格式化日期
 */
export function formatDate(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('YYYY-MM-DD');
}

/**
 * 格式化时间
 */
export function formatTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('HH:mm:ss');
}

/**
 * 相对时间
 */
export function formatRelativeTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).fromNow();
}
