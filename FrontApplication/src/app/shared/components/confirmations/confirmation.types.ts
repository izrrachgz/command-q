import { Observable } from 'rxjs';

export type ConfirmationActionResult = unknown | Promise<unknown> | Observable<unknown>;
export type ConfirmationAction = () => ConfirmationActionResult;
export type ConfirmationValueAction = (value: string) => ConfirmationActionResult;
export type ConfirmationSuccessCallback = () => void;
export type ConfirmationCancelCallback = () => void;
export type ConfirmationFailureCallback = (error: unknown) => void;
