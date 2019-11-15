# dotnet ngx sample

.NET Angular sample - based on .NET official Template

## What does it have?
 - Angular 9.x
 - .NET Core 3.0
 - Docker
 - SSR bundled (no need to publish `node_modules`)

## Getting started

- `npm start`

## Publish

- `npm run build`
- To test artifact `npm run serve:publish`

### Known issues

- Due to `BootModuleBuilder` is timing out use the following instead:
  1. either use `ChikoAngularCliBuilder` (`Startup.cs`)
  2. OR `npm run startx2`