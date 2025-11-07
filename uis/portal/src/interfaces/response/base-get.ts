export interface IBaseGetResponse<T> {
    all: number;
    total: number;
    index?: number;
    size?: number;
    items: T[];
}