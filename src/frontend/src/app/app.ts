import { Component } from '@angular/core';
import { CommentList } from './components/comment-list/comment-list';

@Component({
  selector: 'app-root',
  imports: [CommentList],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
