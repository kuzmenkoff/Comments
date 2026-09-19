import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommentsService } from '../../services/comments.service';

@Component({
  selector: 'app-comment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './comment-form.html',
  styleUrl: './comment-form.css'
})
export class CommentForm implements OnInit {
  @Input() parentId: number | null = null;
  @Output() done = new EventEmitter<void>();

  @ViewChild('textArea') textArea!: ElementRef<HTMLTextAreaElement>;

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(CommentsService);

  readonly captchaImage = signal('');
  readonly captchaId = signal('');
  readonly previewHtml = signal<string | null>(null);
  readonly errors = signal<string[]>([]);
  selectedFile: File | null = null;

  form = this.fb.group({
    userName: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    email: ['', [Validators.required, Validators.email]],
    homePage: [''],
    text: ['', Validators.required],
    captchaAnswer: ['', Validators.required]
  });

  ngOnInit(): void { this.loadCaptcha(); }

  loadCaptcha(): void {
    this.service.getCaptcha().subscribe(c => {
      this.captchaId.set(c.captchaId);
      this.captchaImage.set(c.image);
    });
  }

  onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  insertTag(tag: string): void {
    const el = this.textArea.nativeElement;
    const start = el.selectionStart;
    const end = el.selectionEnd;
    const val = el.value;
    const selected = val.substring(start, end);
    const open = tag === 'a' ? '<a href="" title="">' : `<${tag}>`;
    const close = `</${tag}>`;
    const next = val.substring(0, start) + open + selected + close + val.substring(end);
    this.form.patchValue({ text: next });
    el.focus();
  }

  preview(): void {
    this.previewHtml.set(null);
    this.service.preview(this.form.value.text ?? '').subscribe({
      next: r => this.previewHtml.set(r.html),
      error: () => this.previewHtml.set('(invalid markup)')
    });
  }

  submit(): void {
    this.errors.set([]);
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    const v = this.form.value;
    const fd = new FormData();
    if (this.parentId != null) fd.append('parentId', String(this.parentId));
    fd.append('userName', v.userName!);
    fd.append('email', v.email!);
    if (v.homePage) fd.append('homePage', v.homePage);
    fd.append('text', v.text!);
    fd.append('captchaId', this.captchaId());
    fd.append('captchaAnswer', v.captchaAnswer!);
    if (this.selectedFile) fd.append('file', this.selectedFile);

    this.service.create(fd).subscribe({
      next: () => {
        this.form.reset();
        this.previewHtml.set(null);
        this.selectedFile = null;
        this.loadCaptcha();
        this.service.notifyReload();
        this.done.emit();
      },
      error: err => {
        const problem = err.error;
        const msgs: string[] = [];
        if (problem?.errors) {
          for (const key of Object.keys(problem.errors)) msgs.push(...problem.errors[key]);
        }
        this.errors.set(msgs.length ? msgs : ['Submit failed.']);
        this.loadCaptcha(); // captcha is one-time use
      }
    });
  }
}