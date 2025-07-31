import { type ElementRef, type OnInit, Component, Input, ViewChild, inject } from '@angular/core';
import { CameraMaskService } from './camera-mask.service';
import { CameraMask } from './camera-mask';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import type { Camera } from '../../camera';
import type { Coordinate } from './coordinate';
import { MatPaginatorModule, type PageEvent } from '@angular/material/paginator';
import { type MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-camera-mask',
  templateUrl: './camera-mask.component.html',
  styleUrls: ['./camera-mask.component.css'],
  imports: [MatCardModule, MatButtonToggleModule, MatButtonModule, MatPaginatorModule],
})
export class CameraMaskComponent implements OnInit {
  private readonly snackbarService = inject(SnackbarService);
  private readonly cameraMaskService = inject(CameraMaskService);

  @Input() camera: Camera;

  @ViewChild('canvas', { static: true }) canvas: ElementRef<HTMLCanvasElement>;
  @ViewChild('sampleCanvas', { static: true }) sampleCanvas: ElementRef<HTMLCanvasElement>;
  @ViewChild('savingCanvas', { static: true }) savingCanvas: ElementRef<HTMLCanvasElement>;
  @ViewChild('measureDiv', { static: true }) measureDiv: ElementRef<HTMLDivElement>;

  public ctx: CanvasRenderingContext2D;
  public sampleCtx: CanvasRenderingContext2D;
  public savingCtx: CanvasRenderingContext2D;
  public coordinates: Coordinate[] = [];
  public isLoadingSnapshot = false;
  public isClosed = false;
  public isDragging = false;
  public dragStartIndex = -1;
  public dragOffset: Coordinate;
  public currentPos: Coordinate;
  public dotRadius = 7.5;
  public lineThickness = 4;
  public forgiveness = 30;
  public image: HTMLImageElement;
  public imageHeight: number;
  public imageWidth: number;
  public scaleFactor: number;
  public targetWidth = 960;
  public imageInValidState = true;

  public paginatorIndex = 0;
  public samplePlates: string[] = [];

  ngOnInit(): void {
    this.getSamplePlates();
    this.prepareCanvases();
    this.addEventHandlers();
  }

  public handlePageEvent(pageEvent: PageEvent) {
    this.loadImageIntoCanvas(this.samplePlates[pageEvent.pageIndex]);
  }

  public handleToggleChange(toggleEvent: MatButtonToggleChange) {
    if (toggleEvent.value === 'snapshot') {
      this.samplePlates = [`/api/images/${this.camera.id}/snapshot`];
      this.loadImageIntoCanvas(this.samplePlates[0]);
    } else {
      this.getSamplePlates();
    }
  }

  private loadImageIntoCanvas(url: string) {
    this.isLoadingSnapshot = true;
    this.cameraMaskService.getPlateCapture(url).subscribe((image: Blob) => {
      const reader = new FileReader();

      reader.onload = () => {
        this.image = new Image();

        this.image.onload = () => {
          this.imageInValidState = true;
          this.scaleFactor = this.image.width / this.targetWidth;

          this.measureDiv.nativeElement.appendChild(this.image);
          const wrh = this.image.width / this.image.height;
          this.imageWidth = this.canvas.nativeElement.width;
          this.imageHeight = this.imageWidth / wrh;

          if (this.imageHeight > this.canvas.nativeElement.height) {
            this.imageHeight = this.canvas.nativeElement.height;
            this.imageWidth = this.imageHeight * wrh;
          }

          if (this.canvas.nativeElement.width > this.imageWidth) {
            this.canvas.nativeElement.width = this.imageWidth;
          }

          if (this.canvas.nativeElement.height > this.imageHeight) {
            this.canvas.nativeElement.height = this.imageHeight;
          }

          this.measureDiv.nativeElement.removeChild(this.image);
          this.ctx.drawImage(this.image, 0, 0, this.imageWidth, this.imageHeight);

          this.loadMaskCoordinates();
        };

        this.image.onerror = () => {
          this.imageInValidState = false;
        };

        this.image.src = reader.result as string;
      };

      reader.readAsDataURL(image);
    });
  }

