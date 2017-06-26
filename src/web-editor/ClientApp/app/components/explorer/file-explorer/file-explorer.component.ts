
import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { Link, FileSystemInfo, PathInfo } from '../model';

@Component({
    selector: 'file-explorer',
    templateUrl: 'file-explorer.component.html',
    styleUrls: ['file-explorer.component.css', '../toolbar.css']
})
export class FileExplorerComponent implements OnInit {
    @Input() items: FileSystemInfo[];
    @Output() itemActivate: EventEmitter<FileSystemInfo> = new EventEmitter();

    readonly basePath = '/explorer';
    private currentEntry: FileSystemInfo;

    private selectedIndexes: Set<number> = new Set();

    @ViewChild('newItemArea') newItemSection: ElementRef;
    @ViewChild('newItemInput') newItemInput: ElementRef;
    @ViewChild('listItems') listItemsContainer: ElementRef;
    private waitingNewItemInput = false;

    constructor() { }

    ngOnInit() {
    }

    private onNewItemRequest(itemType) {
        this.newItemInput.nativeElement.placeholder = `enter new ${itemType} name`;
        this.newItemInput.nativeElement.dataType = itemType;
        this.newItemInput.nativeElement.value = '';
        this.newItemSection.nativeElement.style.display = 'block';
        this.newItemInput.nativeElement.focus();
        this.waitingNewItemInput = true;
    }

    private onDeleteRequest() {

    }

    private onSpecialKeys(event) {
        switch (event.code) {
            case 'Enter':
                // Check Input
                const inputValue = this.newItemInput.nativeElement.value;
                if (inputValue)
                    this.checkInput(inputValue);
                break;
            case 'Escape':
                // Cancel input
                this.hideInput();
                break;
        }
    }

    private onNewInputLostFocus() {
        const inputValue = this.newItemInput.nativeElement.value;
        if (!inputValue) {
            this.hideInput();
            return;
        }

        this.checkInput(inputValue);
    }

    private checkInput(value: string) {

        if (!this.waitingNewItemInput) return;
        // validate input

        // create new item
        const entryName = this.newItemInput.nativeElement.value;
        const isFile = this.newItemInput.nativeElement.dataType === 'folder' ? false : true;
        const fsEntry = new FileSystemInfo(PathInfo.fromString(`./${entryName}`), isFile);
        if (this.isItemExists(fsEntry)) {
            const msg = isFile ? 'File' : 'Folder' + ` ${entryName} alreay exists`;
            this.showInputError(msg);
            return;
        }
        this.hideInput();

        // try to find position of item which is greater then new Item
        let itemIndex = this.items.findIndex((item) => {
            if (FileSystemInfo.ascComparer(fsEntry, item) <= 0)
                return true;
            else
                return false;
        });
        console.log(`index=${itemIndex}`)
        if (itemIndex >= 0) {
            this.items.splice(itemIndex, 0, fsEntry);
        }
        else {
            itemIndex = this.items.length;
            this.items.push(fsEntry);
        }
        // Mark new item as selected
        this.selectedIndexes.clear();
        this.selectedIndexes.add(itemIndex);

        // Scroll into view if necessary
        // Execute this action async with delay to allow angular do change detection and update DOM
        setTimeout(() => {
            let htmlElements = this.listItemsContainer.nativeElement.getElementsByClassName('fs-item');
            if (htmlElements && htmlElements.length > itemIndex) {
                htmlElements[itemIndex].scrollIntoView(false); // alignt bottom of the element to the view end
            }
        }, 100);
    }

    private isItemExists(itemSearch: FileSystemInfo): boolean {
        const index = this.items.findIndex((item) => {
            if (item.name == itemSearch.name)
                return true;
            else
                return false;
        });

        return index >= 0;
    }

    private showInputError(message: string) {

    }

    private hideInput() {
        this.newItemSection.nativeElement.style.display = 'none';
        this.newItemInput.nativeElement.value = '';
        this.waitingNewItemInput = false;
    }

    public hasIndexSelected(index) {
        return this.selectedIndexes.has(index);
    }

    public get hasSelection() {
        return this.selectedIndexes.size > 0;
    }

    public onSelectItem(itemIndex, event) {
        if (event.ctrlKey) {
            if (this.selectedIndexes.has(itemIndex))
                this.selectedIndexes.delete(itemIndex);
            else
                this.selectedIndexes.add(itemIndex);
        } else if (event.shiftKey) {
            switch (this.selectedIndexes.size) {
                case 0:
                    this.selectedIndexes.add(itemIndex);
                    break;
                case 1:
                    let index;
                    this.selectedIndexes.forEach(value => index = value);
                    if (itemIndex > index) {
                        for (let i = index + 1; i <= itemIndex; i++) {
                            this.selectedIndexes.add(i);
                        }
                    } else if (itemIndex < index) {
                        for (let i = itemIndex; i < index; i++) {
                            this.selectedIndexes.add(i);
                        }

                    } else {
                        this.selectedIndexes.delete(itemIndex);
                    }
                    break;
                default:
                    let minIndex = this.items.length;
                    this.selectedIndexes.forEach(value => {
                        if (value < minIndex) minIndex = value;
                    });
                    let maxIndex = 0;
                    this.selectedIndexes.forEach(value => {
                        if (value > maxIndex) maxIndex = value;
                    });

                    if (itemIndex < minIndex) {
                        for (let i = itemIndex; i < minIndex; i++)
                            this.selectedIndexes.add(i);
                    }
                    if (itemIndex > maxIndex) {
                        for (let i = maxIndex + 1; i <= itemIndex; i++)
                            this.selectedIndexes.add(i);
                    }
                    if (itemIndex > minIndex && itemIndex < maxIndex) {
                        for (let i = minIndex + 1; i <= itemIndex; i++) {
                            if (!this.selectedIndexes.has(i))
                                this.selectedIndexes.add(i);
                        }
                    }
            }
        } else {
            this.selectedIndexes.clear();
            this.selectedIndexes.add(itemIndex);
        }
    }

    public activateItem(itemIndex, event) {
        const item = this.items[itemIndex];
        console.log(`Item activate:${item}`);
        this.itemActivate.emit(item);
    }
}