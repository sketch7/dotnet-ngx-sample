# dotnet ngx sample

.NET Angular sample - based on .NET official Template

## What does it have?
 - Angular 8.2.x
 - .NET Core 3.0
 - SCSS for styling
 - Navigation/layout
 - Basic theming setup

## Getting started

- `npm start`

### Known issue

- Due to `BootModuleBuilder` is timing out use the following instead:
  1. either use `ChikoAngularCliBuilder` (`Startup.cs`)
  2. OR `npm run startx2`