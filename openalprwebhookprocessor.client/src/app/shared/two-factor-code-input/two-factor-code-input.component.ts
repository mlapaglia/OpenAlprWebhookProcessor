import { Component, ChangeDetectionStrategy, inject, type OnInit, input, output } from '@angular/core';
import { FormBuilder, type FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-two-factor-code-input',
  templateUrl: 'two-factor-code-input.component.html',
  styleUrl: 'two-factor-code-input.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
})
export class TwoFactorCodeInputComponent extends OnPushBaseComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = input<boolean>(false);
  readonly submitText = input<string>('Verify Code');
  readonly codeSubmitted = output<string>();

  form!: FormGroup;
  submitted = false;

  ngOnInit() {
    this.form = this.formBuilder.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.markForCheck();

    if (this.form.invalid) {
      return;
    }

    this.codeSubmitted.emit(this.f.code.value);
  }
}
