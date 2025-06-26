import { Component, OnInit, inject } from '@angular/core'
import { Router, ActivatedRoute, RouterModule } from '@angular/router'
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms'
import { first } from 'rxjs/operators'
import { AccountService, AlertService } from 'app/_services'
import { CommonModule } from '@angular/common'
import { MatCardModule } from '@angular/material/card'
import { MatFormFieldModule } from '@angular/material/form-field'
import { MatInputModule } from '@angular/material/input'
import { MatButtonModule } from '@angular/material/button'

@Component({
  templateUrl: 'add-edit.component.html',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, RouterModule, FormsModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
})
export class AddEditComponent implements OnInit {
  private formBuilder = inject(FormBuilder)
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private accountService = inject(AccountService)
  private alertService = inject(AlertService)

  form: FormGroup
  id: string
  isAddMode: boolean
  isAddingFirstUserMode: boolean
  loading = false
  submitted = false

  ngOnInit() {
    this.id = this.route.snapshot.params['id']
    this.isAddingFirstUserMode = this.route.snapshot.routeConfig?.path !== 'add'
    this.isAddMode = !this.id

    // password not required in edit mode
    const passwordValidators = [Validators.minLength(6)]
    if (this.isAddMode) {
      passwordValidators.push(Validators.required)
    }

    this.form = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', passwordValidators],
    })

    if (!this.isAddMode) {
      this.accountService.getById(this.id)
        .pipe(first())
        .subscribe(x => this.form.patchValue(x))
    }
  }

  // convenience getter for easy access to form fields
  get f() {
    return this.form.controls
  }

  onSubmit() {
    this.submitted = true

    // reset alerts on submit
    this.alertService.clear()

    // stop here if form is invalid
    if (this.form.invalid) {
      return
    }

    this.loading = true
    if (!this.isAddingFirstUserMode) {
      this.addUser()
    }
    else if (this.isAddMode) {
      this.createUser()
    }
    else {
      this.updateUser()
    }
  }

  private addUser() {
    this.accountService.add(this.form.value)
      .pipe(first())
      .subscribe({
        next: () => {
          this.alertService.success('User added successfully', true)
          this.loading = false
        },
        error: (error) => {
          this.alertService.error(error)
          this.loading = false
        },
      })
  }

  private createUser() {
    this.accountService.register(this.form.value)
      .pipe(first())
      .subscribe({
        next: () => {
          this.alertService.success('User added successfully', true)
          this.router.navigate(['../'], { relativeTo: this.route })
        },
        error: (error) => {
          this.alertService.error(error)
          this.loading = false
        },
      })
  }

  private updateUser() {
    this.accountService.update(this.id, this.form.value)
      .pipe(first())
      .subscribe({
        next: () => {
          this.alertService.success('Update successful', true)
          this.router.navigate(['../../'], { relativeTo: this.route })
        },
        error: (error) => {
          this.alertService.error(error)
          this.loading = false
        },
      })
  }
}
