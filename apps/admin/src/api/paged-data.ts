export interface IPagedData<T> {
  // Current page number
  pageIndex: number;

  // Items per page
  pageSize: number;

  // Total record count
  totalCount: number;

  // Data list
  items: T[];
}
