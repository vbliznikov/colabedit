
import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { Link, FileSystemInfo } from '../model';

@Component({
    selector: 'file-explorer',
    templateUrl: 'file-explorer.component.html',
    styleUrls: ['file-explorer.component.css', '../toolbar.css']
})
export class FileExplorerComponent implements OnInit {
    @Input() fileSystemEntry: Observable<FileSystemInfo>;
    @Input() items: FileSystemInfo[];
    @Output() itemActivate: EventEmitter<FileSystemInfo> = new EventEmitter();

    readonly basePath = '/explorer';
    private currentEntry: FileSystemInfo;

    private selectedItemIndex;
    @ViewChild('newItemArea') newItemSection: ElementRef;
    @ViewChild('newItemInput') newItemInput: ElementRef;

    constructor() { }

    ngOnInit() {
    }

    private onNewItemRequest(itemType) {
        this.newItemInput.nativeElement.placeholder = `enter new ${itemType} name`;
        this.newItemInput.nativeElement.value = '';
        this.newItemSection.nativeElement.style.display = 'block';
        this.newItemInput.nativeElement.focus();
    }

    private onDeleteRequest() {

    }

    private checkSpecialKeys(event) {
        switch (event.code) {
            case 'Enter':
                // Check Input
                if (this.newItemInput.nativeElement.value)
                    this.checkInput();
                break;
            case 'Escape':
                // Cancel input
                this.hideInput();
                break;
        }
    }

    private checkInput() {
        // Check for existence
        // Create new item

        this.hideInput();
    }

    private hideInput() {
        this.newItemSection.nativeElement.style.display = 'none';
        // this.newItemInput.nativeElement.value = '';
    }

    private selectItem(itemIndex, event) {
        if (this.selectedItemIndex === itemIndex)
            this.selectedItemIndex = undefined;
        else {
            this.selectedItemIndex = itemIndex;
        }
    }

    public activateItem(itemIndex, event) {
        const item = this.items[itemIndex];
        console.log(`Item activate:${item}`);
        this.itemActivate.emit(item);
    }
}