  private loadMaskCoordinates() {
    this.currentPos = { x: 0, y: 0 };

    if (this.coordinates.length === 0) {
      this.cameraMaskService.getCameraMaskCoordinates(this.camera.id).subscribe((coordinates) => {
        coordinates.forEach((coordinate) => {
          coordinate.x /= this.scaleFactor;
          coordinate.y /= this.scaleFactor;
        });

        this.coordinates = coordinates;
        this.closePolygon();
        this.draw();
        this.isLoadingSnapshot = false;
      });
    } else {
      this.closePolygon();
      this.draw();
      this.isLoadingSnapshot = false;
    }
  }

  public saveMask() {
    const cameraMask = new CameraMask();
    cameraMask.coordinates = [];
    cameraMask.cameraId = this.camera.id;
    this.savingCtx.drawImage(this.image, 0, 0, this.image.width, this.image.height);
    this.savingCtx.fillStyle = 'white';
    this.savingCtx.fillRect(0, 0, this.image.width, this.image.height);

    if (this.coordinates.length > 0) {
      for (const coord of this.coordinates) {
        cameraMask.coordinates.push({
          x: coord.x * this.scaleFactor,
          y: coord.y * this.scaleFactor,
        });
      }

      this.savingCtx.beginPath();
      this.savingCtx.moveTo(cameraMask.coordinates[0].x, cameraMask.coordinates[0].y);

      for (let i = 1; i < cameraMask.coordinates.length; i++) {
        this.savingCtx.lineTo(cameraMask.coordinates[i].x, cameraMask.coordinates[i].y);
      }

      this.savingCtx.closePath();
      this.savingCtx.globalCompositeOperation = 'source-over';
      this.savingCtx.fillStyle = 'black';
      this.savingCtx.fill();
    }

    cameraMask.imageMask = this.savingCanvas.nativeElement.toDataURL('image/png');

    this.cameraMaskService.upsertImageMask(cameraMask).subscribe(() => {
      this.snackbarService.create('Camera mask saved.', SnackBarType.Saved);
    },
    () => {
      this.snackbarService.create('Camera mask failed.', SnackBarType.Error);
    });
  }

  public cancelMask() {
    // do nothing
  }

  private draw() {
    this.ctx.clearRect(0, 0, this.imageWidth, this.imageHeight);
    try {
      this.ctx.drawImage(this.image, 0, 0, this.imageWidth, this.imageHeight);
    } catch {
      this.ctx.reset();
    }

    if (this.coordinates.length > 0) {
      this.ctx.fillStyle = 'rgba(139, 0, 0, 0.2)';
      this.ctx.strokeStyle = '#ffffff';
      this.ctx.lineWidth = this.lineThickness;
      this.ctx.beginPath();
      this.ctx.moveTo(this.coordinates[0].x, this.coordinates[0].y);

      this.coordinates.forEach((point, index) => {
        this.ctx.lineTo(point.x, point.y);
        if (index === this.coordinates.length - 1 && !this.isClosed && this.currentPos) {
          this.ctx.lineTo(this.currentPos.x, this.currentPos.y);
        }
      });

      if (this.isClosed) {
        this.ctx.closePath();
        this.ctx.fill();
      }

      this.ctx.stroke();

      this.coordinates.forEach((point, index) => {
        let tempDotRadius = this.dotRadius;
        if (!this.isClosed
          && this.coordinates.length > 1
          && index == 0
          && this.isNearPoint(index, this.currentPos)) {
          tempDotRadius *= 2;
        }

        if (this.isClosed
          && !this.isDragging
          && this.findClosestPoint(this.currentPos) == index
          && this.isNearPoint(index, this.currentPos)) {
          tempDotRadius *= 2;
        }

        this.ctx.fillStyle = '#ff0000';
        this.ctx.beginPath();
        this.ctx.arc(point.x, point.y, tempDotRadius, 0, Math.PI * 2);
        this.ctx.fill();
      });

      this.drawSampleMaskImage();
    }
  }

