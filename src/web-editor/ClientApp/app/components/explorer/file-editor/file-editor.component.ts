
import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';

@Component({
    selector: 'file-editor',
    templateUrl: 'file-editor.component.html',
    styleUrls: ['file-editor.component.css', '../toolbar.css']
})

export class FileEditorComponent implements OnInit, AfterViewInit {
    public sourceText: string = 'some initial text';
    @ViewChild('editor') editorElement;
    private aceEditor: any;

    constructor() { }

    ngOnInit() {

    }

    ngAfterViewInit() {
        let editor = ace.edit('editor');
        editor.setFontSize("1em");
        editor.getSession().getDocument().setValue(this.sourceText);

        this.aceEditor = editor;
    }
}