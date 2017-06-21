import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { ToastrModule } from 'ngx-toastr';


import { AppComponent, HomeComponent, NotFoundComponent, TopMenuComponent } from './components'
import { ExplorerHomeComponent, FileEditorComponent, FileExplorerComponent, BreadcrumbsComponent, SplitHandleDirective } from './components/explorer';
import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { CounterComponent } from './components/counter/counter.component';

@NgModule({
    bootstrap: [AppComponent],
    declarations: [
        AppComponent,
        NotFoundComponent,
        HomeComponent,
        TopMenuComponent,
        ExplorerHomeComponent,
        FileEditorComponent,
        FileExplorerComponent,
        BreadcrumbsComponent,
        SplitHandleDirective,
        CounterComponent,
        FetchDataComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        ToastrModule.forRoot(),
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent, data: { title: "Home", icon: 'glyphicon-home' } },
            { path: 'counter', component: CounterComponent, data: { title: "Counter", icon: 'glyphicon-education' } },
            { path: 'fetch-data', component: FetchDataComponent, data: { title: "Fetch Data", icon: 'glyphicon-th-list' } },
            {
                path: 'explorer',
                children:
                [
                    { path: '**', component: ExplorerHomeComponent }
                ]
            },
            { path: '**', component: NotFoundComponent }
        ])
    ]
})
export class AppModule {
}
