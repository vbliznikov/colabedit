
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router'
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
    public breadcrumbs: Link[] = [];
    public folderContent: FileSystemInfo[];
    public currentPath: FileSystemInfo;
    private fileSystemEntry$: Observable<FileSystemInfo>;

    constructor(private pathMapService: PathMapService, private router: Router, private activeRoute: ActivatedRoute,
        private toastr: ToastrService) { }

    ngOnInit() {
        console.log('Explorer::Init');
        this.fileSystemEntry$ = this.pathMapService.getFsEntryFromUrl();
        this.fileSystemEntry$.subscribe((value) => {
            console.log(`Current fsEntry='${value}'`);
            this.currentPath = value;
            this.breadcrumbs = this.getBreadCrumbs(value);
            this.folderContent = this.getFolderContent(value);
        });
    }

    private onActivateFsEntry(entry: FileSystemInfo) {
        console.log(`ExplorerHome::onActivate(${entry})`)

        if (entry.isFile) {
            // Open in Editor
            // const currentfolderPath = this.currentPath.isFile ? this.currentPath.parent.name : this.currentPath.name;
            // this.router.navigate([`../${currentfolderPath}`], { queryParams: { 'file': entry.name }, relativeTo: this.activeRoute })
        } else
            this.router.navigate([entry.name], { relativeTo: this.activeRoute });

    }

    private getFolderContent(fsInfo: FileSystemInfo): FileSystemInfo[] {
        // TODO: Request items from Server
        const items: FileSystemInfo[] = [];

        for (let i = 1; i <= 5; i++) {
            const folder = new FileSystemInfo(PathInfo.fromString(`./Folder-${i}`));
            items.push(folder);
        }
        for (let i = 1; i <= 5; i++) {
            const file = new FileSystemInfo(PathInfo.fromString(`./file${i}`), true);
            items.push(file);
        }

        return items;
    }

    private getBreadCrumbs(fsInfo: FileSystemInfo): Link[] {
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