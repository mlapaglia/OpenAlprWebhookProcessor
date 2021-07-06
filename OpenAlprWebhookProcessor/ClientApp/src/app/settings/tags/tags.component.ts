import { Component, OnInit } from '@angular/core';
import { Tag } from './tag';
import { TagsService } from './tags.service';

@Component({
  selector: 'app-tags',
  templateUrl: './tags.component.html',
  styleUrls: ['./tags.component.less']
})
export class TagsComponent implements OnInit {
  public tags: Tag[] = [];
  
  constructor(private tagsService: TagsService) { }

  ngOnInit(): void {
    this.getTags();
  }

  public getTags() {
    this.tagsService.getTags().subscribe(result => {
      this.tags = result;
    });
  }

  public upsertTags() {
    this.tagsService.upsertTags(this.tags).subscribe(result => {
      this.getTags();
    });
  }
}
