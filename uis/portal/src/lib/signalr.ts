import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

let connection: HubConnection | null = null;

export const startConnection = async (path: string): Promise<HubConnection> => {
    if (!connection) {
        connection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_PORTAL_BASEURL}${path.startsWith('/') ? '' : '/'}${path}`)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        try {
            await connection.start();
            console.log('SignalR Connected.');
        } catch (err) {
            console.error('SignalR Connection Error:', err);
        }
    }

    return connection;
};

export const getConnection = (): HubConnection | null => {
    return connection;
};

