export interface IPagedData<T> 
{
    // 当前页码
    pageIndex: number;
    
    // 每页数量
    pageSize: number;
    
    // 总记录数
    totalCount: number;
    
    // 数据列表
    items: T[];    
}