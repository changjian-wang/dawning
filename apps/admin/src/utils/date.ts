import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

/**
 * Format date and time
 */
export function formatDateTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('YYYY-MM-DD HH:mm:ss');
}

/**
 * Format date
 */
export function formatDate(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('YYYY-MM-DD');
}

/**
 * Format time
 */
export function formatTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).format('HH:mm:ss');
}

/**
 * Relative time
 */
export function formatRelativeTime(date: string | Date | undefined): string {
  if (!date) return '-';
  return dayjs(date).fromNow();
}
