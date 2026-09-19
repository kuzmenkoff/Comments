import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CaptchaResponse, CommentDto, CommentSortField, PagedResult, SortDirection
} from '../models/comment.models';

@Injectable({ providedIn: 'root' })
export class CommentsService {
  private readonly api = environment.apiUrl;
  private readonly reload = new Subject<void>();
  readonly onReload = this.reload.asObservable();
  notifyReload(): void { this.reload.next(); }

  constructor(private http: HttpClient) {}

  getPage(page: number, sort: CommentSortField, direction: SortDirection): Observable<PagedResult<CommentDto>> {
    const params = { page, sort, direction } as any;
    return this.http.get<PagedResult<CommentDto>>(`${this.api}/api/comments`, { params });
  }

  create(form: FormData): Observable<CommentDto> {
    return this.http.post<CommentDto>(`${this.api}/api/comments`, form);
  }

  preview(text: string): Observable<{ html: string }> {
    return this.http.post<{ html: string }>(`${this.api}/api/comments/preview`, { text });
  }

  getCaptcha(): Observable<CaptchaResponse> {
    return this.http.get<CaptchaResponse>(`${this.api}/api/captcha`);
  }

  attachmentUrl(id: number): string {
    return `${this.api}/api/attachments/${id}`;
  }
}