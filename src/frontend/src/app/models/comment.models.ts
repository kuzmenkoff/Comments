export type CommentSortField = 'CreatedAt' | 'UserName' | 'Email';
export type SortDirection = 'Ascending' | 'Descending';

export interface AttachmentDto {
  id: number;
  type: number;          // 1 = Image, 2 = Text
  fileName: string;
  contentType: string;
  sizeBytes: number;
}

export interface CommentDto {
  id: number;
  parentId: number | null;
  userName: string;
  email: string;
  homePage: string | null;
  text: string;
  createdAt: string;
  attachment: AttachmentDto | null;
  replies: CommentDto[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CaptchaResponse {
  captchaId: string;
  image: string;         // data-URI
}