export interface IBaseGetResponse<T> {
    total: number;
    index?: number;
    size?: number;
    items: T[];
}