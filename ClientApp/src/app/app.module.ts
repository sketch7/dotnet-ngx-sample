import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
// import { ServiceWorkerModule } from "@angular/service-worker";
import { CommandModule } from "@ssv/ngx.command";
import { SsvUxModule } from "@ssv/ngx.ux";
// import { HubConnectionFactory } from "@ssv/signalr-client";

import { AppRoutingModule } from "./app-routing.module";

// import { environment } from "../environments/environment";
import { AppComponent } from "./app.component";
import { AREAS_COMPONENTS } from "./areas/index";
import { AppSharedModule } from "./shared";


@NgModule({
	declarations: [AppComponent, ...AREAS_COMPONENTS],
	imports: [
		BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
		HttpClientModule,
		FormsModule,
		AppRoutingModule,
		AppSharedModule,
		CommandModule.forRoot(),
		SsvUxModule.forRoot({
			viewport: { resizePollingSpeed: 66 }
		}),
		// ServiceWorkerModule.register("ngsw-worker.js", { enabled: environment.production }),
	],
	// providers: [HubConnectionFactory],
	bootstrap: [AppComponent],
})
export class AppModule {

	// constructor(
	// 	factory: HubConnectionFactory
	// ) {
	// 	factory.create(
	// 		{ key: "hero", endpointUri: "/hero" },
	// 		{ key: "user", endpointUri: "/userNotifications" }
	// 	);
	// }

}
