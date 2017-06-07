# Collaborative file editing solution sample
This is proof of concept solution for collaboraive file editing.

## Features
### 1. Users Authentication. 
Only authenticated users allowed to work with app. Oauth with Google (Github?) will be supported as a primary auth method.
### 2. Files listing. 
Allows users to view files in designated server folder, pick one for view/edit or create a new one. 
For simplicity there will be flat file structure without folders, which might be added in the future.
### 3. Collaborative file editing
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
