import { Link } from "./link";

export interface PagedHateoasResponse<T> {
    data: T[];             // or `value` or `data`, depending on your API
    totalCount: number;     // total number of records across all pages
    pageSize: number;       // how many per page
    currentPage: number;    // current page number
    totalPages: number;     // total number of pages
    links?: Link[];         // HATEOAS navigation links
}
