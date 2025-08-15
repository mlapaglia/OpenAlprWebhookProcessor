import { TestBed } from '@angular/core/testing';
import { AlertService } from './alert.service';
import { Alert, AlertType } from 'app/_models';

describe('AlertService', () => {
  let service: AlertService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AlertService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('onAlert', () => {
    it('should return observable filtered by default id when no id provided', (done) => {
      const testMessage = 'Test alert';

      service.onAlert().subscribe(alert => {
        expect(alert.id).toBe('default-alert');
        expect(alert.message).toBe(testMessage);
        expect(alert.type).toBe(AlertType.Success);
        done();
      });

      service.success(testMessage);
    });

    it('should return observable filtered by specific id', (done) => {
      const testId = 'custom-id';
      const testMessage = 'Test alert';

      service.onAlert(testId).subscribe(alert => {
        expect(alert.id).toBe(testId);
        expect(alert.message).toBe(testMessage);
        expect(alert.type).toBe(AlertType.Error);
        done();
      });

      // This should not trigger the subscription above
      service.success('Other message');

      // This should trigger it
      service.alert(new Alert({ id: testId, message: testMessage, type: AlertType.Error }));
    });

    it('should not emit alerts with different ids', (done) => {
      const targetId = 'target-id';
      const differentId = 'different-id';
      let alertReceived = false;

      service.onAlert(targetId).subscribe(() => {
        alertReceived = true;
        fail('Should not receive alert with different id');
      });

      service.alert(new Alert({ id: differentId, message: 'Test', type: AlertType.Info }));

      // Wait a bit to ensure no alert was received
      setTimeout(() => {
        expect(alertReceived).toBe(false);
        done();
      }, 50);
    });
  });

  describe('success', () => {
    it('should create success alert with default keepAfterRouteChange', (done) => {
      const message = 'Success message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Success);
        expect(alert.keepAfterRouteChange).toBeUndefined();
        expect(alert.id).toBe('default-alert');
        done();
      });

      service.success(message);
    });

    it('should create success alert with keepAfterRouteChange true', (done) => {
      const message = 'Success message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Success);
        expect(alert.keepAfterRouteChange).toBe(true);
        done();
      });

      service.success(message, true);
    });

    it('should create success alert with keepAfterRouteChange false', (done) => {
      const message = 'Success message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Success);
        expect(alert.keepAfterRouteChange).toBe(false);
        done();
      });

      service.success(message, false);
    });
  });

  describe('error', () => {
    it('should create error alert with default keepAfterRouteChange', (done) => {
      const message = 'Error message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Error);
        expect(alert.keepAfterRouteChange).toBeUndefined();
        expect(alert.id).toBe('default-alert');
        done();
      });

      service.error(message);
    });

    it('should create error alert with keepAfterRouteChange true', (done) => {
      const message = 'Error message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Error);
        expect(alert.keepAfterRouteChange).toBe(true);
        done();
      });

      service.error(message, true);
    });
  });

  describe('info', () => {
    it('should create info alert with default keepAfterRouteChange', (done) => {
      const message = 'Info message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Info);
        expect(alert.keepAfterRouteChange).toBeUndefined();
        expect(alert.id).toBe('default-alert');
        done();
      });

      service.info(message);
    });

    it('should create info alert with keepAfterRouteChange false', (done) => {
      const message = 'Info message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Info);
        expect(alert.keepAfterRouteChange).toBe(false);
        done();
      });

      service.info(message, false);
    });
  });

  describe('warn', () => {
    it('should create warning alert with default keepAfterRouteChange', (done) => {
      const message = 'Warning message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Warning);
        expect(alert.keepAfterRouteChange).toBeUndefined();
        expect(alert.id).toBe('default-alert');
        done();
      });

      service.warn(message);
    });

    it('should create warning alert with keepAfterRouteChange true', (done) => {
      const message = 'Warning message';

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        expect(alert.type).toBe(AlertType.Warning);
        expect(alert.keepAfterRouteChange).toBe(true);
        done();
      });

      service.warn(message, true);
    });
  });

  describe('alert', () => {
    it('should emit alert with provided details', (done) => {
      const alertData = new Alert({
        id: 'custom-alert',
        message: 'Custom message',
        type: AlertType.Success,
        keepAfterRouteChange: true,
      });

      service.onAlert('custom-alert').subscribe(alert => {
        expect(alert.id).toBe('custom-alert');
        expect(alert.message).toBe('Custom message');
        expect(alert.type).toBe(AlertType.Success);
        expect(alert.keepAfterRouteChange).toBe(true);
        done();
      });

      service.alert(alertData);
    });

    it('should set default id when alert has no id', (done) => {
      const alertData = new Alert({
        message: 'No id message',
        type: AlertType.Error,
      });

      service.onAlert().subscribe(alert => {
        expect(alert.id).toBe('default-alert');
        expect(alert.message).toBe('No id message');
        expect(alert.type).toBe(AlertType.Error);
        done();
      });

      service.alert(alertData);
    });

    it('should preserve existing id when alert already has one', (done) => {
      const customId = 'existing-id';
      const alertData = new Alert({
        id: customId,
        message: 'Has id message',
        type: AlertType.Info,
      });

      service.onAlert(customId).subscribe(alert => {
        expect(alert.id).toBe(customId);
        expect(alert.message).toBe('Has id message');
        expect(alert.type).toBe(AlertType.Info);
        done();
      });

      service.alert(alertData);
    });
  });

  describe('clear', () => {
    it('should emit clear alert with default id when no id provided', (done) => {
      service.onAlert().subscribe(alert => {
        expect(alert.id).toBe('default-alert');
        expect(alert.message).toBeUndefined();
        expect(alert.type).toBeUndefined();
        done();
      });

      service.clear();
    });

    it('should emit clear alert with specific id', (done) => {
      const customId = 'clear-me';

      service.onAlert(customId).subscribe(alert => {
        expect(alert.id).toBe(customId);
        expect(alert.message).toBeUndefined();
        expect(alert.type).toBeUndefined();
        done();
      });

      service.clear(customId);
    });
  });

  describe('multiple subscribers', () => {
    it('should notify all subscribers of the same id', () => {
      const message = 'Broadcast message';
      let subscriber1Called = false;
      let subscriber2Called = false;

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        subscriber1Called = true;
      });

      service.onAlert().subscribe(alert => {
        expect(alert.message).toBe(message);
        subscriber2Called = true;
      });

      service.success(message);

      expect(subscriber1Called).toBe(true);
      expect(subscriber2Called).toBe(true);
    });
  });
});
