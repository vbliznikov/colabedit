
import { Component, Input, OnInit, ViewChild, AfterViewInit, ElementRef } from '@angular/core';
import { OnChanges, SimpleChanges } from '@angular/core';

@Component({
    selector: 'file-editor',
    templateUrl: 'file-editor.component.html',
    styleUrls: ['file-editor.component.css', '../toolbar.css']
})

export class FileEditorComponent implements OnInit, OnChanges, AfterViewInit {
    @Input()
    public content: string = 'some initial text';
    @ViewChild('editor') editorElement: ElementRef;
    private aceEditor: AceAjax.Editor;

    constructor() { }

    ngOnInit() {

    }

    ngOnChanges(changes: SimpleChanges): void {
        if (!this.aceEditor) return;

        if (changes["content"]) {
            const text = changes["content"].currentValue;
            this.aceEditor.getSession().getDocument().setValue(text);
        }
    }

    ngAfterViewInit() {
        let editor = ace.edit(this.editorElement.nativeElement);
        editor.setFontSize("1em");
        editor.getSession().getDocument().setValue(this.content);
        editor.$blockScrolling = Infinity;

        this.aceEditor = editor;
    }
}