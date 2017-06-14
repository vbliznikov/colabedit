
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { FileSystemInfo, PathInfo } from '../../../model';
import { PathMapService } from '../services/path-map.service';

@Component({
    templateUrl: 'explorer-home.component.html',
    styleUrls: ['explorer-home.component.css'],
    providers: [PathMapService]
})

export class ExplorerHomeComponent implements OnInit {
    path: string;
    private fileSystemEntry$: Observable<FileSystemInfo>;

    constructor(private pathMapService: PathMapService) { }

    ngOnInit() {
        this.fileSystemEntry$ = this.pathMapService.getFsEntryFromUrl();
        this.fileSystemEntry$.subscribe((value) => console.log(`Resulting entry='${value}'`));
    }
}