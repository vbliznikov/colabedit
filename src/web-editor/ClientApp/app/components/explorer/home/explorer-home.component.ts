
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ToastrService } from 'ngx-toastr';

import { FileSystemInfo, PathInfo, Link } from '../model';
import { PathMapService } from '../services/path-map.service';

@Component({
    templateUrl: 'explorer-home.component.html',
    styleUrls: ['explorer-home.component.css'],
    providers: [PathMapService]
})

export class ExplorerHomeComponent implements OnInit {
    readonly basePath = '/explorer';
    breadcrumbs: Link[] = [];
    private fileSystemEntry$: Observable<FileSystemInfo>;

    constructor(private pathMapService: PathMapService, private toastr: ToastrService) { }

    ngOnInit() {
        this.fileSystemEntry$ = this.pathMapService.getFsEntryFromUrl();
        this.fileSystemEntry$.subscribe((value) => {
            console.log(`Resulting entry='${value}'`);
            this.breadcrumbs = this.getBreadCrumbs(value);
        });
        this.toastr.success('Init complete', 'Explorer');
    }

    public getBreadCrumbs(fsInfo: FileSystemInfo): Link[] {
        console.log(`'ExplorerHome:' getBreadCrumbs()`);
        const breabcrumbs: Link[] = [];
        let path = this.basePath;
        const lastFolderPath = fsInfo.isFile ? fsInfo.parent.path.parts : fsInfo.path.parts

        for (let pathPart of lastFolderPath) {
            path += `/${pathPart}`;
            const link: Link = { path: path, title: pathPart };
            breabcrumbs.push(link);
        }

        return breabcrumbs;
    }
}