  private getMousePosition(event: MouseEvent) {
    const rect = this.canvas.nativeElement.getBoundingClientRect();

    return {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top,
    };
  }

  private addPoint(x: number, y: number) {
    this.coordinates.push({ x, y });

    this.draw();
  }

  private isNearPoint(point = -1, mospos: Coordinate) {
    if (this.coordinates.length === 0) {
      return false;
    }

    if (point === -1) {
      for (const coord of this.coordinates) {
        const dx = mospos.x - coord.x;
        const dy = mospos.y - coord.y;

        if (dx * dx + dy * dy < this.forgiveness * this.forgiveness) {
          return true;
        }
      }

      return false;
    }

    const dx = mospos.x - this.coordinates[point].x;
    const dy = mospos.y - this.coordinates[point].y;

    return dx * dx + dy * dy < this.forgiveness * this.forgiveness;
  }

  private closePolygon() {
    if (this.coordinates.length > 2) {
      this.isClosed = true;
      this.draw();
      this.drawSampleMaskImage();
    }
  }

  private drawSampleMaskImage() {
    this.sampleCtx.drawImage(this.image, this.imageWidth / 4, this.imageHeight / 4);
    this.sampleCtx.fillStyle = 'white';
    this.sampleCtx.fillRect(0, 0, this.sampleCanvas.nativeElement.width, this.sampleCanvas.nativeElement.height);

    this.sampleCtx.beginPath();
    this.sampleCtx.moveTo(this.coordinates[0].x * this.scaleFactor / 4, this.coordinates[0].y * this.scaleFactor / 4);

    for (let i = 1; i < this.coordinates.length; i++) {
      this.sampleCtx.lineTo(this.coordinates[i].x * this.scaleFactor / 4, this.coordinates[i].y * this.scaleFactor / 4);
    }

    this.sampleCtx.closePath();
    this.sampleCtx.globalCompositeOperation = 'source-over';
    this.sampleCtx.fillStyle = 'black';
    this.sampleCtx.fill();
  }

  public resetCanvas() {
    this.coordinates.length = 0;

    this.isClosed = false;
    this.isDragging = false;

    this.draw();
    this.sampleCtx.clearRect(0, 0, this.image.width, this.image.height);
  }

  public undoLastPoint() {
    if (this.coordinates.length === 0 || this.isClosed) {
      return;
    }

    this.coordinates.pop();
    this.draw();
  }

  public movePoint(index: number, x: number, y: number) {
    this.coordinates[index] = { x, y };
    this.draw();
  }

