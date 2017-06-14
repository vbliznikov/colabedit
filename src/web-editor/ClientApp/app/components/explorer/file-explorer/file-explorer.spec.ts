import { FileExplorerComponent } from './file-explorer.component';
import { SimpleChanges, SimpleChange } from '@angular/core';

describe('Learn undefined behaviour', () => {
    it('Undefined variable is undefined', () => {
        let path;
        expect(path).toBeUndefined('It seems that path was assigned some value.');
    });
    it('Undefined should be avaluated as false', () => {
        let path;

        expect(Boolean(path)).toBeFalsy();
        expect(!Boolean(path)).toBeTruthy();
        expect(path == undefined).toBeTruthy();
    });

    it('Initial path value should be undefined', () => {
        let component = new FileExplorerComponent();
        component.path = undefined;
        component.ngOnChanges({ path: new SimpleChange(undefined, undefined) });
    });

});