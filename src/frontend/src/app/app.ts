import { Component } from '@angular/core';
import { CommentList } from './components/comment-list/comment-list';
import { CommentForm } from './components/comment-form/comment-form';

@Component({
  selector: 'app-root',
  imports: [CommentList, CommentForm],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
