
import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { ToastrService } from 'ngx-toastr';

import { FileSystemInfo, PathInfo, ServerActionResponse, ServerActionResult } from '../model';

@Injectable()
export class ExplorerDataService {
    readonly apiBasePath = "/api/explorer";
    private acceptJsonHeader: Headers = new Headers({ Accept: "application/json;charset:utf-8" });
    private acceptTextHeader: Headers = new Headers({ Accept: "text/plain;charset:utf-8" });
    private contentTypeJsonHeader: Headers = new Headers({ "Content-type": "application/json;charset:utf-8" });
    private contentTypeFormUrlEncodedHeader: Headers = new Headers({ "Content-type": "application/x-www-form-urlencoded" });

    constructor(private http: Http, private toastr: ToastrService) { }

    public getFolderContent(folder: FileSystemInfo): Observable<FileSystemInfo[]> {
        const url = `${this.apiBasePath}/folder/${folder.path.toString()}`;

        return this.http.get(url, { headers: this.acceptJsonHeader })
            .map(response => response.json() as FileSystemInfo[])
            .catch((err) => this.handleError(err));
    }

    private handleError(err) {
        console.error(err);
        this.toastr.error(err);
        return Observable.of();
    }

    public folderExists(folder: FileSystemInfo): Observable<boolean> {
        const url = `${this.apiBasePath}/folder/${folder.path.toString()}`;

        return this.http.get(url, { headers: this.acceptJsonHeader })
            .map(response => Observable.of(true))
            .catch((err) => {
                return Observable.of(false);
            });
    }

    public getFileContent(file: FileSystemInfo): Observable<string> {
        const url = `${this.apiBasePath}/file/${file.path.toString()}`;

        return this.http.get(url, { headers: this.acceptTextHeader })
            .map(response => response.text())
            .catch((err) => this.handleError(err));
    }

    public createFileSystemObject(fsEntry: FileSystemInfo): Observable<FileSystemInfo> {
        const url = fsEntry.isFile
            ? `${this.apiBasePath}/file/${fsEntry.parent.path.toString()}`
            : `${this.apiBasePath}/folder/${fsEntry.parent.path.toString()}`;

        const body = `name=${fsEntry.name}`;
        return this.http.post(url, body, { headers: this.contentTypeFormUrlEncodedHeader })
            .map(response => response.json() as FileSystemInfo)
            .catch((err) => this.handleError(err));
    }

    public deleteFileSystemObjects(parentPath: PathInfo, entries: FileSystemInfo[]): Observable<ServerActionResponse<FileSystemInfo>[]> {
        const url = `${this.apiBasePath}/folder/${parentPath.toString()}`;
        const body = JSON.stringify(entries);

        return this.http.delete(url, { body: body, headers: this.contentTypeJsonHeader })
            .map(response => response.json() as ServerActionResponse<FileSystemInfo>[])
            .catch(err => this.handleError(err));
    }
}