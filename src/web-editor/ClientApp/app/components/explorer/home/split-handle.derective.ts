
import { Directive, ElementRef, Input, OnInit, OnDestroy } from '@angular/core';

@Directive({ selector: '[splitHandle]' })
export class SplitHandleDirective implements OnInit, OnDestroy {
    @Input('left') leftContainer;
    @Input('right') rightContainer;
    //Min width of the left container
    @Input() minWidth: number;
    private hostElement;
    private startX: number;
    private startWidth;

    constructor(private hostElementRef: ElementRef) {
        this.hostElement = hostElementRef.nativeElement;

        // Ensure that event handlers will have this pointer to the derective class object, not to the eventSource element.
        this.onMouseDown = this.onMouseDown.bind(this);
        this.onMouseUp = this.onMouseUp.bind(this);
        this.onMouseMove = this.onMouseMove.bind(this);
    }

    onMouseDown(event) {
        event.preventDefault = true;
        this.hostElement.parentElement.addEventListener('mouseup', this.onMouseUp, true);
        this.hostElement.parentElement.addEventListener('mousemove', this.onMouseMove, true);

        this.startX = event.clientX;
        this.startWidth = this.leftContainer.scrollWidth;
        this.hostElement.style.left = '0px';
        this.hostElement.style.position = "relative";
    }

    onMouseUp(event) {
        event.preventDefault = true;
        this.hostElement.parentElement.removeEventListener('mousemove', this.onMouseMove, true);
        this.hostElement.parentElement.removeEventListener('mouseup', this.onMouseUp, true);

        const leftWidth = Math.max(this.getLeftWidth(event.clientX), this.minWidth);
        this.leftContainer.style.width = `${leftWidth}px`;
        this.rightContainer.style.width = `calc(100% - ${leftWidth}px)`;

        this.hostElement.style.left = '0px';
        this.hostElement.style.position = "static";
    }

    getLeftWidth(currentX): number {
        return this.startWidth + (currentX - this.startX);
    }

    onMouseMove(event) {
        event.preventDefault = true;
        if (this.minWidth && this.getLeftWidth(event.clientX) <= this.minWidth)
            return false;

        this.hostElement.style.left = (event.clientX - this.startX) + 'px';
    }

    ngOnInit(): void {
        this.hostElement.addEventListener('mousedown', this.onMouseDown, true);
    }

    ngOnDestroy(): void {
        this.hostElement.removeEventListener('mousedown', this.onMouseDown, true);
        this.hostElement.parentElement.removeEventListener('mouseup', this.onMouseUp, true);
        this.hostElement.parentElement.removeEventListener('mousemove', this.onMouseMove, true);
    }
}