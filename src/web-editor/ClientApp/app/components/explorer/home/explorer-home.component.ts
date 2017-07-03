
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router'
import { Observable } from 'rxjs';
import "rxjs/add/operator/map";
import "rxjs/add/operator/switchMap";

import { ToastrService } from 'ngx-toastr';

import { FileSystemInfo, PathInfo, Link, ServerActionResponse } from '../model';
import { PathMapService, ExplorerDataService } from '../services';
import { ItemActionRequest, MultiActionRequest } from '../file-explorer/file-explorer.component';

@Component({
    templateUrl: 'explorer-home.component.html',
    styleUrls: ['explorer-home.component.css'],
    providers: [PathMapService, ExplorerDataService]
})

export class ExplorerHomeComponent implements OnInit {
    readonly basePath = '/explorer';
    public breadcrumbs: Link[] = [];
    public folderContent: FileSystemInfo[];
    public fileContent: string = "";
    public currentPath: FileSystemInfo;
    private fileSystemEntry$: Observable<FileSystemInfo>;

    constructor(private pathMapService: PathMapService, private explorerService: ExplorerDataService,
        private router: Router, private activeRoute: ActivatedRoute, private toastr: ToastrService) { }

    ngOnInit() {
        console.log('Explorer::Init');
        this.fileSystemEntry$ = this.pathMapService.getFsEntryFromUrl();
        this.fileSystemEntry$
            .switchMap((value) => {
                console.log(`Current fsEntry='${value}'`);
                this.currentPath = value;
                this.breadcrumbs = this.getBreadCrumbs(value);
                const folderEntry = value.isFile ? value.parent : value;
                return this.explorerService.getFolderContent(folderEntry)
            })
            .subscribe((list) => this.folderContent = list);
    }

    private onActivateFsEntry(entry: FileSystemInfo) {
        console.log(`ExplorerHome::onActivate(${entry})`)

        if (entry.isFile) {
            // Open in Editor
            this.explorerService.getFileContent(entry)
                .subscribe(value => this.fileContent = value);
        } else {
            this.explorerService.folderExists(entry).subscribe((result) => {
                if (result)
                    this.router.navigate([entry.name], { relativeTo: this.activeRoute })
                else {
                    this.toastr.error(`Folder '${entry.path}' does not exists`);
                    console.log("Refreshing current content.");
                    this.explorerService.getFolderContent(this.currentPath)
                        .subscribe(list => this.folderContent = list);
                }
            });
        }

    }

    private onNewFsEntry(request: ItemActionRequest<FileSystemInfo>) {
        console.log(`ExplorerHome::onNewEntry ${request.item}`)
        let fullPath = this.currentPath.path.concat(request.item.name);
        let newItem = new FileSystemInfo(fullPath, request.item.isFile);

        this.explorerService.createFileSystemObject(newItem)
            .subscribe(item => {
                request.item = item;
                request.complete();
            });
    }

    private onItemsDelete(request: MultiActionRequest<FileSystemInfo>) {
        console.log(`ExplorerHome::onItemsDelte ${request.requestItems.length}`);
        var fsEntries = request.requestItems.map(value => value.item);

        this.explorerService.deleteFileSystemObjects(this.currentPath.path, fsEntries)
            .subscribe(response => {
                response
                    .map(responseItem => {
                        var actionRequest = new ItemActionRequest<FileSystemInfo>(responseItem.entity);
                        actionRequest.cancel = responseItem.status != 200;

                        return actionRequest;
                    })
                    .filter(requestItem => requestItem.cancel)
                    .forEach((resultItem) => {
                        let originRequestItem = request.requestItems.find(originItem => originItem.item.name === resultItem.item.name)
                        if (originRequestItem) originRequestItem.cancel = true;
                    });
                request.complete();
            });
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