
import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { Link, FileSystemInfo } from '../model';

@Component({
    selector: 'file-explorer',
    templateUrl: 'file-explorer.component.html',
    styleUrls: ['file-explorer.component.css']
})

export class FileExplorerComponent implements OnInit, OnChanges {
    @Input() fileSystemEntry: Observable<FileSystemInfo>;

    readonly basePath = '/explorer';
    private currentEntry: FileSystemInfo;
    public folderContent: Link[] = [];
    constructor() { }

    ngOnChanges(changes: SimpleChanges): void {

    }

    ngOnInit() {
        if (!this.fileSystemEntry) return;
        this.fileSystemEntry.subscribe(value => {
            this.currentEntry = value;
            this.folderContent = this.getfolderContent(value);

        });
    }

    public getfolderContent(fsInfo: FileSystemInfo): Link[] {
        console.log('getfolderContent()');
        const items: Link[] = [];

        let folderPath = this.basePath + '/';
        if (fsInfo.isFile)
            folderPath += fsInfo.parent.path.toString();
        else
            folderPath += fsInfo.path.toString();

        const parentFolderInfo = fsInfo.isFile ? fsInfo.parent.parent : fsInfo.parent;
        if (parentFolderInfo) {
            items.push(<Link>{ path: `${this.basePath}/${parentFolderInfo.path.toString()}`, title: '..' });
        }

        for (let i = 0; i < 3; i++) {
            const name = `Folder${i}`;
            const path = `${folderPath}/${name}`;
            const link: Link = { path: `${path}`, title: name };
            items.push(link);
        }

        for (let i = 0; i < 2; i++) {
            const name = `file${i}.json`;
            const path = `${folderPath}`;
            const link: Link = { path: `${path}`, title: name, query: { file: name } };
            items.push(link);
        }

        return items;
    }
}