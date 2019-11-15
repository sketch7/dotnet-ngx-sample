import { Component } from "@angular/core";

@Component({
	selector: "app-counter-component",
	templateUrl: "./counter.component.html"
})
export class CounterComponent {

	currentCount = 0;

	incrementCounter() {
		this.currentCount++;
	}

}
