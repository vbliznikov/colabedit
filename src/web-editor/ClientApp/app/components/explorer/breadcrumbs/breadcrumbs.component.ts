
import { Component, Input } from '@angular/core';

import { Link } from '../model';


@Component({
    selector: 'explorer-breadcrumbs',
    templateUrl: 'breadcrumbs.component.html',
    styles: [`.breadcrumb {margin-bottom: 0px;}`]
})

export class BreadcrumbsComponent {
    @Input() pathItems: Link[];

    constructor() { }
}