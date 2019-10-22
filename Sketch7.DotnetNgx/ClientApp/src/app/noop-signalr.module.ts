import { NgModule } from "@angular/core";
import { HubConnectionFactory, HubConnectionOptions, HubConnection, ConnectionState, ConnectionStatus } from "@ssv/signalr-client";

import { Observable, of, BehaviorSubject } from "rxjs";

const disconnectedState = Object.freeze<ConnectionState>({ status: ConnectionStatus.disconnected });
// todo: create interface + move to lib
export class NoopHubConnectionFactory {

	create(..._connectionOptions: HubConnectionOptions[]): this { return this; }
	get<THub>(_key: string): HubConnection<THub> {
		return NoopHubConnection.Empty as HubConnection<THub>;
	}
	remove(_key: string): void { /*stub*/ }
	connectAll(): void { /*stub*/ }
	disconnectAll(): void { /*stub*/ }
}

export class NoopHubConnection<THub = any> {

	static Empty = new NoopHubConnection(undefined);

	readonly connectionState$: Observable<ConnectionState> = new BehaviorSubject(disconnectedState);
	readonly key: string = "";

	constructor(_connectionOption: HubConnectionOptions | undefined) { /*stub*/ }
	connect(_data?: () => { [key: string]: string }): Observable<void> {
		return of();
	}
	disconnect(): Observable<void> {
		return of();
	}
	setData(_getData: () => { [key: string]: string }): void { /*stub*/ }
	on<TResult>(_methodName: keyof THub): Observable<TResult> {
		return of();
	}
	stream<TResult>(_methodName: keyof THub, ..._args: any[]): Observable<TResult> {
		return of();
	}
	send(_methodName: keyof THub | "StreamUnsubscribe", ..._args: any[]): Observable<void> {
		return of();
	}
	invoke<TResult>(_methodName: keyof THub, ..._args: any[]): Observable<TResult> {
		return of();
	}
	dispose(): void { /*stub*/ }
}

@NgModule({
	providers: [
		{ provide: HubConnectionFactory, useClass: NoopHubConnectionFactory }
	],
})
export class NoopSignalrClientModule {

}