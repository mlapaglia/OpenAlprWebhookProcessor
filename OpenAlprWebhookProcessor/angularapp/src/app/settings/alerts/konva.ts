// polygon-draw.component.ts

import { Component, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import Konva from 'konva';

@Component({
  selector: 'app-polygon-draw',
  template: `
    <div #container></div>
  `
})
export class PolygonDrawComponent implements AfterViewInit {
  @ViewChild('container', { static: false }) container: ElementRef;

  stage: Konva.Stage;
  layer: Konva.Layer;
  isDrawing: boolean = false;
  lines: Konva.Line[] = [];
  currentLine: Konva.Line;

  ngAfterViewInit() {
    this.stage = new Konva.Stage({
      container: this.container.nativeElement,
      width: 200,
      height: 200,
    });

    this.layer = new Konva.Layer();
    this.stage.add(this.layer);

    this.stage.on('mousedown', (e) => {
      if (!this.isDrawing) {
        this.isDrawing = true;
        this.currentLine = new Konva.Line({
          points: [this.stage.getPointerPosition()!.x, this.stage.getPointerPosition()!.y],
          stroke: 'black',
          strokeWidth: 2,
          lineJoin: 'round',
          draggable: true,
        });
        this.layer.add(this.currentLine);
        this.layer.draw();
      } else {
        this.currentLine.points(this.currentLine.points().concat([this.stage.getPointerPosition()!.x, this.stage.getPointerPosition()!.y]));
        this.layer.batchDraw();
      }
    });

    this.stage.on('mouseup', () => {
      this.isDrawing = false;
      this.lines.push(this.currentLine);
    });
  }
}