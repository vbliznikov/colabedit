import { FileSystemInfo } from './';

export class ServerActionResponse<T> {
    entity: T;
    status: ServerActionResult;
    errors: string[];
}

export enum ServerActionResult {
    Ok = 200,
    ClientFauilure = 400,
    ServerFailure = 500
}