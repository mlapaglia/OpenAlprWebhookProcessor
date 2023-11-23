import { TestBed } from '@angular/core/testing';
import { PlatesComponent } from './plates.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe(PlatesComponent.name, () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
                RouterTestingModule,
                BrowserAnimationsModule]
        });
    });

    it('should create the app', () => {
        const fixture = TestBed.createComponent(PlatesComponent);
        const app = fixture.componentInstance;
        expect(app).toBeTruthy();
    });
});
