
import { Directive, OnInit, OnDestroy, ElementRef } from '@angular/core';

import { WatchNotifyService } from './watch-notify.service';

@Directive({ selector: '[editor-watch]' })
export class WatchNotifyDirective implements OnInit, OnDestroy {
    private aceEditor: AceAjax.Editor;
    private currentActivity: boolean;
    private inactivityCount = 0;
    private timerId;

    constructor(private hostElementRef: ElementRef, private watchService: WatchNotifyService) {

    }

    ngOnInit(): void {
        this.watchService.afterEditorInit
            .asObservable()
            .subscribe((editor) => {
                this.aceEditor = editor;
                this.addActiveLineDecorator("fa-eye");
                // editor.on('change', (e) => this.onDocumentChangeActivity(e));

                // this.timerId = setInterval(() => this.checkActivity(), 500);
            });
    }

    ngOnDestroy() {
        if (this.timerId)
            clearInterval(this.timerId);
    }

    private checkActivity() {
        if (this.currentActivity) {
            this.inactivityCount = 0;
            return;
        }
    }

    private onDocumentChangeActivity(event) {
        this.currentActivity = true;
    }
    private addActiveLineDecorator(decorator) {
        let activeLineDiv;
        let elements = this.hostElementRef.nativeElement.getElementsByClassName("ace_gutter-active-line");
        if (elements && elements.length > 0) {
            activeLineDiv = elements[0];
            activeLineDiv.classList.add("fa");
            activeLineDiv.classList.add(decorator);
        } else {
            console.log(`ace_gutter-active-line div not found.`)
        }
    }
}