# Collaborative file editing solution sample
This is proof of concept solution for collaboraive file editing.

## Planned Features
### 1. Users Authentication [planned]. 
Only authenticated users allowed to work with app. ~Oauth with Google (Github?) will be supported as a primary auth method~. Builtin auth for the first version would be preferable to simplify testing and demo.
### 2. Files listing [done]. 
Allows users to view files in designated server folder, pick one for view/edit or create a new one. 
For simplicity there will be flat file structure without folders, which might be added in the future.
### 3. Collaborative file editing [in-progress]
1. Multiple users simultaneously editing single file(document) should be supported. 
1. File format is a plain text format, perhaps with some markdown features [optional].
1. At the top of the document list of users (avatars) currently working on document should be displayed.
1. There is no __Save__ button, changes are saved immediately after user stops typing text (a few seconds timeout).
1. All users should instantly see changes in the document from other collaborators.
1. Collaborators should see **what others are working on** (location of other users' cursors and changes they have made).
### 4. Non functional requirements
1. Solution should be implemented as a Web application
1. Backend should be implemented using .Net
1. Frontend technologies might be whatever would appropriate
1. Solution should work in major versions of modern browsers... or whatever reasonable restrictions will come into play.

## Instalation instructions
1. Clone repository: `git clone git@github.com:vbliznikov/colabedit.git`
1. Install npm dependencies: `cd src/web-editor`, `yarn` or `npm install`. _(should be run inside src/web-editor)_
1. Generate vendor bundle with all dependencies: `npm run update-vendor-bundle`.  _(should be run inside src/web-editor)_
1. Restore nuget packages: `dotnet restore`.  _(should be run inside src/web-editor)_
1. Run dev server: `dotnet run` | `dotnet watch run`.  _(should be run inside src/web-editor)_

## Backend tests
1. Go to tests folder `cd src/web-editor.test`
1. Restore nuget packages: `dotnet restore`. _(should be run inside src/web-editor.test)_
1. Run tests: `dotnet test` | `dotnet watch test`. _(should be run inside src/web-editor.test)_