import { Component, ApplicationRef } from "@angular/core";

@Component({
	selector: "app-root",
	templateUrl: "./app.component.html",
	styleUrls: ["./app.component.scss"]
})
export class AppComponent {

	title = "DotnetNgx";

	constructor(
		appRef: ApplicationRef
	) {
		appRef.isStable.pipe(
			// first(stable => stable)
		).subscribe(x => console.warn("stable change", x));
	}

}