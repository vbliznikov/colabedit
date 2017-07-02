
import { Injectable } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { ToastrService } from 'ngx-toastr';

import { FileSystemInfo, PathInfo } from '../model';

@Injectable()
export class ExplorerDataService {
    readonly apiBasePath = "/api/explorer";
    private acceptJsonHeader: Headers = new Headers({ Accept: "application/json;charset:utf-8" });
    private acceptTextHeader: Headers = new Headers({ Accept: "text/plain;charset:utf-8" });

    constructor(private http: Http, private toastr: ToastrService) { }

    public getFolderContent(folder: FileSystemInfo): Observable<FileSystemInfo[]> {
        const url = `${this.apiBasePath}/folder/${folder.path.toString()}`;

        return this.http.get(url, { headers: this.acceptJsonHeader })
            .map(response => response.json() as FileSystemInfo[])
            .catch((err) => {
                console.error(err);
                this.toastr.error(err);
                return Observable.of();
            });
    }

    public getFileContent(file: FileSystemInfo): Observable<string> {
        const url = `${this.apiBasePath}/file/${file.path.toString()}`;

        return this.http.get(url, { headers: this.acceptTextHeader })
            .map(response => response.text())
            .catch((err) => {
                console.error(err);
                this.toastr.error(err);
                return Observable.of();
            });
    }
}