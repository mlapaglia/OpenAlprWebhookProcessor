import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Tag } from './tag';
import { TagsService } from './tags.service';

@Component({
  selector: 'app-tags',
  templateUrl: './tags.component.html',
  styleUrls: ['./tags.component.less']
})
export class TagsComponent implements OnInit {
  public tags: MatTableDataSource<Tag>;
  public isSaving: boolean = false;

  public rowsToDisplay = [
    'name',
    'delete',
  ];

  constructor(private tagsService: TagsService) { }

  ngOnInit(): void {
    this.getTags();
  }

  public getTags() {
    this.tagsService.getTags().subscribe(result => {
      this.tags = new MatTableDataSource<Tag>(result);
    });
  }

  public saveTags() {
    this.tagsService.upsertTags(this.tags.data).subscribe(result => {
      this.getTags();
    });
  }

  public deleteTag(tag: Tag) {
    this.tags.data.forEach((item, index) => {
      if(item === tag) {
        this.tags.data.splice(index,1);
      }
    });

    this.tags._updateChangeSubscription();
  }

  public addTag() {
    this.tags.data.push(new Tag());
    this.tags._updateChangeSubscription();
  }
}
