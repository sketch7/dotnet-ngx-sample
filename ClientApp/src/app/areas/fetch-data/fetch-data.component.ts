import { Component, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

interface WeatherForecast {
	dateFormatted: string;
	temperatureC: number;
	temperatureF: number;
	summary: string;
}

@Component({
	selector: "app-fetch-data",
	templateUrl: "./fetch-data.component.html"
})
export class FetchDataComponent {

	forecasts: WeatherForecast[] | undefined;

	constructor(
		http: HttpClient,
		@Inject("BASE_URL") baseUrl: string
	) {
		http.get<WeatherForecast[]>(baseUrl + "api/SampleData/WeatherForecasts").subscribe(result => {
			this.forecasts = result;
		}, error => console.error(error));
	}

}
