
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
        // TODOl Check for existence

        // Create new item
        if (!this.waitingNewItemInput) return;

        const entryName = this.newItemInput.nativeElement.value;
        const isFile = this.newItemInput.nativeElement.dataType === 'folder' ? false : true;
        const fsEntry = new FileSystemInfo(PathInfo.fromString(`./${entryName}`), isFile);
        this.items.push(fsEntry);
        this.items.sort(FileSystemInfo.ascComparer);
        this.hideInput();

        const itemIndex = this.items.indexOf(fsEntry);
        // Mark new item as selected
        this.selectedIndexes.clear();
        this.selectedIndexes.add(itemIndex);

        // Scroll into view if necessary
        let htmlElements = this.listItemsContainer.nativeElement.getElementsByClassName('fs-item');
        if (htmlElements && htmlElements.length > itemIndex) {
            console.log(`Scroll into view ${itemIndex}/${htmlElements.length}`)
            htmlElements[itemIndex].scrollIntoView(false);
        }
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
            console.log(this.selectedIndexes.size);
            this.selectedIndexes.add(itemIndex);
        }
    }

    public activateItem(itemIndex, event) {
        const item = this.items[itemIndex];
        console.log(`Item activate:${item}`);
        this.itemActivate.emit(item);
    }
}