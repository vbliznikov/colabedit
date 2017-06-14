
import { Injectable } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import "rxjs/add/operator/map";
import "rxjs/add/operator/switchMap";
import "rxjs/add/operator/debounceTime";

import { FileSystemInfo, PathInfo, FileSystemEntryBuilder } from '../../../model';

@Injectable()
export class PathMapService {

    constructor(private activeRoute: ActivatedRoute) { }

    public getFsEntryFromUrl(): Observable<FileSystemInfo> {
        return this.activeRoute.url
            .map((segments) => new PathInfo(segments.map(value => value.path)))
            .switchMap((folderPath) =>
                this.activeRoute.queryParams
                    .map(params => params['file'])
                    .map(fileName => this.createFsEntry(folderPath, fileName))
            )
            .debounceTime(50);
    }

    createFsEntry(folderPath: PathInfo, fileName: string): FileSystemInfo {

        if (!fileName)
            return new FileSystemInfo(folderPath, false);

        const fullPathInfo = folderPath.concat(fileName);
        return new FileSystemInfo(fullPathInfo, true);
    }
}