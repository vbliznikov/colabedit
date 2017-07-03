
import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class WatchNotifyService {
    public afterEditorInit: EventEmitter<AceAjax.Editor> = new EventEmitter();

    constructor() { }

    public editorInitComplete(editor: AceAjax.Editor): void {
        this.afterEditorInit.emit(editor);
    }
}