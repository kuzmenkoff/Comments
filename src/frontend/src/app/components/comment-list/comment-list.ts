import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommentDto, CommentSortField, SortDirection } from '../../models/comment.models';
import { CommentsService } from '../../services/comments.service';
import { CommentItem } from '../comment-item/comment-item';

@Component({
  selector: 'app-comment-list',
  imports: [DatePipe, CommentItem],
  templateUrl: './comment-list.html',
  styleUrl: './comment-list.css'
})
export class CommentList implements OnInit {
  readonly comments = signal<CommentDto[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(1);
  readonly sort = signal<CommentSortField>('CreatedAt');
  readonly direction = signal<SortDirection>('Descending');
  readonly expandedId = signal<number | null>(null);

  constructor(private service: CommentsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.getPage(this.page(), this.sort(), this.direction()).subscribe(res => {
      this.comments.set(res.items);
      this.totalPages.set(res.totalPages || 1);
    });
  }

  changeSort(field: CommentSortField): void {
    if (this.sort() === field) {
      this.direction.set(this.direction() === 'Ascending' ? 'Descending' : 'Ascending');
    } else {
      this.sort.set(field);
      this.direction.set('Ascending');
    }
    this.page.set(1);
    this.load();
  }

  goTo(p: number): void {
    if (p < 1 || p > this.totalPages()) return;
    this.page.set(p);
    this.load();
  }

  toggle(id: number): void {
    this.expandedId.set(this.expandedId() === id ? null : id);
  }

  indicator(field: CommentSortField): string {
    if (this.sort() !== field) return '';
    return this.direction() === 'Ascending' ? ' ▲' : ' ▼';
  }
}