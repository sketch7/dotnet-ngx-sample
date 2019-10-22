import { NgModule } from "@angular/core";
import { ServerModule } from "@angular/platform-server";
import { ModuleMapLoaderModule } from "@nguniversal/module-map-ngfactory-loader";
import { AppComponent } from "./app.component";
import { AppModule } from "./app.module";
import { NoopSignalrClientModule } from "./noop-signalr.module";

@NgModule({
	imports: [AppModule, ServerModule, ModuleMapLoaderModule, NoopSignalrClientModule],
	bootstrap: [AppComponent]
})
export class AppServerModule { }
