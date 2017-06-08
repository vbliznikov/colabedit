import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';


import { AppComponent, HomeComponent, NotFoundComponent, TopMenuComponent } from './components'
import { ExplorerHomeComponent, FileEditorComponent, FileExplorerComponent } from './components/explorer';
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
        CounterComponent,
        FetchDataComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent, data: { title: "Home", icon: 'glyphicon-home' } },
            { path: 'counter', component: CounterComponent, data: { title: "Counter", icon: 'glyphicon-education' } },
            { path: 'fetch-data', component: FetchDataComponent, data: { title: "Fetch Data", icon: 'glyphicon-th-list' } },
            {
                path: 'explorer', component: ExplorerHomeComponent,
                children:
                [
                    { path: '', component: FileExplorerComponent, outlet: 'left-nav' },
                    { path: '', component: FileEditorComponent }
                ]
            },
            { path: '**', component: NotFoundComponent }
        ])
    ]
})
export class AppModule {
}
