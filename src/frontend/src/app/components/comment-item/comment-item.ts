import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommentDto } from '../../models/comment.models';
import { CommentsService } from '../../services/comments.service';
import { CommentForm } from '../comment-form/comment-form';

@Component({
  selector: 'app-comment-item',
  imports: [DatePipe, CommentItem, CommentForm],
  templateUrl: './comment-item.html',
  styleUrl: './comment-item.css'
})
export class CommentItem {
  @Input({ required: true }) comment!: CommentDto;
  showReply = false;
  constructor(public comments: CommentsService) {}
  isImage(c: CommentDto): boolean { return c.attachment?.type === 1; }
}