  public isPointInPolygon(x: number, y: number, poly: Coordinate[]) {
    let isInside = false;

    if (this.isNearPoint(undefined, { x, y })) {
      return false;
    }

    for (let i = 0, j = poly.length - 1; i < poly.length; j = i++) {
      const xi = poly[i].x, yi = poly[i].y;
      const xj = poly[j].x, yj = poly[j].y;

      const intersect = (yi > y) !== (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi;

      if (intersect) {
        isInside = !isInside;
      }
    }
    return isInside;
  }

  private findClosestPoint(mousePos: Coordinate) {
    return this.coordinates.findIndex(p =>
      Math.sqrt((p.x - mousePos.x) ** 2 + (p.y - mousePos.y) ** 2) < this.dotRadius * 2,
    );
  }

  private getSamplePlates() {
    this.cameraMaskService.getPlateCaptures(this.camera.id).subscribe((plates) => {
      this.samplePlates = plates;
      this.loadImageIntoCanvas(this.camera.sampleImageUrl);
    });
  }

  private prepareCanvases() {
    this.ctx = this.canvas.nativeElement.getContext('2d') ?? (() => {
      throw new Error('ctx is null');
    })();
    this.sampleCtx = this.sampleCanvas.nativeElement.getContext('2d') ?? (() => {
      throw new Error('ctx is null');
    })();
    this.savingCtx = this.savingCanvas.nativeElement.getContext('2d') ?? (() => {
      throw new Error('ctx is null');
    })();

    this.savingCtx.canvas.hidden = true;
  }

  private addEventHandlers() {
    this.canvas.nativeElement.addEventListener('mousedown', this.handleMouseDown);
    document.addEventListener('mousemove', this.handleMouseMove);
    this.canvas.nativeElement.addEventListener('mouseup', this.handleMouseUp);
    this.canvas.nativeElement.addEventListener('click', this.handleClick);
    this.canvas.nativeElement.addEventListener('mouseleave', this.handleMouseLeave);
  }

  private readonly handleMouseDown = (event: MouseEvent): void => {
    if (!this.imageInValidState) {
      return;
    }

    const mousePos = this.getMousePosition(event);

    if (this.isClosed && this.isPointInPolygon(mousePos.x, mousePos.y, this.coordinates)) {
      this.dragStartIndex = -1;
      this.dragOffset = { x: mousePos.x, y: mousePos.y };
      this.isDragging = true;
      this.canvas.nativeElement.style.cursor = 'move';
    } else {
      this.dragStartIndex = this.findClosestPoint(mousePos);

      if (this.dragStartIndex !== -1) {
        this.isDragging = true;
      } else {
        this.canvas.nativeElement.style.cursor = 'default';
      }
    }
  };

  private readonly handleMouseMove = (event: MouseEvent): void => {
    if (!this.imageInValidState) {
      return;
    }
    const mousePos = this.getMousePosition(event);

    if (this.coordinates.length === 0) {
      this.currentPos = mousePos;
      return;
    }

    if (this.isDragging && this.dragStartIndex !== -1) {
      this.movePoint(this.dragStartIndex, mousePos.x, mousePos.y);
    } else if (this.isDragging && this.isClosed && this.dragStartIndex === -1) {
      this.moveEntirePolygon(mousePos);
    } else if (!this.isClosed) {
      this.handlePolygonDrawing(mousePos);
    } else {
      this.handleClosedPolygonHover(mousePos);
    }
  };

  private readonly handleMouseUp = (): void => {
    if (!this.imageInValidState) {
      return;
    }
    this.isDragging = false;
    this.dragStartIndex = -1;
    this.dragOffset = { x: 0, y: 0 };
    this.canvas.nativeElement.style.cursor = 'default';
  };

  private readonly handleClick = (event: MouseEvent): void => {
    if (!this.imageInValidState) {
      return;
    }
    if (!this.isClosed && !this.isDragging) {
      const mousePos = this.getMousePosition(event);

      if (this.isNearPoint(0, mousePos)) {
        this.closePolygon();
      } else {
        this.addPoint(mousePos.x, mousePos.y);
      }
    }
  };

  private readonly handleMouseLeave = (): void => {
    if (!this.imageInValidState) {
      return;
    }
    this.isDragging = false;
    this.dragStartIndex = -1;
    this.dragOffset = { x: 0, y: 0 };
    this.currentPos = { x: 0, y: 0 };
    this.canvas.nativeElement.style.cursor = 'default';
    this.draw();
  };

  private moveEntirePolygon(mousePos: { x: number; y: number }): void {
    this.canvas.nativeElement.style.cursor = 'move';
    const dx = mousePos.x - this.dragOffset.x;
    const dy = mousePos.y - this.dragOffset.y;

    this.coordinates.forEach((point) => {
      point.x += dx;
      point.y += dy;
    });

    this.dragOffset = { x: mousePos.x, y: mousePos.y };
    this.draw();
  }

  private handlePolygonDrawing(mousePos: { x: number; y: number }): void {
    this.currentPos = mousePos;
    this.draw();
  }

  private handleClosedPolygonHover(mousePos: { x: number; y: number }): void {
    this.canvas.nativeElement.style.cursor = this.isPointInPolygon(mousePos.x, mousePos.y, this.coordinates) ? 'move' : 'default';
    this.currentPos = mousePos;
    this.draw();
  }
}
