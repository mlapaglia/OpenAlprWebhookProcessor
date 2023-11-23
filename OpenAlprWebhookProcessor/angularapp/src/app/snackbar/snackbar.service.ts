import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarComponent } from './snackbar.component';
import { SnackBarType } from './snackbartype';
import { SnackBar } from './snackbar';

@Injectable({
    providedIn: 'root'
})
export class SnackbarService {
    constructor(private snackBar: MatSnackBar) { }

    create(message: string, snackBarType: SnackBarType, message2?: string) {
        this.snackBar.openFromComponent(
            SnackbarComponent,
            {
                horizontalPosition: 'right',
                verticalPosition: 'bottom',
                duration: 3000,
                data: new SnackBar({
                    message: message,
                    message2: message2,
                    snackType: snackBarType
                })
            });
    }
}
