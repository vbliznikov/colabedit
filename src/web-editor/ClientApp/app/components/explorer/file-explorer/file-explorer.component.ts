
import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'file-explorer',
    templateUrl: 'file-explorer.component.html',
    styleUrls: ['file-explorer.component.css']
})

export class FileExplorerComponent implements OnInit {
    @Input() public path: string;
    constructor() { }

    ngOnInit() {

    }
}