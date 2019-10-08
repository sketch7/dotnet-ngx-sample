# dotnet ngx sample

.NET Angular sample - based on .NET official Template

## What does it have?
 - Angular CLI 8.0 template
 - .NET Core 2.2
 - SCSS for styling
 - Font Awesome and Bootstrap 4.x (no javascript, just styles)
 - Navigation/layout
 - Basic theming setup

## Getting started

- `npm start`

### Known issue

- Due to `BootModuleBuilder` is timing out use the following instead:
  1. either use `ChikoAngularCliBuilder` (`Startup.cs`)
  2. OR `npm run startx2`