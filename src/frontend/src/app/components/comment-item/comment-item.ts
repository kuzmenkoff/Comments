import { Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommentDto } from '../../models/comment.models';
import { CommentsService } from '../../services/comments.service';

@Component({
  selector: 'app-comment-item',
  imports: [DatePipe, CommentItem],   // self-import enables recursion
  templateUrl: './comment-item.html',
  styleUrl: './comment-item.css'
})
export class CommentItem {
  @Input({ required: true }) comment!: CommentDto;

  constructor(public comments: CommentsService) {}

  isImage(c: CommentDto): boolean {
    return c.attachment?.type === 1;
  }
}