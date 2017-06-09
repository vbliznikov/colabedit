
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    templateUrl: 'explorer-home.component.html',
    styleUrls: ['explorer-home.component.css']
})

export class ExplorerHomeComponent implements OnInit {
    path: string;
    constructor(private router: Router, private activeRoute: ActivatedRoute) { }

    ngOnInit() {
        this.activeRoute.queryParams.subscribe((params) => {
            const path = params['path'];

            if (!path) {
                let url = this.getUrlFromRoute(this.activeRoute);
                url = `/${url}?path=.`;
                this.router.navigateByUrl(url)
                    .then((result) => console.log(`Navigated to ${url}: ${result}`));
            } else {
                this.path = this.normalizePath(path);
            }
        });
    }

    private normalizePath(pathValue: string): string {
        let path = pathValue.trim();
        //TODO: Implement string normalization
        return path;
    }
    private getUrlFromRoute(route: ActivatedRoute): string {
        return route.snapshot.url.toString();
    }
}