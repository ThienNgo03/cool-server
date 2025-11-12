export interface IBaseRequest<T> {
    ids?: string;
    pageIndex?: number;
    pageSize?: number;
    searchTerm?: string;
    include?: keyof T;
    sortBy?: keyof T;
    sortOrder?: 'asc' | 'desc';
    createdDate?: Date;
    lastUpdated?: Date;
}