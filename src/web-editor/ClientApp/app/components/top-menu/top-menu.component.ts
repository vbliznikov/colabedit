import { Component, OnInit } from '@angular/core';
import { Router, Route } from '@angular/router';

@Component({
    selector: 'top-menu',
    templateUrl: 'top-menu.component.html',
    styleUrls: ['top-menu.component.css']
})

export class TopMenuComponent implements OnInit {
    public links: [any] = [0];

    constructor(private router: Router) { }

    ngOnInit() {
        for (let route of this.router.config) {
            if (!route.data) continue;

            let link = {
                path: route.path,
                title: (route.data.title) ? route.data.title : '[Missing Title]',
                class: (route.data.icon) ? route.data.icon : ''
            };
            this.links.push(link);
        }
    }
}